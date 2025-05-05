using RobloxBuddy.Models;
using RobloxBuddy.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;

namespace RobloxBuddy.Pages
{
    /// <summary>
    /// Interaction logic for GamesPage.xaml
    /// </summary>
    public partial class GamesPage : Page
    {
        public GamesPage()
        {
            try
            {
                // Initialize the component - simplest possible version
                InitializeComponent();

                // Show loading message
                if (txtMessage != null)
                {
                    txtMessage.Text = "Games page under construction...";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing GamesPage: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}