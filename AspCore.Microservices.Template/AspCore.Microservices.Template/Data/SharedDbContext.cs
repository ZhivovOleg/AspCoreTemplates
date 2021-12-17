using Microsoft.EntityFrameworkCore;
using AspCore.Microservices.Template.Data.Models;

namespace AspCore.Microservices.Template.Data;

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