using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigPolicy
{
    public class PolicyOrigin
    {

        private static readonly string AllowUI = "AllowOriginsUI";
        private static readonly string AllowGateway = "AllowOriginsGateway";

        public static void ConfigureServices(IServiceCollection services, String[] urls)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: PolicyOrigin.AllowGateway, builder => {

                    foreach (String url in urls)
                    {
                        builder.WithOrigins(url)
                       .AllowAnyHeader()
                       .AllowAnyMethod();
                    }
                });
            });
        }

        public static void ConfigureServicesPolicyUI(IServiceCollection services, String[] urls)
        {

            services.AddCors(options =>
            {
                options.AddPolicy(name: PolicyOrigin.AllowUI, builder => {

                    foreach(String url in urls)
                    {
                        builder.WithOrigins(url)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    }
                });
            });
        }

        public static void ConfigureApp(IApplicationBuilder app)
        {
            app.UseCors(PolicyOrigin.AllowGateway);
        }

        public static void ConfigureAppPolicyUI(IApplicationBuilder app)
        {
            app.UseCors(PolicyOrigin.AllowUI);
        }

    }
}
