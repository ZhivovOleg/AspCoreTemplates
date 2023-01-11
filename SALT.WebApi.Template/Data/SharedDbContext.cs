using Microsoft.EntityFrameworkCore;
using SALT.WebApi.Template.Data.Models;

namespace SALT.WebApi.Template.Data;

/// <summary>
/// Context for postgres DB
/// </summary>
public sealed class SharedDbContext : DbContext
{
    /// <summary>
    /// DI ctor
    /// </summary>
    public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
    { }

    /// <summary>
    /// models
    /// </summary>
    public DbSet<ExampleModel> ExampleModels { get; set; }
}