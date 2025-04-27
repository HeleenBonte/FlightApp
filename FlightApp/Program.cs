using FlightApp.Data;
using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories;
using FlightApp.Repositories.Interface;
using FlightApp.Repositories.Interfaces;
using FlightApp.Services;
using FlightApp.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Route = FlightApp.Domains.EntitiesDB.Route;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<FlightsDbContext>(options =>
       options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
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

builder.Services.AddTransient<IDAO<Route>, RouteDAO>();
builder.Services.AddTransient<IService<Route>, RouteService>();

builder.Services.AddTransient<IDAO<Ticket>, TicketDAO>();
builder.Services.AddTransient<IService<Ticket>, TicketService>();

builder.Services.AddAutoMapper(typeof(Program));

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
