using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Reoria.Engine.Application.Configuration.Interfaces;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;
using Reoria.Server.Core.Database.DbContexts;

namespace Reoria.Server.Core.Database;

/// <summary>
/// Bootstrap phase participant for database configuration and service registration.
/// Handles both configuration sources and database context service registration during bootstrap.
/// </summary>
/// <remarks>
/// Runs during bootstrap phase - no constructor dependencies allowed.
/// </remarks>
public class DbContextBootstrap : IBootstrapConfiguration, IBootstrapServices
{
    /// <inheritdoc/>
    public string Name
        => "Database Context Bootstrap";

    /// <inheritdoc/>
    public string Description
        => "Adds database configuration sources and registers Entity Framework Core database contexts during bootstrap.";

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
    public void OnRegisterServices(ContainerBuilder services)
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
}