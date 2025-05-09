using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FlightApp.Areas.Identity.Data;
using FlightApp.Data;
using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories;
using FlightApp.Repositories.Interface;
using FlightApp.Repositories.Interfaces;
using FlightApp.Services;
using FlightApp.Services.Interfaces;
using FlightApp.Util.Mail;
using FlightApp.Util.Mail.Interfaces;
using FlightApp.Util.PDF;
using FlightApp.Util.PDF.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using Route = FlightApp.Domains.EntitiesDB.Route;

var builder = WebApplication.CreateBuilder(args);
//var connectionString1 = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

// Key Vault settings
string? vaultUrl = builder.Configuration["KeyVault:VaultUrl"];
string? dbSecretName = builder.Configuration["KeyVault:dbSecretName"];
string? mailSecretName = builder.Configuration["KeyVault:mailSecretName"];

// Maak een client met default credentials (werkt lokaal met ingelogde gebruiker, en in Azure met managed identity)
var client = new SecretClient(new Uri(vaultUrl), new DefaultAzureCredential());

KeyVaultSecret dbSecret = client.GetSecret(dbSecretName);
KeyVaultSecret mailSecret = client.GetSecret(mailSecretName);

// Add services to the container.
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"] = dbSecret.Value;
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<FlightsDbContext>(options =>
       options.UseSqlServer(connectionString));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Configuration["Emailsettings:Password"] = mailSecret.Value;
builder.Services.AddSingleton<IEmailSend, EmailSend>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IDAO<BookingClass>, BookingClassDAO>();
builder.Services.AddTransient<IService<BookingClass>, BookingClassService>();

builder.Services.AddTransient<IDAO<Booking>, BookingDAO>();
builder.Services.AddTransient<IService<Booking>, BookingService>();

builder.Services.AddTransient<IDAO<City>, CityDAO>();
builder.Services.AddTransient<IService<City>, CityService>();

builder.Services.AddTransient<IFlightDAO, FlightDAO>();
builder.Services.AddTransient<IFlightService, FlightService>();

builder.Services.AddTransient<IDAO<Holiday>, HolidayDAO>();
builder.Services.AddTransient<IService<Holiday>, HolidayService>();

builder.Services.AddTransient<IDAO<MealChoice>, MealChoiceDAO>();
builder.Services.AddTransient<IService<MealChoice>, MealChoiceService>();

builder.Services.AddTransient<IDAO<Passenger>, PassengerDAO>();
builder.Services.AddTransient<IService<Passenger>, PassengerService>();

builder.Services.AddTransient<IRouteDAO, RouteDAO>();
builder.Services.AddTransient<IRouteService, RouteService>();

builder.Services.AddTransient<IDAO<Ticket>, TicketDAO>();
builder.Services.AddTransient<IService<Ticket>, TicketService>();

builder.Services.AddTransient<IBookingHistoryDAO, BookingHistoryDAO>();
builder.Services.AddTransient<IBookingHistoryService, BookingHistoryService>();

builder.Services.AddTransient<ICreatePDF, CreatePDF>();
builder.Services.AddTransient<IEmailSend, EmailSend>();

builder.Services.AddTransient<ITicketDAO, TicketDAO>();
builder.Services.AddTransient<ITicketService, TicketService>();



builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

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
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
