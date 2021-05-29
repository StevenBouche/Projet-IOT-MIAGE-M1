using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace ConfigPolicy
{
    public static class PolicyOrigin
    {

        public static void AddCustomPolicy(this IServiceCollection services, PolicyConfig config)
        {
            config.AllowPolicies.ForEach(policy =>
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(name: policy.Name, builder => {

                        foreach (string url in policy.Allowed)
                        {
                            builder.WithOrigins(url)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                        }
                    });
                });
            });
        }

        public static void UseCustomPolicy(this IApplicationBuilder app, PolicyConfig config)
        {
            config.AllowPolicies.ForEach(policy =>
            {
                app.UseCors(policy.Name);
            });
        }
    }
}
