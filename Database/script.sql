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



IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Events')
BEGIN
    CREATE TABLE Events (
        EventId     INT IDENTITY(1,1) PRIMARY KEY,
        EventName   NVARCHAR(200)  NOT NULL,
        EventDate   DATETIME2      NOT NULL,
        Description NVARCHAR(2000) NULL,
        ImageUrl    NVARCHAR(1000) NULL,
        VenueId     INT            NULL,
        CONSTRAINT FK_Events_Venues
            FOREIGN KEY (VenueId)
            REFERENCES Venues(VenueId)
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


-- Sample Venues
IF NOT EXISTS (SELECT 1 FROM Venues)
BEGIN
    INSERT INTO Venues (VenueName, Location, Capacity, ImageUrl) VALUES
        ('Grand Ballroom', '123 Main Street, Johannesburg', 500,
         'https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=600'),
        ('Garden Pavilion', '45 Park Lane, Cape Town', 200,
         'https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=600'),
        ('Rooftop Terrace', '78 Skyline Drive, Durban', 150,
         'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?w=600');
END;

-- Sample Events
IF NOT EXISTS (SELECT 1 FROM Events)
BEGIN
    INSERT INTO Events (EventName, EventDate, Description, VenueId, ImageUrl) VALUES
        ('Tech Summit 2026', '2026-06-15',
         'Annual technology conference featuring keynote speakers and workshops.',
         1, 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=600'),
        ('Spring Wedding Expo', '2026-09-20',
         'Showcase of wedding vendors, venues, and planning services.',
         2, 'https://images.unsplash.com/photo-1519741497674-611481863552?w=600'),
        ('Jazz Night', '2026-07-10',
         'An evening of live jazz music under the stars on the rooftop terrace.',
         3, 'https://images.unsplash.com/photo-1511192336575-5a79af67a629?w=600');
END;

-- Sample Bookings
IF NOT EXISTS (SELECT 1 FROM Bookings)
BEGIN
    INSERT INTO Bookings (EventId, VenueId, BookingDate) VALUES
        (1, 1, '2026-06-15'),
        (2, 2, '2026-09-20');
END;


SELECT 'Venues'   AS TableName, COUNT(*) AS RowCount FROM Venues
UNION ALL
SELECT 'Events',                COUNT(*)              FROM Events
UNION ALL
SELECT 'Bookings',              COUNT(*)              FROM Bookings;