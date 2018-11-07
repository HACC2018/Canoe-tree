﻿using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using Microsoft.EntityFrameworkCore;
using PlantHunter.Web.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web.Data;

namespace PlantHunter.Web.NotificationHubs
{
    public class NotificationHubProxy
    {
        private NotificationHubClient _hubClient;
        private readonly ApplicationDbContext _applicationDbContext;

        public NotificationHubProxy(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
            _hubClient = NotificationHubClient.CreateClientFromConnectionString(NotificationHubConfiguration.ConnectionString, NotificationHubConfiguration.HubName);
        }

        /// 
        /// <summary>
        /// Get registration ID from Azure Notification Hub
        /// </summary>
        public async Task<string> CreateRegistrationId()
        {
            return await _hubClient.CreateRegistrationIdAsync();
        }

        /// 
        /// <summary>
        /// Delete registration ID from Azure Notification Hub
        /// </summary>
        /// <param name="registrationId"></param>
        public async Task DeleteRegistration(string registrationId)
        {
            await _hubClient.DeleteRegistrationAsync(registrationId);
        }

        /// 
        /// <summary>
        /// Register device to receive push notifications. 
        /// Registration ID ontained from Azure Notification Hub has to be provided
        /// Then basing on platform (Android, iOS or Windows) specific
        /// handle (token) obtained from Push Notification Service has to be provided
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deviceUpdate"></param>
        /// <returns></returns>
        public async Task<HubResponse> RegisterForPushNotifications(string id, DeviceRegistration deviceUpdate)
        {
            RegistrationDescription registrationDescription = null;

            switch (deviceUpdate.Platform)
            {
                case MobilePlatform.wns:
                    registrationDescription = new WindowsRegistrationDescription(deviceUpdate.Handle);
                    break;
                case MobilePlatform.apns:
                    registrationDescription = new AppleRegistrationDescription(deviceUpdate.Handle);
                    break;
                case MobilePlatform.gcm:
                    registrationDescription = new GcmRegistrationDescription(deviceUpdate.Handle);
                    break;
                default:
                    return new HubResponse().AddErrorMessage("Please provide correct platform notification service name.");
            }

            registrationDescription.RegistrationId = id;
            if (deviceUpdate.Tags != null)
                registrationDescription.Tags = new HashSet<string>(deviceUpdate.Tags);


            try
            {
                await _hubClient.CreateOrUpdateRegistrationAsync(registrationDescription);
                var device = await _applicationDbContext.PushRegistrations.FirstOrDefaultAsync(f => f.DeviceId == deviceUpdate.DeviceId);
                if(device == null)
                {
                    _applicationDbContext.PushRegistrations.Add(new Data.Models.PushRegistration { DeviceId = deviceUpdate.DeviceId, MobilePlatform = deviceUpdate.Platform, Tag = deviceUpdate.Tags.First() });
                }
                else
                {
                    device.Tag = deviceUpdate.Tags.First();
                }
                await _applicationDbContext.SaveChangesAsync();
                return new HubResponse();
            }
            catch (MessagingException)
            {
                return new HubResponse().AddErrorMessage("Registration failed because of HttpStatusCode.Gone. PLease register once again.");
            }
        }

        /// 
        /// <summary>
        /// Send push notification to specific platform (Android, iOS or Windows)
        /// </summary>
        /// <param name="newNotification"></param>
        /// <returns></returns>
        public async Task<HubResponse<NotificationOutcome>> SendNotification(PushNotification newNotification)
        {
            string title = "Plant update";
            string body = newNotification.Content;
            var data = new { action = "Play", userId = 5 };
            var pushSent = PushNotificationLogic.SendPushNotification(newNotification.Tags, title, body, data);

            return new HubResponse<NotificationOutcome>();

            //try
            //{
            //    NotificationOutcome outcome = null;

            //    switch (newNotification.Platform)
            //    {
            //        case MobilePlatform.wns:
            //            if (newNotification.Tags == null)
            //                outcome = await _hubClient.SendWindowsNativeNotificationAsync(newNotification.Content);
            //            else
            //                outcome = await _hubClient.SendWindowsNativeNotificationAsync(newNotification.Content, newNotification.Tags);
            //            break;
            //        case MobilePlatform.apns:
            //            if (newNotification.Tags == null)
            //                outcome = await _hubClient.SendAppleNativeNotificationAsync(newNotification.Content);
            //            else
            //                outcome = await _hubClient.SendAppleNativeNotificationAsync(newNotification.Content, newNotification.Tags);
            //            break;
            //        case MobilePlatform.gcm:
            //            if (newNotification.Tags == null)
            //                outcome = await _hubClient.SendGcmNativeNotificationAsync(newNotification.Content);
            //            else
            //                outcome = await _hubClient.SendGcmNativeNotificationAsync(newNotification.Content, newNotification.Tags);
            //            break;
            //    }

            //    if (outcome != null)
            //    {
            //        if (!((outcome.State == NotificationOutcomeState.Abandoned) ||
            //            (outcome.State == NotificationOutcomeState.Unknown)))
            //            return new HubResponse<NotificationOutcome>();
            //    }

            //    return new HubResponse<NotificationOutcome>().SetAsFailureResponse().AddErrorMessage("Notification was not sent due to issue. Please send again.");
        //}
        }
    }
}
