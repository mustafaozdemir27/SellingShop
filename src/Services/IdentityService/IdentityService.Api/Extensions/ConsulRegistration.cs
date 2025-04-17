using Consul;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using System.Net;
using System.Net.Sockets;

namespace IdentityService.Api.Extensions;

public static class ConsulRegistration
{
    public static IServiceCollection ConfigureConsul(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConsulClient, ConsulClient>(c => new ConsulClient(consulConfig =>
        {
            var address = configuration["ConsulConfig:Address"];
            consulConfig.Address = new Uri("http://localhost:8500");
        }));

        return services;
    }

    public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime)
    {
        var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();

        var loggingFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();

        var logger = loggingFactory.CreateLogger<IApplicationBuilder>();

        // Get server ip address
        var features = app.Properties["server.Features"] as FeatureCollection;

        var addresses = features.Get<IServerAddressesFeature>();
        if (addresses.Addresses.Count == 0)
        {
            // Add the default address to the IServerAddressesFeature if it does not exist
            addresses.Addresses.Add("http://localhost:5005");
        }
        var address = addresses.Addresses.First();

        // Get local IP address
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        var ip = ipAddress?.ToString() ?? "localhost";

        // Register service with consul
        var uri = new Uri(address);
        var registration = new AgentServiceRegistration()
        {
            ID = "IdentityService",
            Name = "IdentityService",
            Address = ip,
            Port = uri.Port,
            Tags = new[] { "IdentityService"},
            Check = new AgentServiceCheck()
            {
                HTTP = $"{uri.Scheme}://{ip}:{uri.Port}/health",
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(5),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
            }
        };

        logger.LogInformation($"Registering service with Consul at {ip}:{uri.Port}..");
        consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        consulClient.Agent.ServiceRegister(registration).Wait();

        lifetime.ApplicationStopping.Register(() =>
        {
            logger.LogInformation("Deregistering service from Consul..");
            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        });

        return app;
    }
}
