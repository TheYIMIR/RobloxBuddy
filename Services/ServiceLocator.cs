using RobloxBuddy.Models;
using System;
using System.Collections.Generic;

namespace RobloxBuddy.Services
{
    public static class ServiceLocator
    {
        private static Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Initialize()
        {
            // Load settings
            var userSettings = SettingsManager.LoadSettings();
            Register<UserSettings>(userSettings);

            // Initialize services
            var robloxApiService = new RobloxApiService(userSettings);
            Register<RobloxApiService>(robloxApiService);

            var notificationService = new NotificationService();
            Register<NotificationService>(notificationService);
        }

        public static void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }
    }
}