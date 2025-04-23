using System;
using System.Collections.Generic;
using FlightApp.Domains.EntitiesDB;
using Microsoft.EntityFrameworkCore;

namespace FlightApp.Domains.DataDB;

public partial class FlightsDbContext : DbContext
{
    public FlightsDbContext()
    {
    }

    public FlightsDbContext(DbContextOptions<DbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingClass> BookingClasses { get; set; }

    public virtual DbSet<BookingHistory> BookingHistories { get; set; }

    public virtual DbSet<BookingPassenger> BookingPassengers { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Flight> Flights { get; set; }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<MealChoice> MealChoices { get; set; }

    public virtual DbSet<Passenger> Passengers { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<RouteFlightBridge> RouteFlightBridges { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQL22_VIVES; Database=Flights; Trusted_Connection=True; TrustServerCertificate=True; MultipleActiveResultSets=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Booking");

            entity.Property(e => e.BookingId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BookingID");
            entity.Property(e => e.RouteCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<BookingClass>(entity =>
        {
            entity.ToTable("BookingClass");

            entity.Property(e => e.BookingClassId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BookingClassID");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BookingHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId);

            entity.ToTable("BookingHistory");

            entity.Property(e => e.HistoryId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("HistoryID");
            entity.Property(e => e.BookingId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BookingID");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<BookingPassenger>(entity =>
        {
            entity.HasKey(e => new { e.BookingId, e.PassengerId });

            entity.ToTable("BookingPassenger");

            entity.Property(e => e.BookingId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BookingID");
            entity.Property(e => e.PassengerId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PassengerID");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("City");

            entity.Property(e => e.CityId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CityID");
            entity.Property(e => e.CityName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.ToTable("Flight");

            entity.Property(e => e.FlightId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("FlightID");
            entity.Property(e => e.ArrivalCity)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DepartureCity)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.ToTable("Holiday");

            entity.Property(e => e.HolidayId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("HolidayID");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MealChoice>(entity =>
        {
            entity.ToTable("MealChoice");

            entity.Property(e => e.MealChoiceId).HasColumnName("MealChoiceID");
            entity.Property(e => e.RouteId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("RouteID");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Passenger>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Passenger");

            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PassengerId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PassengerID");
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.ToTable("Route");

            entity.Property(e => e.RouteId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("RouteID");
            entity.Property(e => e.ArrivalCityId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ArrivalCityID");
            entity.Property(e => e.DepartureCityId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DepartureCityID");
        });

        modelBuilder.Entity<RouteFlightBridge>(entity =>
        {
            entity.HasKey(e => new { e.RouteId, e.FlightId });

            entity.ToTable("RouteFlightBridge");

            entity.Property(e => e.RouteId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("RouteID");
            entity.Property(e => e.FlightId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("FlightID");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Ticket");

            entity.Property(e => e.TicketId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TicketID");
            entity.Property(e => e.BookingClassId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BookingClassID");
            entity.Property(e => e.FlightId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("FlightID");
            entity.Property(e => e.MealChoiceId).HasColumnName("MealChoiceID");
            entity.Property(e => e.PassengerId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PassengerID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
