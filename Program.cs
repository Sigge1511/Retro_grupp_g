
using System;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Retro_grupp_g.Data;


namespace Retro_grupp_g
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Kulturinställningar: sv-SE. För att kunna hantera decimaltecken
            // i tex priser med både 1,5 och 1.5. Hjälpte till för att undvika errors---
            var supportedCultures = new[] { new CultureInfo("sv-SE") };
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("sv-SE");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                // (Valfritt) Till�t att kultur kan komma fr�n cookie/Accept-Language-header
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new CookieRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider()
                };
            });

            // Razor Pages + lokalisering (f�r valideringsmeddelanden/DataAnnotations)
            builder.Services
                .AddRazorPages()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

            //connection to online database
            builder.Services.AddDbContext<SakilaDbContext>(options =>
                options.UseMySql(builder.Configuration.GetConnectionString("SakilaDb"),
                new MySqlServerVersion(new Version(8, 0, 35))));



            var app = builder.Build();

            // --- Middleware ---
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Aktivera kultur (l�ggs tidigt i pipelinen)
            var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
