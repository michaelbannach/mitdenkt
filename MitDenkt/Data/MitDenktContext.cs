using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MitDenkt.Models;

namespace MitDenkt.Data;

public class MitDenktContext : IdentityDbContext<Customer, IdentityRole<int>, int>
{
    public  MitDenktContext (DbContextOptions<MitDenktContext> options) : base(options) {}
    
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
}