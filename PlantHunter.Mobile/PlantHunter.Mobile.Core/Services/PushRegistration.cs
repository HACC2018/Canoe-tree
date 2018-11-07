﻿using PlantHunter.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PlantHunter.Mobile.Core.Services
{
    public class PushRegistration : IPushRegistration
    {
        private readonly IAppSettings _appSettings;
        private readonly IApiService _apiService;

        public PushRegistration(IAppSettings appSettings, IApiService apiService)
        {
            _appSettings = appSettings;
            _apiService = apiService;
        }

        public void Init()
        {
            if (string.IsNullOrEmpty(_appSettings.PushRegistrationId))
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var registrationId = await _apiService.GetPushRegistrationId();
                    _appSettings.PushRegistrationId = registrationId;
                    var handle = _appSettings.Handle;

                    var deviceUpdate = new DeviceRegistration()
                    {
                        Handle = handle,
                        Platform = MobilePlatform.wns,
                        Tags = new string[1] { registrationId },
                        DeviceId = Plugin.DeviceInfo.CrossDeviceInfo.Current.Id
                    };

                    var result = await _apiService.EnablePushNotifications(registrationId, deviceUpdate);
                });
              
            }
        }
    }
}