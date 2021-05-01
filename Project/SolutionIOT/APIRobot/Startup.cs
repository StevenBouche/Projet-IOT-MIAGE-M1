using APIRobot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDBAccess;
using MQTTBroker.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIRobot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MQTTConfig>(Configuration.GetSection(nameof(MQTTConfig)));
            //Load mongodb settings in RestaurantDatabaseSetting where is in appsettings.json with name : RestaurantDatabaseSetting
            services.Configure<DatabaseSettings>(Configuration.GetSection(nameof(DatabaseSettings)));

            //services.AddTransient<IMongoDBContext<Account>, MongoDBContext<Account, IDatabaseSettings>>();

            services.AddSignalR();

            services.AddSingleton<MQTTService, MQTTService>()
                    .AddHostedService(services => services.GetService<MQTTService>());

            services.AddSingleton<VideoQueue, VideoQueue>()
                    .AddHostedService(services => services.GetService<VideoQueue>());

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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<VideoHub>("/video");
            });
        }
    }
}
