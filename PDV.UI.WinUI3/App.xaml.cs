using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using PDV.Infrastructure;
using PDV.Infrastructure.Services;
using PDV.UI.WinUI3.Helpers;
using PDV.UI.WinUI3.ViewModels;
using PDV.UI.WinUI3.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PDV.UI.WinUI3
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static T GetService<T>() where T : class
        {
            if ((App.Current as App)!.Host.Services is null)
            {
                throw new ArgumentNullException("Host.Services is null");
            }
            return (App.Current as App)!.Host.Services.GetService(typeof(T)) as T ?? throw new ArgumentNullException($"{typeof(T)} not found");
        }

        private Window? m_window;
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider ServiceProvider => _serviceProvider;

        public IHost Host { get; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            Host = BuildHost();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await Host.StartAsync();
            _serviceProvider = Host.Services;
            m_window = Host.Services.GetRequiredService<MainWindow>();
            WindowHelper.MainWindow = m_window;
            m_window.Activate();
        }

        private IHost BuildHost()
        {
            var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    var appLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    config.SetBasePath(appLocation);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Registrar serviços
                    ConfigureServices(services, context.Configuration);
                })
                .Build();

            return host;
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<MainWindow>();

            // Adicionar configuração da infraestrutura
            services.AddInfrastructure(
                configuration.GetConnectionString("PostgresConnection"),
                configuration.GetConnectionString("SqliteConnection"),
                configuration
            );

            services.AddTransient<ProductViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<HomePage>();
            services.AddTransient<ProductPage>();

            // Adicionar serviços de background
            services.AddHostedService<SyncBackgroundService>();
        }

    }
}
