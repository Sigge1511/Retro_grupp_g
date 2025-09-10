using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Retro_grupp_g.Data;
namespace Retro_grupp_g
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Kulturinställningar: sv-SE ---
            var supportedCultures = new[] { new CultureInfo("sv-SE") };
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("sv-SE");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                // (Valfritt) Tillåt att kultur kan komma från cookie/Accept-Language-header
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new CookieRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider()
                };
            });

            // Razor Pages + lokalisering (för valideringsmeddelanden/DataAnnotations)
            builder.Services
                .AddRazorPages()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

            // DbContext
            builder.Services.AddDbContext<SakilaContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SakilaConnection")));

            var app = builder.Build();

            // --- Middleware ---
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Aktivera kultur (läggs tidigt i pipelinen)
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
