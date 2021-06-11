using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Emmersion.InfluxDB.UnitTests")]
[assembly: InternalsVisibleTo("Emmersion.InfluxDB.IntegrationTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Emmersion.InfluxDB
{

    public class DependencyInjectionConfig
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpClient, HttpClient>();
            services.AddTransient<ISender, Sender>();
            services.AddSingleton<IInfluxRecorder, InfluxRecorder>();
        }
    }
}