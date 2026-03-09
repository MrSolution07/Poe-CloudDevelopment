-- ============================================================
-- EventEase Database Schema
-- Part 1: Core tables (Venue, Event, Booking)
-- Part 2: Consolidated booking view (vw_BookingDetail)
-- Part 3: EventType lookup, Venue.IsAvailable, Event.EventTypeId
-- ============================================================

-- ==================== PART 1: Core Tables ====================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Venue')
BEGIN
    CREATE TABLE Venue (
        VenueId       INT IDENTITY(1,1) PRIMARY KEY,
        VenueName     NVARCHAR(200)  NOT NULL,
        Location      NVARCHAR(500)  NOT NULL,
        Capacity      INT            NOT NULL CHECK (Capacity > 0),
        ImageUrl      NVARCHAR(1000) NULL,
        IsAvailable   BIT            NOT NULL DEFAULT 1
    );
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EventType')
BEGIN
    CREATE TABLE EventType (
        EventTypeId   INT IDENTITY(1,1) PRIMARY KEY,
        Name          NVARCHAR(100)  NOT NULL
    );
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Event')
BEGIN
    CREATE TABLE [Event] (
        EventId       INT IDENTITY(1,1) PRIMARY KEY,
        EventName     NVARCHAR(200)  NOT NULL,
        EventDate     DATETIME2      NOT NULL,
        Description   NVARCHAR(MAX)  NULL,
        ImageUrl      NVARCHAR(1000) NULL,
        VenueId       INT            NULL,
        EventTypeId   INT            NULL,
        CONSTRAINT FK_Event_Venue     FOREIGN KEY (VenueId)     REFERENCES Venue(VenueId),
        CONSTRAINT FK_Event_EventType FOREIGN KEY (EventTypeId) REFERENCES EventType(EventTypeId)
    );
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Booking')
BEGIN
    CREATE TABLE Booking (
        BookingId     INT IDENTITY(1,1) PRIMARY KEY,
        EventId       INT            NOT NULL,
        VenueId       INT            NOT NULL,
        BookingDate   DATETIME2      NOT NULL,
        CONSTRAINT FK_Booking_Event FOREIGN KEY (EventId) REFERENCES [Event](EventId),
        CONSTRAINT FK_Booking_Venue FOREIGN KEY (VenueId) REFERENCES Venue(VenueId),
        CONSTRAINT UQ_Venue_BookingDate UNIQUE (VenueId, BookingDate)
    );
END;

-- ==================== SEED DATA ====================

-- Seed EventType lookup values
IF NOT EXISTS (SELECT 1 FROM EventType)
BEGIN
    INSERT INTO EventType (Name) VALUES
        ('Conference'),
        ('Wedding'),
        ('Concert'),
        ('Workshop'),
        ('Exhibition'),
        ('Corporate'),
        ('Birthday Party'),
        ('Other');
END;

-- Seed sample venues
IF NOT EXISTS (SELECT 1 FROM Venue)
BEGIN
    SET IDENTITY_INSERT Venue ON;
    INSERT INTO Venue (VenueId, VenueName, Location, Capacity, ImageUrl, IsAvailable) VALUES
        (1, 'Grand Ballroom',   '123 Main Street, Johannesburg', 500,
         'https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=600', 1),
        (2, 'Garden Pavilion',  '45 Park Lane, Cape Town',       200,
         'https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=600', 1),
        (3, 'Rooftop Terrace',  '78 Skyline Drive, Durban',      150,
         'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?w=600', 1);
    SET IDENTITY_INSERT Venue OFF;
END;

-- Seed sample events
IF NOT EXISTS (SELECT 1 FROM [Event])
BEGIN
    SET IDENTITY_INSERT [Event] ON;
    INSERT INTO [Event] (EventId, EventName, EventDate, Description, VenueId, EventTypeId, ImageUrl) VALUES
        (1, 'Tech Summit 2026', '2026-06-15',
         'Annual technology conference featuring keynote speakers and workshops.',
         1, 1, 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=600'),
        (2, 'Spring Wedding Expo', '2026-09-20',
         'Showcase of wedding vendors, venues, and planning services.',
         2, 2, 'https://images.unsplash.com/photo-1519741497674-611481863552?w=600');
    SET IDENTITY_INSERT [Event] OFF;
END;

-- Seed sample booking
IF NOT EXISTS (SELECT 1 FROM Booking)
BEGIN
    SET IDENTITY_INSERT Booking ON;
    INSERT INTO Booking (BookingId, EventId, VenueId, BookingDate) VALUES
        (1, 1, 1, '2026-06-15');
    SET IDENTITY_INSERT Booking OFF;
END;

-- ==================== PART 2: Consolidated Booking View ====================

GO
CREATE OR ALTER VIEW vw_BookingDetail AS
SELECT
    b.BookingId,
    b.BookingDate,
    e.EventId,
    e.EventName,
    e.EventDate,
    e.Description,
    et.Name        AS EventTypeName,
    v.VenueId,
    v.VenueName,
    v.Location,
    v.Capacity,
    v.ImageUrl,
    v.IsAvailable
FROM Booking b
INNER JOIN [Event] e  ON b.EventId  = e.EventId
INNER JOIN Venue v    ON b.VenueId  = v.VenueId
LEFT  JOIN EventType et ON e.EventTypeId = et.EventTypeId;
GO
