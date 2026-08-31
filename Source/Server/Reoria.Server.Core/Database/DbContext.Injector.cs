using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Reoria.Engine.Application.Configuration.Interfaces;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Server.Core.Database.DbContexts;

namespace Reoria.Server.Core.Database;

public class DbContextInjector : IApplicationConfigurationInjector, IApplicationServicesInjector
{
    /// <inheritdoc/>
    public string Name 
        => "Database Context Injector";

    /// <inheritdoc/>
    public string Description 
        => "Adds support for a database context powered by Entity Framework Core to the application.";

    /// <inheritdoc/>
    public Type[] Dependencies
        => [];

    /// <inheritdoc/>
    public Platform Platform
        => Platform.Server;

    /// <inheritdoc/>
    public void OnBuildConfiguration(IAppConfigurationBuilder builder) 
        => builder.AddConfigurationSource("appsettings.server.data.json", false, true);

    /// <inheritdoc/>
    public void OnBuildServices(ContainerBuilder services)
    {
        _ = services.Register(context =>
        {
            IConfiguration configuration = context.Resolve<IConfiguration>();

            DbContextOptions<AuthenticationDbContext> options = new DbContextOptionsBuilder<AuthenticationDbContext>()
                .UseSqlite(configuration.GetConnectionString("Authentication"))
                .Options;

            return new AuthenticationDbContext(options);
        })
        .AsSelf()
        .InstancePerLifetimeScope();        

        _ = services.Register(context =>
        {
            IConfiguration configuration = context.Resolve<IConfiguration>();

            DbContextOptions<DataDbContext> options = new DbContextOptionsBuilder<DataDbContext>()
                .UseSqlite(configuration.GetConnectionString("Data"))
                .Options;

            return new DataDbContext(options);
        })
        .AsSelf()
        .InstancePerLifetimeScope();
    }

    /// <inheritdoc/>
    public void OnConfigureServices(IServiceProvider provider)
    {

    }
}