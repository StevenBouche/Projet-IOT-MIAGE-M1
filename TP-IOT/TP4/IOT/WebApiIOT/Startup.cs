using DataAccess.IOT;
using DataAccess.IOT.Cache;
using DataAccess.IOT.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Models;
using MongoDBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiIOT
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApiIOT", Version = "v1" });
            });

            services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowOriginsFront", builder =>
                {
                    builder
                        .WithOrigins("http://localhost:443")
                        .AllowAnyHeader()
                        .AllowAnyMethod();

                    builder
                        .WithOrigins("http://localhost:80")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });

             });

            services.Configure<AlertDatabaseSetting>(Configuration.GetSection(nameof(AlertDatabaseSetting)));
            services.AddSingleton<IAlertDatabaseSetting>(sp => sp.GetRequiredService<IOptions<AlertDatabaseSetting>>().Value);
            services.AddTransient<IMongoDBContext<Alert>, MongoDBContext<Alert, IAlertDatabaseSetting>>();

            services.Configure<EquipmentIOTDatabaseSetting>(Configuration.GetSection(nameof(EquipmentIOTDatabaseSetting)));
            services.AddSingleton<IEquipmentIOTDatabaseSetting>(sp => sp.GetRequiredService<IOptions<EquipmentIOTDatabaseSetting>>().Value);
            services.AddTransient<IMongoDBContext<EquipmentIOT>, MongoDBContext<EquipmentIOT, IEquipmentIOTDatabaseSetting>>();

            services.Configure<DataIOTDatabaseSetting>(Configuration.GetSection(nameof(DataIOTDatabaseSetting)));
            services.AddSingleton<IDataIOTDatabaseSetting>(sp => sp.GetRequiredService<IOptions<DataIOTDatabaseSetting>>().Value);
            services.AddTransient<IMongoDBContext<DataIOT>, MongoDBContext<DataIOT, IDataIOTDatabaseSetting>>();

            services.AddTransient<EquipmentIOTManager, EquipmentIOTManager>();
            services.AddTransient<DataIOTManager, DataIOTManager>();
            services.AddTransient<AlertManager, AlertManager>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApiIOT v1"));
            }

            //app.UseHttpsRedirection();

            app.UseCors("AllowOriginsFront");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
