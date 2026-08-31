using Microsoft.EntityFrameworkCore;

namespace Reoria.Server.Core.Database.DbContexts;

public partial class DataDbContext(DbContextOptions<DataDbContext> options) : DbContext(options)
{
}