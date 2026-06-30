using Didakt.Api.Auth.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Didakt.Api.Auth.Data;

internal class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}