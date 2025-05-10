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

    public FlightsDbContext(DbContextOptions<FlightsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingClass> BookingClasses { get; set; }

    public virtual DbSet<BookingHistory> BookingHistories { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Flight> Flights { get; set; }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<MealChoice> MealChoices { get; set; }

    public virtual DbSet<Passenger> Passengers { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=flightfullstack.database.windows.net; Database=Flights; User Id=Beheerder; password=FullstackServer4Me; TrustServerCertificate=True; MultipleActiveResultSets=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasDefaultValue("");
            entity.Property(e => e.LastName).HasDefaultValue("");
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Booking");

            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.BookingTime).HasColumnType("datetime");
            entity.Property(e => e.FlightId).HasColumnName("FlightID");
            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("UserID");

            entity.HasOne(d => d.Flight).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.FlightId)
                .HasConstraintName("FK_Booking_Flight");

            entity.HasOne(d => d.Route).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("FK_Booking_Route");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Booking_AspNetUsers");

            entity.HasMany(d => d.Passengers).WithMany(p => p.Bookings)
                .UsingEntity<Dictionary<string, object>>(
                    "BookingPassenger",
                    r => r.HasOne<Passenger>().WithMany()
                        .HasForeignKey("PassengerId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_BookingPassenger_Passenger"),
                    l => l.HasOne<Booking>().WithMany()
                        .HasForeignKey("BookingId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_BookingPassenger_Booking"),
                    j =>
                    {
                        j.HasKey("BookingId", "PassengerId");
                        j.ToTable("BookingPassenger");
                        j.IndexerProperty<int>("BookingId").HasColumnName("BookingID");
                        j.IndexerProperty<int>("PassengerId").HasColumnName("PassengerID");
                    });
        });

        modelBuilder.Entity<BookingClass>(entity =>
        {
            entity.ToTable("BookingClass");

            entity.Property(e => e.BookingClassId).HasColumnName("BookingClassID");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BookingHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId);

            entity.ToTable("BookingHistory");

            entity.Property(e => e.HistoryId).HasColumnName("HistoryID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("UserID");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingHistories)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingHistory_Booking");

            entity.HasOne(d => d.User).WithMany(p => p.BookingHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingHistory_AspNetUsers");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("City");

            entity.Property(e => e.CityId).HasColumnName("CityID");
            entity.Property(e => e.CityName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.ToTable("Flight");

            entity.Property(e => e.FlightId)
                .ValueGeneratedNever()
                .HasColumnName("FlightID");
            entity.Property(e => e.ArrivalTime).HasColumnType("datetime");
            entity.Property(e => e.DepartureTime).HasColumnType("datetime");

            entity.HasOne(d => d.ArrivalCityNavigation).WithMany(p => p.FlightArrivalCityNavigations)
                .HasForeignKey(d => d.ArrivalCity)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Flight_City_Arrival");

            entity.HasOne(d => d.DepartureCityNavigation).WithMany(p => p.FlightDepartureCityNavigations)
                .HasForeignKey(d => d.DepartureCity)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Flight_City_Departure");
        });

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.ToTable("Holiday");

            entity.Property(e => e.HolidayId).HasColumnName("HolidayID");
            entity.Property(e => e.CityId).HasColumnName("CityID");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.City).WithMany(p => p.Holidays)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_Holiday_City");
        });

        modelBuilder.Entity<MealChoice>(entity =>
        {
            entity.ToTable("MealChoice");

            entity.Property(e => e.MealChoiceId).HasColumnName("MealChoiceID");
            entity.Property(e => e.CityId).HasColumnName("CityID");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.City).WithMany(p => p.MealChoices)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_MealChoice_Route");
        });

        modelBuilder.Entity<Passenger>(entity =>
        {
            entity.ToTable("Passenger");

            entity.Property(e => e.PassengerId).HasColumnName("PassengerID");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.ToTable("Route");

            entity.Property(e => e.RouteId)
                .ValueGeneratedNever()
                .HasColumnName("RouteID");
            entity.Property(e => e.ArrivalCityId).HasColumnName("ArrivalCityID");
            entity.Property(e => e.ArrivalTime).HasColumnType("datetime");
            entity.Property(e => e.DepartureCityId).HasColumnName("DepartureCityID");
            entity.Property(e => e.DepartureTime).HasColumnType("datetime");

            entity.HasOne(d => d.ArrivalCity).WithMany(p => p.RouteArrivalCities)
                .HasForeignKey(d => d.ArrivalCityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Route_City_Arrival");

            entity.HasOne(d => d.DepartureCity).WithMany(p => p.RouteDepartureCities)
                .HasForeignKey(d => d.DepartureCityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Route_City_Departure");

            entity.HasMany(d => d.Flights).WithMany(p => p.Routes)
                .UsingEntity<Dictionary<string, object>>(
                    "RouteFlightBridge",
                    r => r.HasOne<Flight>().WithMany()
                        .HasForeignKey("FlightId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RouteFlightBridge_Flight"),
                    l => l.HasOne<Route>().WithMany()
                        .HasForeignKey("RouteId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RouteFlightBridge_Route"),
                    j =>
                    {
                        j.HasKey("RouteId", "FlightId");
                        j.ToTable("RouteFlightBridge");
                        j.IndexerProperty<int>("RouteId").HasColumnName("RouteID");
                        j.IndexerProperty<int>("FlightId").HasColumnName("FlightID");
                    });
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Ticket");

            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.BookingClassId).HasColumnName("BookingClassID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.FlightId).HasColumnName("FlightID");
            entity.Property(e => e.MealChoiceId).HasColumnName("MealChoiceID");
            entity.Property(e => e.PassengerId).HasColumnName("PassengerID");

            entity.HasOne(d => d.BookingClass).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.BookingClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_BookingClass");

            entity.HasOne(d => d.Booking).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_Booking");

            entity.HasOne(d => d.Flight).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.FlightId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_Flight");

            entity.HasOne(d => d.MealChoice).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.MealChoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_MealChoice");

            entity.HasOne(d => d.Passenger).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.PassengerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_Passenger");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
