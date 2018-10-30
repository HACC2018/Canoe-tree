﻿// ---------------------------------------------------------------
// <author>Paul Datsyuk</author>
// <url>https://www.linkedin.com/in/pauldatsyuk/</url>
// ---------------------------------------------------------------

using Xamarin.Essentials;

namespace PlantHunter.Mobile.Core.Services
{
    public class AppSettings : IAppSettings
    {
        public const string ApiUrlKey = "ApiUrlKey";
        public const string ApiUrlDefaultValue = "https://planthunter-2-dev-as.azurewebsites.net/";

        public string ApiUrl
        {
            get { return Preferences.Get(ApiUrlKey, ApiUrlDefaultValue); }
            set { Preferences.Set(ApiUrlKey, value); }
        }
    }
}
