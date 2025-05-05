using RobloxBuddy.Converters;
using RobloxBuddy.Services;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms; // For NotifyIcon

namespace RobloxBuddy
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon _notifyIcon;
        private bool _isExit;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize services
            ServiceLocator.Initialize();

            // Initialize notifications
            NotificationService.InitializeNotifications();

            MainWindow = new MainWindow();
            MainWindow.Closing += MainWindow_Closing;

            // Initialize system tray icon
            InitializeNotifyIcon();
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();

            try
            {
                // Load icon from application resources
                var iconStream = GetResourceStream(new Uri("pack://application:,,,/Resources/roblox_icon.ico"))?.Stream;
                if (iconStream != null)
                {
                    _notifyIcon.Icon = new System.Drawing.Icon(iconStream);
                }
                else
                {
                    // Fallback to default application icon
                    _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading icon: {ex.Message}");
                // Fallback to system icon
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            _notifyIcon.Text = "RobloxBuddy";
            _notifyIcon.Visible = true;

            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            // Create context menu for the notify icon
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            // Add "Open" menu item
            ToolStripMenuItem openItem = new ToolStripMenuItem("Open");
            openItem.Click += (s, e) => ShowMainWindow();
            contextMenu.Items.Add(openItem);

            // Add separator
            contextMenu.Items.Add(new ToolStripSeparator());

            // Add "Exit" menu item
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => ExitApplication();
            contextMenu.Items.Add(exitItem);

            // Assign context menu to notify icon
            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else
            {
                MainWindow.Show();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Check settings to see if we should minimize to tray instead of closing
            var settings = ServiceLocator.Get<Models.UserSettings>();

            if (!_isExit && settings.MinimizeToTray)
            {
                e.Cancel = true;
                MainWindow.Hide(); // Hide window instead of closing
            }
        }

        private void ExitApplication()
        {
            _isExit = true;

            // Clean up notify icon properly
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            // Close main window
            if (MainWindow != null)
            {
                MainWindow.Close();
            }

            // Shutdown the application
            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Clean up resources
                if (_notifyIcon != null)
                {
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up resources: {ex.Message}");
            }

            base.OnExit(e);
        }
    }
}