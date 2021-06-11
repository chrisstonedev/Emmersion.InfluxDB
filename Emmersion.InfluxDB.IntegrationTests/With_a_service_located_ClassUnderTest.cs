using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Emmersion.InfluxDB.IntegrationTests
{
    public class With_a_service_located_ClassUnderTest<T> where T : class
    {
        [SetUp]
        public void WithAServiceLocatedSetUp()
        {
            RegisterTestSpecificServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            ClassUnderTest = _serviceProvider.GetService<T>();
        }

        [TearDown]
        public void WithAServiceLocatedTearDown()
        {
            _serviceProvider.GetService<IInfluxRecorder>().Dispose();
        }

        private readonly IServiceCollection serviceCollection;
        private IServiceProvider _serviceProvider;

        protected With_a_service_located_ClassUnderTest()
        {
            serviceCollection = new ServiceCollection();
            InitializeConfiguration(serviceCollection);
            RegisterRequiredServices(serviceCollection);
            serviceCollection.AddTransient<T, T>();
        }

        protected T ClassUnderTest { get; private set; }

        protected virtual void RegisterTestSpecificServices(IServiceCollection services)
        {
        }

        protected U GetInstance<U>()
        {
            return _serviceProvider.GetService<U>();
        }

        private static void RegisterRequiredServices(IServiceCollection services)
        {
            DependencyInjectionConfig.ConfigureServices(services);
            services.AddTransient<Settings, Settings>();
            services.AddScoped<IInfluxSettings>(x => x.GetService<Settings>().InfluxSettings);
            services.AddTransient<IInfluxLogger, InfluxLogger>();
        }

        private static void InitializeConfiguration(IServiceCollection services)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: true);
            var configuration = configurationBuilder.Build();

            services.AddSingleton<IConfiguration>(configuration);
        }
    }
}