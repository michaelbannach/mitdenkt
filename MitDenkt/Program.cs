using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using MitDenkt.Data;
using MitDenkt.Models;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5057")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();

        });
});
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<MitDenktContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<Customer, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false; 
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<MitDenktContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/auth/login";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

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
        bool exists = context.Employees.Any(e =>
            e.FirstName == employee.FirstName && e.LastName == employee.LastName);

        if (!exists)
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


// Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    else
    {
        app.UseHttpsRedirection();
    }

   
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "client")),
        RequestPath = "/client"
    });
    app.UseRouting();
    var clientPath = Path.Combine(builder.Environment.ContentRootPath, "client");

    if (Directory.Exists(clientPath))
    {
        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = new PhysicalFileProvider(clientPath),
            RequestPath = ""
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(clientPath),
            RequestPath = ""
        });
    }
    app.UseCors(MyAllowSpecificOrigins);
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}