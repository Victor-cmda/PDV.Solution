using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using PDV.Domain.Interfaces;
using PDV.Application.Services;
using PDV.Infrastructure.Data;
using PDV.Infrastructure.Data.Contexts;
using PDV.Infrastructure.Services;
using PDV.Infrastructure.Synchronization;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using PDV.Domain.Interfaces.PDV.Domain.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Serilog.Extensions.Logging.File;

using Serilog;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PDV.UI.WinUI3
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Microsoft.UI.Xaml.Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public IServiceProvider Services { get; }

        public App()
        {
            Services = ConfigureServices();
            this.InitializeComponent();
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                try
                {
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                    string logPathAlternative = Path.Combine(desktopPath, "PDV_Logs");

                    if (!Directory.Exists(logPathAlternative))
                    {
                        Directory.CreateDirectory(logPathAlternative);
                        Debug.WriteLine($"Diretório de logs alternativo criado em: {logPathAlternative}");
                    }

                    services.AddLogging(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Information);
                        builder.AddProvider(new FileLoggerProvider(logPathAlternative));
                    });
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"Erro ao criar diretório de logs alternativo: {fallbackEx.Message}");

                    services.AddLogging(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Information);
                        builder.AddSerilog();
                    });
                }
            });

            var localDbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "pdv.db");
            var sqliteConnectionString = $"Data Source={localDbPath}";
            var postgresConnectionString = "Host=localhost;Port=5435;Database=postgres;Username=postgres;Password=1234";

            // Configurar contextos de banco de dados
            services.AddDbContextFactory<LocalDbContext>(options =>
                options.UseSqlite(sqliteConnectionString));

            services.AddDbContextFactory<RemoteDbContext>(options =>
                options.UseNpgsql(postgresConnectionString));

            services.AddSingleton<IConnectivityService, ConnectivityService>();
            services.AddSingleton<ISyncNotificationService, SyncNotificationService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ISyncService, SyncService>();

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await EnsureLocalDatabaseAsync();
            await EnsureRemoteDatabaseAsync();

            MainWindow = new MainWindow();
            MainWindow.Activate();
        }

        private async Task EnsureLocalDatabaseAsync()
        {
            try
            {
                // Obter o caminho do banco de dados
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var dbDirectory = Path.Combine(appDataPath, "PDV");
                var dbPath = Path.Combine(dbDirectory, "pdv.db");

                // Garantir que o diretório exista
                if (!Directory.Exists(dbDirectory))
                {
                    Directory.CreateDirectory(dbDirectory);
                }

                // Verificar se o banco já existe
                bool newDatabase = !File.Exists(dbPath);

                // Inicializar banco de dados
                var contextFactory = Services.GetService<IDbContextFactory<LocalDbContext>>();
                if (contextFactory != null)
                {
                    using var context = await contextFactory.CreateDbContextAsync();

                    // EnsureCreated cria o banco se ele não existir
                    await context.Database.EnsureCreatedAsync();

                    // Se for um banco novo, podemos adicionar dados iniciais
                    //if (newDatabase)
                    //{
                    //    await SeedInitialDataAsync(context);
                    //}
                }

                // Registrar no log que o banco foi inicializado
                var logger = Services.GetService<ILogger<App>>();
                logger?.LogInformation($"Banco de dados local inicializado em: {dbPath}");
            }
            catch (Exception ex)
            {
                // Logar o erro
                var logger = Services.GetService<ILogger<App>>();
                logger?.LogError(ex, "Erro ao criar o banco de dados local");

                // Em modo de desenvolvimento, pode ser útil mostrar o erro
#if DEBUG
                var dialog = new ContentDialog
                {
                    Title = "Erro de Banco de Dados",
                    Content = $"Não foi possível inicializar o banco de dados local: {ex.Message}",
                    CloseButtonText = "OK"
                };

                // Use um método de extensão do WinUI para mostrar o diálogo sem passar o XamlRoot
                await dialog.ShowAsync();
#endif
            }
        }

        private async Task EnsureRemoteDatabaseAsync()
        {
            try
            {
                // Verificar conectividade primeiro
                var connectivityService = Services.GetService<IConnectivityService>();
                if (connectivityService != null)
                {
                    await connectivityService.CheckAndUpdateConnectivityAsync();
                    if (!connectivityService.IsOnline())
                    {
                        var logger = Services.GetService<ILogger<App>>();
                        logger?.LogWarning("Não foi possível verificar o banco remoto - dispositivo offline");
                        return;
                    }
                }

                // Inicializar banco de dados remoto
                var contextFactory = Services.GetService<IDbContextFactory<RemoteDbContext>>();
                if (contextFactory != null)
                {
                    using var context = await contextFactory.CreateDbContextAsync();

                    try
                    {
                        // Verificar se conseguimos conectar
                        bool canConnect = await context.Database.CanConnectAsync();

                        if (canConnect)
                        {
                            // Verificar se o banco tem as tabelas necessárias
                            // No PostgreSQL, podemos verificar a existência de tabelas específicas
                            var tableExists = await context.Database.ExecuteSqlRawAsync(
                                @"SELECT COUNT(*) FROM information_schema.tables 
                        WHERE table_schema = 'public' AND table_name = 'Products'");

                            if (tableExists == 0)
                            {
                                // Tabelas não existem, precisamos criá-las
                                // Em produção, você provavelmente usaria migrações
                                // mas para simplificar, usaremos EnsureCreated
                                await context.Database.EnsureCreatedAsync();

                                // Inserir dados iniciais no banco remoto
                                //await SeedInitialDataAsync(context);
                            }

                            var logger = Services.GetService<ILogger<App>>();
                            logger?.LogInformation("Banco de dados remoto verificado e inicializado");
                        }
                        else
                        {
                            throw new Exception("Não foi possível conectar ao banco de dados PostgreSQL");
                        }
                    }
                    catch (Exception dbEx)
                    {
                        var logger = Services.GetService<ILogger<App>>();
                        logger?.LogError(dbEx, "Erro ao verificar/criar o banco de dados remoto");

                        // Não lançamos a exceção para o chamador para não impedir o aplicativo de iniciar
                    }
                }
            }
            catch (Exception ex)
            {
                // Logar o erro
                var logger = Services.GetService<ILogger<App>>();
                logger?.LogError(ex, "Erro ao inicializar o banco de dados remoto");
            }
        }


        public static Window? MainWindow { get; private set; }
    }
}
