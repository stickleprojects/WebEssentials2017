﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

using VSIXBundler.Core.Helpers;
using VSIXBundler.Core.Installer;

namespace WebEssentials.Commands
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        private readonly ILogger _logger;

        public LogWindow(ILogger logger)
        {
            _logger = logger;
            InitializeComponent();

            Loaded += (s, e) =>
            {
                Title = Vsix.Name;
                Icon = BitmapFrame.Create(new Uri("pack://application:,,,/WebEssentials;component/Resources/small.png", UriKind.RelativeOrAbsolute));

                IEnumerable<string> logs = InstallerService.Installer.Store.Log.Select(l => l.ToString()).Reverse();
                log.Text = string.Join(Environment.NewLine, logs);

                reset.Content = WebEssentials.Resources.Text.ReInstall;
                close.Content = WebEssentials.Resources.Text.Close;
                ActivityLog.Content = WebEssentials.Resources.Text.ActivityLog;

                reset.Click += ResetClickAsync;
            };
        }

        private async void ResetClickAsync(object sender, RoutedEventArgs e)
        {
            string msg = WebEssentials.Resources.Text.ResetLog;
            MessageBoxResult answer = MessageBox.Show(msg, Vsix.Name, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes)
                return;

            Telemetry.ResetInvoked();
            Close();

            try
            {
                await InstallerService.ResetAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Log(ex.ToString());
            }
        }
    }
}