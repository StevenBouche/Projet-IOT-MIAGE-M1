using APIRobot.Configs;
using APIRobot.Configs.Database;
using APIRobot.Configs.HostedServices;
using APIRobot.HostedServices;
using APIRobot.Models.Data;
using APIRobot.Services;
using APIRobot.Services.Cache;
using APIRobot.Services.SignalHub;
using APIRobot.SignalHub;
using ConfigPolicy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDBAccess;
using SharedModels.Auth;
using SharedModels.Authorization;

namespace APIRobot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IConfiguration ConfigurationDatabase { get; }
        public IConfiguration ConfigurationHostedServices { get; }
        public IConfiguration ConfigurationServices { get; }
        private PolicyConfig PolicyConfiguration { get; set; }

        public Startup(IWebHostEnvironment env)
        {
            var basePath = $"{ env.ContentRootPath}/AppSettings";

            Configuration = ConfigurationBuilder(basePath, env.EnvironmentName, "");
            ConfigurationServices = ConfigurationBuilder(basePath, env.EnvironmentName, "Services");
            ConfigurationDatabase = ConfigurationBuilder(basePath, env.EnvironmentName, "Database");
            ConfigurationHostedServices = ConfigurationBuilder(basePath, env.EnvironmentName, "HostedServices");
        }

        private static IConfiguration ConfigurationBuilder(string basePath, string environmentName, string name)
        {

            var path = string.IsNullOrEmpty(name) ? basePath : $"{basePath}/{name}";
            var file = string.IsNullOrEmpty(name) ? $"appsettings.{environmentName}.json" : $"appsettings.{name}.{environmentName}.json";

            return new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile(file, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddTransient<IProxyHubEquipment, ProxyHub>();

            ConfigureConfiguration(services);
            ConfigureCacheEquipment(services);
            ConfigureDatabaseServices(services);
            ConfigureServiceAuth(services);
            ConfigureHostedServices(services);

            //Custom policy
            services.AddCustomPolicy(PolicyConfiguration);

            //Add signal r json
            services.AddSignalR()
                .AddMessagePackProtocol();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "APIRobot", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "APIRobot v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCustomPolicy(PolicyConfiguration);

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<EquipmentHub>("/hub/equipment");
                endpoints.MapHub<EquipmentsHub>("/hub/equipments");
                endpoints.MapHub<EquipmentStreamHub>("/hub/equipmentStream");
            });
        }

        private void ConfigureConfiguration(IServiceCollection services)
        {
            //Policy configuration
            PolicyConfiguration = new PolicyConfig();
            Configuration.GetSection(nameof(PolicyConfig)).Bind(PolicyConfiguration);

            services.Configure<CertificateConfig>(Configuration.GetSection(nameof(CertificateConfig)));

            //HostedServices
            services.Configure<TCPServiceConfig>(ConfigurationHostedServices.GetSection(nameof(TCPServiceConfig)));
            services.Configure<MQTTServiceConfig>(ConfigurationHostedServices.GetSection(nameof(MQTTServiceConfig)));

            //Databases
            services.Configure<DataDatabaseSettings>(ConfigurationDatabase.GetSection(nameof(DataDatabaseSettings)));
            services.Configure<UsersDatabaseSettings>(ConfigurationDatabase.GetSection(nameof(UsersDatabaseSettings)));
            services.Configure<AuthorizationDatabaseSettings>(ConfigurationDatabase.GetSection(nameof(AuthorizationDatabaseSettings)));
            
            //Services
            services.Configure<JwtServiceConfig>(ConfigurationServices.GetSection(nameof(JwtServiceConfig)));
            services.Configure<AuthorizationServiceConfig>(ConfigurationServices.GetSection(nameof(AuthorizationServiceConfig)));
            services.Configure<EquipmentsConnectionCacheConfig>(ConfigurationServices.GetSection(nameof(EquipmentsConnectionCacheConfig)));
            
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<DataDatabaseSettings>>().Value);
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<UsersDatabaseSettings>>().Value);
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<AuthorizationDatabaseSettings>>().Value);
        }

        private static void ConfigureDatabaseServices(IServiceCollection services)
        {
            //Context Database
            services.AddTransient<IMongoDBContext<DataRobot>, MongoDBContext<DataRobot, DataDatabaseSettings>>();
            services.AddTransient<IMongoDBContext<EquipmentAuth>, MongoDBContext<EquipmentAuth, UsersDatabaseSettings>>();
            services.AddTransient<IMongoDBContext<AuthorizationEquipment>, MongoDBContext<AuthorizationEquipment, AuthorizationDatabaseSettings>>();

            //Services Database
            services.AddTransient<DataRobotService>();
            services.AddTransient<UsersService>();
            services.AddTransient<IAuthorizationMQTT, AuthorizationService>();
            services.AddTransient<IAuthorizationTCP, AuthorizationService>();
        }

        private static void ConfigureServiceAuth(IServiceCollection services)
        {
            //Service Auth
            services.AddTransient<JwtService>();
            services.AddTransient<IValidatorEquipmentToken, JwtService>();
        }

        private static void ConfigureCacheEquipment(IServiceCollection services)
        {
            //Cache
            services.AddSingleton<EquipmentsConnectionCache>();
            services.AddTransient<IMQTTConnectionCache>(services => services.GetService<EquipmentsConnectionCache>());
            services.AddTransient<ITCPConnectionCache>(services => services.GetService<EquipmentsConnectionCache>());
            services.AddTransient<ICacheEquipment>(services => services.GetService<EquipmentsConnectionCache>());
            services.AddTransient<IChannelConnectionCache>(services => services.GetService<EquipmentsConnectionCache>());
        }

        private static void ConfigureHostedServices(IServiceCollection services)
        {
            //Services hosted singleton

            services.AddSingleton<MQTTService>();
            services.AddSingleton<TCPService>();
            services.AddSingleton<ChannelQueueService>();
            services.AddTransient<IVideoQueueHandler>(services => services.GetService<ChannelQueueService>());


            //Add all services hosted

            services.AddHostedService(services => services.GetService<MQTTService>());
            services.AddHostedService(services => services.GetService<ChannelQueueService>());
            services.AddHostedService(services => services.GetService<TCPService>());
        }
    }
}
