-- ============================================================
-- EventEase Database Schema
-- Cloud Development A (CLDV7111) — Portfolio of Evidence
-- ============================================================
-- This script matches the EF Core-created schema (plural table names).
-- All statements are idempotent (IF NOT EXISTS / CREATE OR ALTER),
-- so running it against an existing database will not break anything.
--
-- Part 1: Core tables (Venues, Events, Bookings) with seed data
-- Part 2: Consolidated booking view (vw_BookingDetail)
-- Part 3: EventTypes lookup, Venues.IsAvailable, Events.EventTypeId,
--          BookingRequests table, and advanced filtering support
-- ============================================================

-- ==================== PART 1: Core Tables ====================
-- Entity Integrity:  Each table uses an INT IDENTITY primary key,
--                    guaranteeing every row is uniquely identifiable.
-- Referential Integrity: Foreign key constraints enforce that every
--   Event references a valid Venue and every Booking references a
--   valid Event and Venue.

-- 1.1 Venues table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Venues')
BEGIN
    CREATE TABLE Venues (
        VenueId       INT IDENTITY(1,1) PRIMARY KEY,  -- Entity integrity (PK)
        VenueName     NVARCHAR(200)  NOT NULL,
        Location      NVARCHAR(500)  NOT NULL,
        Capacity      INT            NOT NULL CHECK (Capacity > 0),
        ImageUrl      NVARCHAR(1000) NULL,
        IsAvailable   BIT            NOT NULL DEFAULT 1
    );
END;

-- 1.2 EventTypes lookup table (required before Events due to FK)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EventTypes')
BEGIN
    CREATE TABLE EventTypes (
        EventTypeId   INT IDENTITY(1,1) PRIMARY KEY,  -- Entity integrity (PK)
        Name          NVARCHAR(100)  NOT NULL
    );
END;

-- Seed EventType lookup values
IF NOT EXISTS (SELECT 1 FROM EventTypes)
BEGIN
    INSERT INTO EventTypes (Name) VALUES
        ('Conference'),
        ('Wedding'),
        ('Concert'),
        ('Workshop'),
        ('Exhibition'),
        ('Corporate'),
        ('Birthday Party'),
        ('Other');
END;

-- 1.3 Events table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Events')
BEGIN
    CREATE TABLE Events (
        EventId       INT IDENTITY(1,1) PRIMARY KEY,  -- Entity integrity (PK)
        EventName     NVARCHAR(200)  NOT NULL,
        EventDate     DATETIME2      NOT NULL,
        Description   NVARCHAR(2000) NULL,
        ImageUrl      NVARCHAR(1000) NULL,
        VenueId       INT            NULL,
        EventTypeId   INT            NULL,
        CONSTRAINT FK_Events_Venues FOREIGN KEY (VenueId)
            REFERENCES Venues(VenueId),                -- Referential integrity
        CONSTRAINT FK_Events_EventTypes FOREIGN KEY (EventTypeId)
            REFERENCES EventTypes(EventTypeId)         -- Referential integrity
    );
END;

-- 1.4 Bookings table (associative entity linking Venues and Events)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bookings')
BEGIN
    CREATE TABLE Bookings (
        BookingId     INT IDENTITY(1,1) PRIMARY KEY,   -- Entity integrity (PK)
        EventId       INT            NOT NULL,
        VenueId       INT            NOT NULL,
        BookingDate   DATE           NOT NULL,
        CONSTRAINT FK_Bookings_Events FOREIGN KEY (EventId)
            REFERENCES Events(EventId),                -- Referential integrity
        CONSTRAINT FK_Bookings_Venues FOREIGN KEY (VenueId)
            REFERENCES Venues(VenueId),                -- Referential integrity
        CONSTRAINT UQ_Bookings_Event UNIQUE (EventId),
        CONSTRAINT UQ_Bookings_Venue_Date UNIQUE (VenueId, BookingDate)
            -- Prevents double-booking the same venue on the same date
    );
END;

-- If the table already exists, enforce date-only storage and uniqueness rules
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bookings')
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.columns c
        INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID('Bookings')
          AND c.name = 'BookingDate'
          AND t.name <> 'date'
    )
    BEGIN
        ALTER TABLE Bookings ALTER COLUMN BookingDate DATE NOT NULL;
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Bookings_Event')
    BEGIN
        ALTER TABLE Bookings ADD CONSTRAINT UQ_Bookings_Event UNIQUE (EventId);
    END;
END;

-- 1.5 BookingRequests table for public enquiries
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BookingRequests')
BEGIN
    CREATE TABLE BookingRequests (
        BookingRequestId INT IDENTITY(1,1) PRIMARY KEY,  -- Entity integrity (PK)
        CustomerName     NVARCHAR(200)  NOT NULL,
        Email            NVARCHAR(200)  NOT NULL,
        Phone            NVARCHAR(20)   NULL,
        PreferredVenueId INT            NULL,
        PreferredDate    DATETIME2      NULL,
        Message          NVARCHAR(2000) NOT NULL,
        Status           NVARCHAR(20)   NOT NULL DEFAULT 'Pending',
        CreatedAt        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_BookingRequests_Venues FOREIGN KEY (PreferredVenueId)
            REFERENCES Venues(VenueId)                 -- Referential integrity
    );
END;

-- ==================== PART 1: Seed Data ====================

-- Seed sample venues (placeholder image URLs)
IF NOT EXISTS (SELECT 1 FROM Venues)
BEGIN
    SET IDENTITY_INSERT Venues ON;
    INSERT INTO Venues (VenueId, VenueName, Location, Capacity, ImageUrl, IsAvailable) VALUES
        (1, 'Grand Ballroom',  '123 Main Street, Johannesburg', 500,
         'https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=600', 1),
        (2, 'Garden Pavilion', '45 Park Lane, Cape Town',       200,
         'https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=600', 1),
        (3, 'Rooftop Terrace', '78 Skyline Drive, Durban',      150,
         'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?w=600', 1);
    SET IDENTITY_INSERT Venues OFF;
END;

-- Seed sample events (placeholder image URLs)
IF NOT EXISTS (SELECT 1 FROM Events)
BEGIN
    SET IDENTITY_INSERT Events ON;
    INSERT INTO Events (EventId, EventName, EventDate, Description, VenueId, EventTypeId, ImageUrl) VALUES
        (1, 'Tech Summit 2026',    '2026-06-15',
         'Annual technology conference featuring keynote speakers and workshops.',
         1, 1, 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=600'),
        (2, 'Spring Wedding Expo', '2026-09-20',
         'Showcase of wedding vendors, venues, and planning services.',
         2, 2, 'https://images.unsplash.com/photo-1519741497674-611481863552?w=600'),
        (3, 'Jazz Night',          '2026-07-10',
         'An evening of live jazz music under the stars on the rooftop terrace.',
         3, 3, 'https://images.unsplash.com/photo-1511192336575-5a79af67a629?w=600');
    SET IDENTITY_INSERT Events OFF;
END;

-- Seed sample bookings
IF NOT EXISTS (SELECT 1 FROM Bookings)
BEGIN
    SET IDENTITY_INSERT Bookings ON;
    INSERT INTO Bookings (BookingId, EventId, VenueId, BookingDate) VALUES
        (1, 1, 1, '2026-06-15'),
        (2, 2, 2, '2026-09-20');
    SET IDENTITY_INSERT Bookings OFF;
END;

-- ==================== PART 2: Consolidated Booking View ====================
-- This view joins Bookings, Events, and Venues into a single result set,
-- allowing booking specialists to see all relevant information at a glance.
-- The application searches this view by BookingId or EventName.
GO
CREATE OR ALTER VIEW vw_BookingDetail AS
SELECT
    b.BookingId,
    b.BookingDate,
    e.EventId,
    e.EventName,
    e.EventDate,
    e.Description   AS EventDescription,
    e.ImageUrl      AS EventImageUrl,
    et.Name         AS EventTypeName,
    v.VenueId,
    v.VenueName,
    v.Location,
    v.Capacity,
    v.ImageUrl      AS VenueImageUrl,
    v.IsAvailable
FROM Bookings b
INNER JOIN Events    e  ON b.EventId     = e.EventId
INNER JOIN Venues    v  ON b.VenueId     = v.VenueId
LEFT  JOIN EventTypes et ON e.EventTypeId = et.EventTypeId;
GO
