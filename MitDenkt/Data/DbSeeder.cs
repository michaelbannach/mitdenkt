using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MitDenkt.Models;

namespace MitDenkt.Data
{
    public static class DbSeeder
    {
        public static async Task Seed(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MitDenktContext>();

            // Automatisch Migrationen anwenden
            context.Database.Migrate();

            // Mitarbeiter 
            if (!context.Employees.Any())
            {
                context.Employees.AddRange(
                    new Employee { FirstName = "Tina", LastName = "Müller" },
                    new Employee { FirstName = "Peter", LastName = "Meier" },
                    new Employee { FirstName = "Michael", LastName = "Berger" },
                    new Employee { FirstName = "Sonja", LastName = "Klein" }
                );
            }

            //Dienstleistungen
            if (!context.Services.Any())
            {
                context.Services.AddRange(
                    new Service { Name = "Damenhaarschnitt", Category = "Hauptdienstleistung", DurationMinutes = 45, Price = 35 },
                    new Service { Name = "Herrenhaarschnitt", Category = "Hauptdienstleistung", DurationMinutes = 30, Price = 25 },
                    new Service { Name = "Farbe: Komplett", Category = "Farbe", DurationMinutes = 60, Price = 55 },
                    new Service { Name = "Strähnen", Category = "Farbe", DurationMinutes = 90, Price = 70 },
                    new Service { Name = "Haarkur", Category = "Pflege", DurationMinutes = 30, Price = 15 },
                    new Service { Name = "Waschen & Föhnen", Category = "Pflege", DurationMinutes = 30, Price = 18 },
                    new Service { Name = "Augenbrauen zupfen", Category = "Extra", DurationMinutes = 15, Price = 8 },
                    new Service { Name = "Wimpern färben", Category = "Extra", DurationMinutes = 15, Price = 10 }
                );
            }

            await context.SaveChangesAsync();

            // Demo-Nutzer
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Customer>>();
            string demoEmail = "demonutzer@mitdenkt.de";
            string demoPassword = "GrandiosesPasswort1!";

            if (await userManager.FindByEmailAsync(demoEmail) == null)
            {
                var demoUser = new Customer
                {
                    UserName = demoEmail,
                    Email = demoEmail,
                    FirstName = "Demo",
                    LastName = "Nutzer",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(demoUser, demoPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Fehler beim Anlegen des Demo-Benutzers: {errors}");
                }
            }

            //  Beispiel-Buchung 
            if (!context.Bookings.Any())
            {
                var tina = context.Employees.FirstOrDefault(e => e.FirstName == "Tina");
                var customer = await userManager.FindByEmailAsync(demoEmail);
                var service = context.Services.FirstOrDefault();

                if (tina != null && customer != null && service != null)
                {
                    context.Bookings.Add(new Booking
                    {
                        CustomerId = customer.Id,
                        EmployeeId = tina.Id,
                        StartTime = DateTime.Today.AddHours(10),
                        EndTime = DateTime.Today.AddHours(11),
                        BookingServices = new[] { new BookingService { ServiceId = service.Id } }
                    });

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}