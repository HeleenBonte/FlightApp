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
using FlightApp.Util.Hotels;
using FlightApp.Util.Hotels.Interfaces;
using FlightApp.Util.Mail;
using FlightApp.Util.Mail.Interfaces;
using FlightApp.Util.PDF;
using FlightApp.Util.PDF.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Configuration;
using System.Globalization;
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

// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

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

// Add MVC with localization support
builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

// Configure supported cultures with proper currency format and date format
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // Create custom cultures with correct currency symbols and date formats
    var enCulture = new CultureInfo("en");
    var nlCulture = new CultureInfo("nl");
    var esCulture = new CultureInfo("es");

    // Set Euro as the currency symbol for all cultures
    enCulture.NumberFormat.CurrencySymbol = "€";
    nlCulture.NumberFormat.CurrencySymbol = "€";
    esCulture.NumberFormat.CurrencySymbol = "€";

    // Set all cultures to show currency symbol before amount (pattern 0 = €n)
    enCulture.NumberFormat.CurrencyPositivePattern = 0; // € n
    nlCulture.NumberFormat.CurrencyPositivePattern = 0; // € n
    esCulture.NumberFormat.CurrencyPositivePattern = 0; // € n

    // Also set negative patterns to show € before amount
    enCulture.NumberFormat.CurrencyNegativePattern = 1; // -€n
    nlCulture.NumberFormat.CurrencyNegativePattern = 1; // -€n
    esCulture.NumberFormat.CurrencyNegativePattern = 1; // -€n

    // Set European date format (dd/MM/yyyy) for all cultures
    enCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
    nlCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
    esCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";

    // Also ensure the same format for date time fields
    enCulture.DateTimeFormat.DateSeparator = "/";
    nlCulture.DateTimeFormat.DateSeparator = "/";
    esCulture.DateTimeFormat.DateSeparator = "/";

    var supportedCultures = new[]
    {
        enCulture,
        nlCulture,
        esCulture
    };

    options.DefaultRequestCulture = new RequestCulture(enCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

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
builder.Services.AddScoped<IHolidayPriceService, HolidayPriceService>();

builder.Services.AddTransient<IDAO<MealChoice>, MealChoiceDAO>();
builder.Services.AddTransient<IService<MealChoice>, MealChoiceService>();

builder.Services.AddTransient<IPassengerDAO, PassengerDAO>();
builder.Services.AddTransient<IPassengerService, PassengerService>();

builder.Services.AddTransient<IRouteDAO, RouteDAO>();
builder.Services.AddTransient<IRouteService, RouteService>();

builder.Services.AddTransient<IDAO<Ticket>, TicketDAO>();
builder.Services.AddTransient<IService<Ticket>, TicketService>();

builder.Services.AddTransient<IBookingHistoryDAO, BookingHistoryDAO>();
builder.Services.AddTransient<IBookingHistoryService, BookingHistoryService>();

builder.Services.AddTransient<ICreatePDF, CreatePDF>();
builder.Services.AddTransient<IEmailSend, EmailSend>();
builder.Services.AddTransient<IHotelService, HotelService>();

builder.Services.AddTransient<ITicketDAO, TicketDAO>();
builder.Services.AddTransient<ITicketService, TicketService>();


builder.Services.AddTransient<IDAO<AspNetUser>, AspUserDAO>();
builder.Services.AddTransient<IService<AspNetUser>, AspUserService>();



builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Configure supported cultures
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "nl", "es" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FlightApp API",
        Version = "version 1",
        Description = "An API to perform Flight operations",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Zephyrus",
            Email = "ZephyrusAirlines@gmail.com",
            Url = new Uri("https://vives.be")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "Flight API LICX",
            Url = new Uri("https://example.com.license"),
        }
    });
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

var swaggerOptions = new FlightApp.Options.SwaggerOptions();
builder.Configuration.GetSection(nameof(FlightApp.Options.SwaggerOptions)).Bind(swaggerOptions);

app.UseSwagger(option => { option.RouteTemplate = swaggerOptions.JsonRoute; });

app.UseSwaggerUI(option =>
{
    option.SwaggerEndpoint(swaggerOptions.UiEndpoint, swaggerOptions.Description);
});

app.UseSwagger();

app.UseRequestLocalization();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
