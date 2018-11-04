﻿// ---------------------------------------------------------------
// <author>Paul Datsyuk</author>
// <url>https://www.linkedin.com/in/pauldatsyuk/</url>
// ---------------------------------------------------------------

using Acr.UserDialogs;
using ExifLib;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PlantHunter.Mobile.Core.Helpers;
using PlantHunter.Mobile.Core.Models;
using PlantHunter.Mobile.Core.Services;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlantHunter.Mobile.Core.ViewModels
{
    public class PhotoDetailsViewModel : MvxViewModel<(ImageSource source, MediaFile photo, Plant plant)>
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly Services.IAppSettings _settings;
        private readonly IUserDialogs _userDialogs;
        private readonly ILocalizeService _localizeService;
        private readonly IApiService _apiService;

        public ImageSource PictureSource { get; set; }
        public MediaFile Photo { get; set; }
        public string PlantName { get; set; }
        public Plant Plant { get; set; }
        public bool UploadButtonVisible { get; set; }

        public PhotoDetailsViewModel(IMvxNavigationService navigationService, IAppSettings settings, IUserDialogs userDialogs, ILocalizeService localizeService, IApiService apiService)
        {
            _navigationService = navigationService;
            _settings = settings;
            _userDialogs = userDialogs;
            _localizeService = localizeService;
            _apiService = apiService;
        }


        public override void Prepare((ImageSource source, MediaFile photo, Plant plant) parameter)
        {
            if(parameter.source != default && parameter.photo != default)
            {
                PictureSource = parameter.source;
                Photo = parameter.photo;
            }
            Plant = parameter.plant;
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            if (Plant != null)
            {
                PictureSource = ImageSource.FromUri(new Uri("https://planthunter-2-dev-as.azurewebsites.net/" + Plant.PlantFileUrl));
                PlantName = Plant.Name;
                UploadButtonVisible = false;
            }
            UploadButtonVisible = true;
        }


        public IMvxAsyncCommand UploadPictureCommand =>
            new MvxAsyncCommand(async () =>
            {
                //First try image;
                (double longitude, double lattitude) = GetPostionExif();

                if(longitude  == 0 || lattitude == 0)
                {
                    var postion = await GetCurrentLocation();
                    if (postion == null)
                        return;
                    longitude = postion.Longitude;
                    lattitude = postion.Latitude;
                }

                var additionalInfo = new AddPictureModel()
                {
                    Longitude = longitude,
                    Latitude = lattitude,
                    Name = PlantName
                };
                if(await _apiService.UploadPictureAsync(Photo.GetStream(), Path.GetExtension(Photo.Path), additionalInfo))
                {
                    _userDialogs.Alert("Uploaded successfully");
                    await _navigationService.Navigate<MainViewModel>();
                }
                else
                {
                    _userDialogs.Alert("Upload failed. Try again, prob not going to work");
                }
            });

        public async Task<Position> GetCurrentLocation()
        {
            Position position = null;
            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 100;

                position = await locator.GetLastKnownLocationAsync();

                if (position != null)
                {
                    //got a cahched position, so let's use it.
                    return position;
                }

                var available = locator.IsGeolocationAvailable;
                var enabled = locator.IsGeolocationEnabled;
                if (!available || !enabled)
                {
                    _userDialogs.Alert("Can't get your location", "Error", "Ok");
                    return null;
                }

                position = await locator.GetPositionAsync(TimeSpan.FromSeconds(20), null, true);

            }
            catch (Exception ex)
            {
                //Display error as we have timed out or can't get location.
            }

            if (position == null)
            {
                _userDialogs.Alert("Can't get your location", "Error", "Ok");
                return null;
            }

            var output = string.Format("Time: {0} \nLat: {1} \nLong: {2} \nAltitude: {3} \nAltitude Accuracy: {4} \nAccuracy: {5} \nHeading: {6} \nSpeed: {7}",
                position.Timestamp, position.Latitude, position.Longitude,
                position.Altitude, position.AltitudeAccuracy, position.Accuracy, position.Heading, position.Speed);

            Debug.WriteLine(output);
            return position;
        }

        public (double longitude, double lattitude) GetPostionExif()
        {
            using (ExifReader reader = new ExifReader(Photo.Path))
            {
                Double[] GpsLongArray;
                Double[] GpsLatArray;
                Double GpsLongDouble;
                Double GpsLatDouble;

                if (reader.GetTagValue<Double[]>(ExifTags.GPSLongitude, out GpsLongArray)
                    && reader.GetTagValue<Double[]>(ExifTags.GPSLatitude, out GpsLatArray))
                {
                    GpsLongDouble = GpsLongArray[0] + GpsLongArray[1] / 60 + GpsLongArray[2] / 3600;
                    GpsLatDouble = GpsLatArray[0] + GpsLatArray[1] / 60 + GpsLatArray[2] / 3600;

                    Console.WriteLine("The picture was taken at {0},{1}", GpsLongDouble, GpsLatDouble);
                    return (-GpsLongDouble, GpsLatDouble); //TODO: fix - on longitude
                }
                return (0, 0);
            }
        }
    }
}