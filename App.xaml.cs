using System;
using System.Windows;

namespace EpicMarkdownManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set up global exception handling
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                MessageBox.Show($"An unexpected error occurred: {args.ExceptionObject}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
        }
    }
}