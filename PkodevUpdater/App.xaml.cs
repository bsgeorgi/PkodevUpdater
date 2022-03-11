using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using UpdaterLibrary.Extensions;
using UpdaterLibrary.Interfaces;
using UpdaterLibrary.Models;

namespace PkodevUpdater
{
    public partial class App
    {
        private readonly IHost _host;

        public App()
        {
            _host = CreateHostBuilder().Build();
        }
        
        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            using IServiceScope serviceScope = _host.Services.CreateScope();
            var provider = serviceScope.ServiceProvider;

            var appSettings = provider.GetRequiredService<IOptions<AppSettings>>();

            var githubClient = provider.GetRequiredService<IOctokitGithubClientFactory>();
            githubClient.AppSettings = appSettings;

            var updaterWindow = _host.Services.GetRequiredService<UpdaterWindow>();
            updaterWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }
            base.OnExit(e);
        }

        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.Configure<AppSettings>(context.Configuration);
                    services.AddSingleton<UpdaterWindow>();
                    services.AddServiceCollection();
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    var curDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    config.SetBasePath(curDir)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                });
    }
}