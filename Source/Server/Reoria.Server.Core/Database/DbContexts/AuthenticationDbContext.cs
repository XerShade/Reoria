using Microsoft.EntityFrameworkCore;

namespace Reoria.Server.Core.Database.DbContexts;

public partial class AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataDbContext).Assembly);

    }
}