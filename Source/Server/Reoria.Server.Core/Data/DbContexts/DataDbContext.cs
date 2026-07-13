using Microsoft.EntityFrameworkCore;

namespace Reoria.Server.Core.Data.DbContexts;

public class DataDbContext(DbContextOptions<DataDbContext> options) : DbContext(options)
{
}