using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MitDenkt.Data;
using MitDenkt.Models;
using MitDenkt.Services;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container
builder.Services.AddScoped<BookingManager>();

builder.Services.AddRazorPages();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("http://localhost:5057")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<MitDenktContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<Customer>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole<int>>() // optional: Rollen
.AddEntityFrameworkStores<MitDenktContext>()
.AddDefaultUI() // Razor Pages (z. B. Login)
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/auth/login";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// Seed-Datenbank mit Standarddaten
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MitDenktContext>();

    var defaultEmployees = new[]
    {
        new Employee { FirstName = "Tina", LastName = "Müller" },
        new Employee { FirstName = "Peter", LastName = "Meier" },
        new Employee { FirstName = "Michael", LastName = "Berger" },
        new Employee { FirstName = "Sonja", LastName = "Klein" }
    };

    foreach (var employee in defaultEmployees)
    {
        if (!context.Employees.Any(e => e.FirstName == employee.FirstName && e.LastName == employee.LastName))
        {
            context.Employees.Add(employee);
        }
    }

    context.SaveChanges();

    if (!context.Bookings.Any())
    {
        var tina = context.Employees.FirstOrDefault(e => e.FirstName == "Tina");
        var customer = context.Customers.FirstOrDefault();

        if (tina != null && customer != null)
        {
            context.Bookings.Add(new Booking
            {
                CustomerId = customer.Id,
                EmployeeId = tina.Id,
                StartTime = DateTime.Today.AddHours(9),
                EndTime = DateTime.Today.AddHours(10)
            });
            context.SaveChanges();
        }
    }
}

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}

// 👉 wwwroot verwenden
app.UseDefaultFiles();   // z. B. index.html automatisch laden
app.UseStaticFiles();    // z. B. style.css, img/logo.png etc.

app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
