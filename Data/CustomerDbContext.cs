using Microsoft.EntityFrameworkCore;

public class CustomerDbContext : DbContext
{
    public required DbSet<Customer> Customers { get; set; }

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }
}
