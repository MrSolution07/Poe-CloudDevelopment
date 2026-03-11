-- ============================================================
-- EventEase Database Schema
-- Cloud Development A (CLDV7111) — Portfolio of Evidence
-- ============================================================
-- Part 1: Core tables (Venue, Event, Booking) with seed data
-- Part 2: Consolidated booking view (vw_BookingDetail)
-- Part 3: EventType lookup table, Venue.IsAvailable field,
--          Event.EventTypeId field, and advanced filtering support
-- ============================================================

-- ==================== PART 1: Core Tables ====================
-- Entity Integrity: Each table uses an INT IDENTITY primary key,
-- guaranteeing every row is uniquely identifiable.
-- Referential Integrity: Foreign key constraints enforce that
-- every Event references a valid Venue and every Booking
-- references a valid Event and Venue.

-- 1.1 Venue table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Venue')
BEGIN
    CREATE TABLE Venue (
        VenueId       INT IDENTITY(1,1) PRIMARY KEY,  -- Entity integrity (PK)
        VenueName     NVARCHAR(200)  NOT NULL,
        Location      NVARCHAR(500)  NOT NULL,
        Capacity      INT            NOT NULL CHECK (Capacity > 0),
        ImageUrl      NVARCHAR(1000) NULL
    );
END;

-- 1.2 Event table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Event')
BEGIN
    CREATE TABLE [Event] (
        EventId       INT IDENTITY(1,1) PRIMARY KEY,  -- Entity integrity (PK)
        EventName     NVARCHAR(200)  NOT NULL,
        EventDate     DATETIME2      NOT NULL,
        Description   NVARCHAR(MAX)  NULL,
        ImageUrl      NVARCHAR(1000) NULL,
        VenueId       INT            NULL,
        CONSTRAINT FK_Event_Venue FOREIGN KEY (VenueId)
            REFERENCES Venue(VenueId)                  -- Referential integrity
    );
END;

-- 1.3 Booking table (associative entity linking Venue and Event)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Booking')
BEGIN
    CREATE TABLE Booking (
        BookingId     INT IDENTITY(1,1) PRIMARY KEY,   -- Entity integrity (PK)
        EventId       INT            NOT NULL,
        VenueId       INT            NOT NULL,
        BookingDate   DATETIME2      NOT NULL,
        CONSTRAINT FK_Booking_Event FOREIGN KEY (EventId)
            REFERENCES [Event](EventId),               -- Referential integrity
        CONSTRAINT FK_Booking_Venue FOREIGN KEY (VenueId)
            REFERENCES Venue(VenueId),                 -- Referential integrity
        CONSTRAINT UQ_Venue_BookingDate UNIQUE (VenueId, BookingDate)
            -- Prevents double-booking the same venue on the same date
    );
END;

-- ==================== PART 1: Seed Data ====================

-- Seed sample venues (placeholder image URLs for Part 1)
IF NOT EXISTS (SELECT 1 FROM Venue)
BEGIN
    SET IDENTITY_INSERT Venue ON;
    INSERT INTO Venue (VenueId, VenueName, Location, Capacity, ImageUrl) VALUES
        (1, 'Grand Ballroom',   '123 Main Street, Johannesburg', 500,
         'https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=600'),
        (2, 'Garden Pavilion',  '45 Park Lane, Cape Town',       200,
         'https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=600'),
        (3, 'Rooftop Terrace',  '78 Skyline Drive, Durban',      150,
         'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?w=600');
    SET IDENTITY_INSERT Venue OFF;
END;

-- Seed sample events (placeholder image URLs for Part 1)
IF NOT EXISTS (SELECT 1 FROM [Event])
BEGIN
    SET IDENTITY_INSERT [Event] ON;
    INSERT INTO [Event] (EventId, EventName, EventDate, Description, VenueId, ImageUrl) VALUES
        (1, 'Tech Summit 2026', '2026-06-15',
         'Annual technology conference featuring keynote speakers and workshops.',
         1, 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=600'),
        (2, 'Spring Wedding Expo', '2026-09-20',
         'Showcase of wedding vendors, venues, and planning services.',
         2, 'https://images.unsplash.com/photo-1519741497674-611481863552?w=600'),
        (3, 'Jazz Night', '2026-07-10',
         'An evening of live jazz music under the stars on the rooftop terrace.',
         3, 'https://images.unsplash.com/photo-1511192336575-5a79af67a629?w=600');
    SET IDENTITY_INSERT [Event] OFF;
END;

-- Seed sample bookings
IF NOT EXISTS (SELECT 1 FROM Booking)
BEGIN
    SET IDENTITY_INSERT Booking ON;
    INSERT INTO Booking (BookingId, EventId, VenueId, BookingDate) VALUES
        (1, 1, 1, '2026-06-15'),
        (2, 2, 2, '2026-09-20');
    SET IDENTITY_INSERT Booking OFF;
END;

-- ==================== PART 2: Consolidated Booking View ====================
-- This view joins Booking, Event, and Venue data into a single result set,
-- allowing booking specialists to see all relevant information at a glance.
-- A search function in the application queries this view by BookingId or EventName.

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
    v.VenueId,
    v.VenueName,
    v.Location,
    v.Capacity,
    v.ImageUrl      AS VenueImageUrl
FROM Booking b
INNER JOIN [Event] e  ON b.EventId  = e.EventId
INNER JOIN Venue v    ON b.VenueId  = v.VenueId;
GO

-- ==================== PART 3: EventType Lookup & Filtering ====================

-- 3.1 EventType lookup table with predefined categories
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EventType')
BEGIN
    CREATE TABLE EventType (
        EventTypeId   INT IDENTITY(1,1) PRIMARY KEY,   -- Entity integrity (PK)
        EventTypeName NVARCHAR(100)  NOT NULL
    );
END;

-- Seed EventType lookup values
IF NOT EXISTS (SELECT 1 FROM EventType)
BEGIN
    INSERT INTO EventType (EventTypeName) VALUES
        ('Conference'),
        ('Wedding'),
        ('Concert'),
        ('Workshop'),
        ('Exhibition'),
        ('Corporate'),
        ('Birthday Party'),
        ('Other');
END;

-- 3.2 Add IsAvailable field to Venue for filtering by venue availability
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('Venue') AND name = 'IsAvailable'
)
BEGIN
    ALTER TABLE Venue ADD IsAvailable BIT NOT NULL DEFAULT 1;
END;

-- 3.3 Add EventTypeId foreign key to Event for classification
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('[Event]') AND name = 'EventTypeId'
)
BEGIN
    ALTER TABLE [Event] ADD EventTypeId INT NULL;
    ALTER TABLE [Event] ADD CONSTRAINT FK_Event_EventType
        FOREIGN KEY (EventTypeId) REFERENCES EventType(EventTypeId);
END;

-- Update seed events with event type categories
UPDATE [Event] SET EventTypeId = 1 WHERE EventId = 1 AND EventTypeId IS NULL; -- Conference
UPDATE [Event] SET EventTypeId = 2 WHERE EventId = 2 AND EventTypeId IS NULL; -- Wedding
UPDATE [Event] SET EventTypeId = 3 WHERE EventId = 3 AND EventTypeId IS NULL; -- Concert

-- 3.4 Update the consolidated view to include EventType and availability
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
    et.EventTypeName,
    v.VenueId,
    v.VenueName,
    v.Location,
    v.Capacity,
    v.ImageUrl      AS VenueImageUrl,
    v.IsAvailable
FROM Booking b
INNER JOIN [Event] e     ON b.EventId      = e.EventId
INNER JOIN Venue v       ON b.VenueId      = v.VenueId
LEFT  JOIN EventType et  ON e.EventTypeId  = et.EventTypeId;
GO
