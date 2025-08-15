using Microsoft.EntityFrameworkCore;

public sealed class DataContext(ILoggerFactory LoggerFactory, IConfiguration Configuration) : DbContext
{
    public DbSet<Product> Products { get; set; }

    private readonly ILoggerFactory _loggerFactory = LoggerFactory;
    private readonly IConfiguration _configuration = Configuration;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseNpgsql(_configuration.GetConnectionString("Default")!)
            .UseLoggerFactory(_loggerFactory)
            .EnableDetailedErrors();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder
            .Entity<Product>(product =>
            {
                product
                    .ToTable("product")
                    .HasKey(product => product.Id);

                product.Property(p => p.Id)
                       .HasColumnName("id");

                product.Property(p => p.Name)
                       .HasColumnName("name");

                product.Property(p => p.Quantity)
                       .HasColumnName("quantity");

                product.Property(p => p.Code)
                       .HasColumnName("code");
            });
}
