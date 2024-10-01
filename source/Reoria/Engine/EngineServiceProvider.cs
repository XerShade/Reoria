using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reoria.Engine.Interfaces;
using Serilog;

namespace Reoria.Engine;

public class EngineServiceProvider : IEngineServiceProvider
{
    protected readonly ServiceCollection serviceCollection;
    protected readonly IConfigurationRoot configuration;

    public EngineServiceProvider()
    {
        this.serviceCollection = new ServiceCollection();

        this.configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _ = this.serviceCollection.AddSingleton(this.configuration);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(this.configuration)
            .CreateLogger();

        _ = this.serviceCollection.AddLogging(builder => _ = builder.AddSerilog());
    }

    public IConfigurationRoot Configuration => this.configuration;
    public ServiceProvider ServiceProvider => this.serviceCollection.BuildServiceProvider();
}
