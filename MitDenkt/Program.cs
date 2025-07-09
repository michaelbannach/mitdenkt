using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MitDenkt.Data;
using MitDenkt.Models;
using MitDenkt.Services;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Services registrieren
builder.Services.AddScoped<BookingManager>();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

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

builder.Services.AddDbContext<MitDenktContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<Customer>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<MitDenktContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/auth/login";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// === Datenbank seeden ===
await DbSeeder.Seed(app.Services);

// === Middleware-Konfiguration ===
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();   
app.UseStaticFiles();   

app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();