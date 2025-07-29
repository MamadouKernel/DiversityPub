
using DiversityPub.Data;
using DiversityPub.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64;
    });

// Ajouter SignalR
builder.Services.AddSignalR();

builder.Services.AddDbContext<DiversityPubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Enregistrer les services hébergés
builder.Services.AddHostedService<CampagneExpirationService>();
builder.Services.AddHostedService<GeolocationService>();

// Enregistrer les services HTTP
builder.Services.AddHttpClient<GeocodingService>();
builder.Services.AddScoped<GeocodingService>();

// Configuration de l'authentification par cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Session de 8 heures
        options.SlidingExpiration = true; // Renouvellement automatique
        options.Cookie.HttpOnly = true; // Sécurité
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS en production
        options.Cookie.SameSite = SameSiteMode.Lax; // Compatibilité cross-site
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Mapper le hub SignalR
app.MapHub<DiversityPub.Hubs.NotificationHub>("/notificationHub");

app.Run();
