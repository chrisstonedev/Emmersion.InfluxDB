using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("EL.InfluxDB.UnitTests")]
[assembly: InternalsVisibleTo("EL.InfluxDB.IntegrationTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace EL.InfluxDB
{

    public class DependencyInjectionConfig
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDataSanitizer, DataSanitizer>();
            services.AddSingleton<IHttpClient, HttpClient>();
            services.AddTransient<ISender, Sender>();
            services.AddSingleton<IInfluxRecorder, InfluxRecorder>();
        }
    }
}