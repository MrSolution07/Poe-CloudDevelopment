-- ============================================================
-- EventEase Database Script
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Venues')
BEGIN
    CREATE TABLE Venues (
        VenueId    INT IDENTITY(1,1) PRIMARY KEY,
        VenueName  NVARCHAR(200)  NOT NULL,
        Location   NVARCHAR(500)  NOT NULL,
        Capacity   INT            NOT NULL CHECK (Capacity > 0),
        ImageUrl   NVARCHAR(1000) NULL
    );
END; 



IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EventTypes')
BEGIN
    CREATE TABLE EventTypes (
        EventTypeId INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(100) NOT NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM EventTypes)
BEGIN
    INSERT INTO EventTypes (Name) VALUES
        ('Conference'), ('Wedding'), ('Concert'), ('Workshop'),
        ('Exhibition'), ('Corporate'), ('Birthday Party'), ('Other');
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Events')
BEGIN
    CREATE TABLE Events (
        EventId     INT IDENTITY(1,1) PRIMARY KEY,
        EventName   NVARCHAR(200)  NOT NULL,
        EventDate   DATETIME2      NOT NULL,
        Description NVARCHAR(2000) NULL,
        ImageUrl    NVARCHAR(1000) NULL,
        VenueId     INT            NULL,
        EventTypeId INT            NULL,
        CONSTRAINT FK_Events_Venues
            FOREIGN KEY (VenueId)
            REFERENCES Venues(VenueId),
        CONSTRAINT FK_Events_EventTypes
            FOREIGN KEY (EventTypeId)
            REFERENCES EventTypes(EventTypeId)
    );
END;


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bookings')
BEGIN
    CREATE TABLE Bookings (
        BookingId   INT IDENTITY(1,1) PRIMARY KEY,
        EventId     INT       NOT NULL,
        VenueId     INT       NOT NULL,
        BookingDate DATE      NOT NULL,
        CONSTRAINT FK_Bookings_Events
            FOREIGN KEY (EventId)
            REFERENCES Events(EventId),
        CONSTRAINT FK_Bookings_Venues
            FOREIGN KEY (VenueId)
            REFERENCES Venues(VenueId),
        CONSTRAINT UQ_Bookings_Event
            UNIQUE (EventId),
        CONSTRAINT UQ_Bookings_Venue_Date
            UNIQUE (VenueId, BookingDate)
    );
END;

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


-- Sample Venues — ImageUrl points at themed local images shipped
-- under EventEaseApp/wwwroot/images/. User-created rows fall back
-- to the neutral venue-fallback.jpg via the application's ImageHelper.
IF NOT EXISTS (SELECT 1 FROM Venues)
BEGIN
    INSERT INTO Venues (VenueName, Location, Capacity, ImageUrl) VALUES
        ('Grand Ballroom',  '123 Main Street, Johannesburg', 500,
         '/images/venue-grand-ballroom.jpg'),
        ('Garden Pavilion', '45 Park Lane, Cape Town',       200,
         '/images/venue-garden-pavilion.jpg'),
        ('Rooftop Terrace', '78 Skyline Drive, Durban',      150,
         '/images/venue-rooftop-terrace.jpg');
END;

-- Sample Events — ImageUrl points at themed local images — see comment above.
IF NOT EXISTS (SELECT 1 FROM Events)
BEGIN
    INSERT INTO Events (EventName, EventDate, Description, VenueId, ImageUrl) VALUES
        ('Tech Summit 2026',    '2026-06-15',
         'Annual technology conference featuring keynote speakers and workshops.',
         1, '/images/event-tech-summit.jpg'),
        ('Spring Wedding Expo', '2026-09-20',
         'Showcase of wedding vendors, venues, and planning services.',
         2, '/images/event-wedding-expo.jpg'),
        ('Jazz Night',          '2026-07-10',
         'An evening of live jazz music under the stars on the rooftop terrace.',
         3, '/images/event-jazz-night.jpg');
END;

-- Sample Bookings
IF NOT EXISTS (SELECT 1 FROM Bookings)
BEGIN
    INSERT INTO Bookings (EventId, VenueId, BookingDate) VALUES
        (1, 1, '2026-06-15'),
        (2, 2, '2026-09-20');
END;

-- Part 2: Consolidated booking view (joins Bookings, Events, Venues).
-- The application's BookingsController.Overview action queries this view
-- via a keyless EF entity (BookingDetailView) when a SQL provider is in use.
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


SELECT 'Venues'   AS TableName, COUNT(*) AS RowCount FROM Venues
UNION ALL
SELECT 'Events',                COUNT(*)              FROM Events
UNION ALL
SELECT 'Bookings',              COUNT(*)              FROM Bookings;