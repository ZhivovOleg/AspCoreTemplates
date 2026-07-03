using Microsoft.EntityFrameworkCore;
using SALT.WebApi.Template.Data.Models;

namespace SALT.WebApi.Template.Data;

/// <summary>
/// Context for postgres DB
/// </summary>
/// <remarks>
/// DI ctor
/// </remarks>
public sealed class SharedDbContext(DbContextOptions<SharedDbContext> options) : DbContext(options)
{

    /// <summary>
    /// models
    /// </summary>
    public DbSet<ExampleModel> ExampleModels => Set<ExampleModel>();
}
