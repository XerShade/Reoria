using Microsoft.EntityFrameworkCore;

namespace Reoria.Server.Core.Database.DbContexts;

public partial class AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : DbContext(options)
{

}