using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using PortalAcademico.Data;
using PortalAcademico.Models;
using PortalAcademico.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

    if (IsPostgres(connectionString))
    {
        var normalized = NormalizePostgresConnectionString(connectionString);
        options.UseNpgsql(normalized);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "PortalAcademico.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICatalogoCursosCacheService, CatalogoCursosCacheService>();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var scopedServices = scope.ServiceProvider;
    var loggerFactory = scopedServices.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("ApplicationDbSeeder");

    try
    {
        await ApplicationDbSeeder.SeedAsync(scopedServices, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al ejecutar la inicializacion de datos");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

static bool IsPostgres(string connectionString)
{
    return connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase);
}

static string NormalizePostgresConnectionString(string connectionString)
{
    if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(connectionString);
        var userInfo = Uri.UnescapeDataString(uri.UserInfo ?? string.Empty).Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Database = uri.AbsolutePath.Trim('/')
        };

        builder.Port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port;

        if (userInfo.Length > 0 && !string.IsNullOrWhiteSpace(userInfo[0]))
        {
            builder.Username = userInfo[0];
        }

        if (userInfo.Length > 1 && !string.IsNullOrWhiteSpace(userInfo[1]))
        {
            builder.Password = userInfo[1];
        }

        if (!string.IsNullOrEmpty(uri.Query))
        {
            var query = uri.Query.Trim('?');
            foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split('=', 2);
                if (parts.Length == 2)
                {
                    builder[parts[0]] = Uri.UnescapeDataString(parts[1]);
                }
            }
        }

        if (!builder.ContainsKey("Ssl Mode"))
        {
            builder.SslMode = SslMode.Require;
        }

        return builder.ConnectionString;
    }

    return connectionString;
}
