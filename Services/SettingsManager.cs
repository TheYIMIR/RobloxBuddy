using Newtonsoft.Json;
using RobloxBuddy.Models;
using System;
using System.IO;

namespace RobloxBuddy.Services
{
    public static class SettingsManager
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RobloxBuddy",
            "settings.json");

        public static UserSettings LoadSettings()
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Load settings if file exists
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    return JsonConvert.DeserializeObject<UserSettings>(json) ?? new UserSettings();
                }
            }
            catch (Exception ex)
            {
                // Log error in real application
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new UserSettings();
        }

        public static void SaveSettings(UserSettings settings)
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                // Log error in real application
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}