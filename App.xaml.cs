using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace TechnoPoss
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            MainPage = serviceProvider.GetRequiredService<AppShell>();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var exception = args.ExceptionObject as Exception;
                System.Diagnostics.Debug.WriteLine($"Unhandled exception: {exception?.ToString()}");
            };
        }
    }
}