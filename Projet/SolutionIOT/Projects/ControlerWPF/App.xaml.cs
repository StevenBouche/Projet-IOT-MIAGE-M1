using ControlerWPF.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTLib.Config;
using System.IO;
using System.Windows;

namespace ControlerWPF
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private ServiceProvider ServiceProvider;
        public IConfiguration Configuration { get; private set; }

        public App()
        {
            InitConfiguration();
            InitServices();
        }
        private void InitConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }

        private void InitServices()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
        }

        public void OnStartup(object sender, StartupEventArgs e)
        {

            var mainWindow = ServiceProvider.GetService<MainWindow>();

            mainWindow.ConfigClientMQTT = new MQTTConfigClient();
            mainWindow.ConfigTopicMQTT = new MQTTServiceConfig();
            Configuration.GetSection(nameof(MQTTConfigClient)).Bind(mainWindow.ConfigClientMQTT);
            Configuration.GetSection(nameof(MQTTServiceConfig)).Bind(mainWindow.ConfigTopicMQTT);

            mainWindow.Show();
        }

    }
}
