using Microsoft.EntityFrameworkCore;

namespace Reoria.Server.Core.Data.DbContexts;

public class AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : DbContext(options)
{

}