using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventEaseApp.Models;

namespace EventEaseApp.Data;

public class EventEaseContext : IdentityDbContext<ApplicationUser>
{
    public EventEaseContext(DbContextOptions<EventEaseContext> options) : base(options) { }

    public DbSet<Venue> Venues { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<EventType> EventTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(v => v.VenueId);
            entity.Property(v => v.VenueName).IsRequired().HasMaxLength(200);
            entity.Property(v => v.Location).IsRequired().HasMaxLength(500);
            entity.Property(v => v.ImageUrl).HasMaxLength(1000);
            entity.Property(v => v.IsAvailable).HasDefaultValue(true);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.EventName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);

            entity.HasOne(e => e.Venue)
                  .WithMany(v => v.Events)
                  .HasForeignKey(e => e.VenueId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.EventType)
                  .WithMany(et => et.Events)
                  .HasForeignKey(e => e.EventTypeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.BookingId);

            entity.HasOne(b => b.Event)
                  .WithMany(e => e.Bookings)
                  .HasForeignKey(b => b.EventId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Venue)
                  .WithMany(v => v.Bookings)
                  .HasForeignKey(b => b.VenueId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(b => new { b.VenueId, b.BookingDate }).IsUnique();
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.HasKey(et => et.EventTypeId);
            entity.Property(et => et.Name).IsRequired().HasMaxLength(100);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventType>().HasData(
            new EventType { EventTypeId = 1, Name = "Conference" },
            new EventType { EventTypeId = 2, Name = "Wedding" },
            new EventType { EventTypeId = 3, Name = "Concert" },
            new EventType { EventTypeId = 4, Name = "Workshop" },
            new EventType { EventTypeId = 5, Name = "Exhibition" },
            new EventType { EventTypeId = 6, Name = "Corporate" },
            new EventType { EventTypeId = 7, Name = "Birthday Party" },
            new EventType { EventTypeId = 8, Name = "Other" }
        );

        modelBuilder.Entity<Venue>().HasData(
            new Venue
            {
                VenueId = 1,
                VenueName = "Grand Ballroom",
                Location = "123 Main Street, Johannesburg",
                Capacity = 500,
                ImageUrl = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=600",
                IsAvailable = true
            },
            new Venue
            {
                VenueId = 2,
                VenueName = "Garden Pavilion",
                Location = "45 Park Lane, Cape Town",
                Capacity = 200,
                ImageUrl = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=600",
                IsAvailable = true
            },
            new Venue
            {
                VenueId = 3,
                VenueName = "Rooftop Terrace",
                Location = "78 Skyline Drive, Durban",
                Capacity = 150,
                ImageUrl = "https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?w=600",
                IsAvailable = true
            }
        );

        modelBuilder.Entity<Event>().HasData(
            new Event
            {
                EventId = 1,
                EventName = "Tech Summit 2026",
                EventDate = new DateTime(2026, 6, 15),
                Description = "Annual technology conference featuring keynote speakers and workshops.",
                VenueId = 1,
                EventTypeId = 1,
                ImageUrl = "https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=600"
            },
            new Event
            {
                EventId = 2,
                EventName = "Spring Wedding Expo",
                EventDate = new DateTime(2026, 9, 20),
                Description = "Showcase of wedding vendors, venues, and planning services.",
                VenueId = 2,
                EventTypeId = 2,
                ImageUrl = "https://images.unsplash.com/photo-1519741497674-611481863552?w=600"
            },
            new Event
            {
                EventId = 3,
                EventName = "Jazz Night",
                EventDate = new DateTime(2026, 7, 10),
                Description = "An evening of live jazz music under the stars on the rooftop terrace.",
                VenueId = 3,
                EventTypeId = 3,
                ImageUrl = "https://images.unsplash.com/photo-1511192336575-5a79af67a629?w=600"
            }
        );

        modelBuilder.Entity<Booking>().HasData(
            new Booking
            {
                BookingId = 1,
                EventId = 1,
                VenueId = 1,
                BookingDate = new DateTime(2026, 6, 15)
            },
            new Booking
            {
                BookingId = 2,
                EventId = 2,
                VenueId = 2,
                BookingDate = new DateTime(2026, 9, 20)
            }
        );
    }
}
