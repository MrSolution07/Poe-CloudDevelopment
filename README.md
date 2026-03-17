# EventEase

Event and venue booking management system. Built for CLDV7111 (Cloud Development A) and hosted on Microsoft Azure.

---

## Overview

EventEase replaces a manual, spreadsheet-based process for managing venue bookings. It provides:

- A public landing page with featured venues and upcoming events
- A public booking request form (no login required)
- Admin authentication and role-based access
- Full CRUD for Venues, Events, and Bookings
- Management of public booking requests (Pending / Processed)
- Image upload for venues and events via Azure Blob Storage

---

## Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB for local development, or Azure SQL for production)
- Azure account (for production deployment: App Service, SQL Database, Blob Storage)

---

## Technology Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 10 |
| Web framework | ASP.NET Core MVC |
| ORM | Entity Framework Core 10 |
| Database | SQL Server / Azure SQL Database |
| Authentication | ASP.NET Core Identity |
| Image storage | Azure Blob Storage (Azure.Storage.Blobs 12.x) |
| Front-end | Razor views, Bootstrap 5, jQuery |

---

## Login Credentials

The application seeds a single admin user on first run.

| Purpose | Email | Password |
|---------|--------|----------|
| Admin (full access) | admin@eventease.co.za | Admin123 |

Use these credentials to log in and access the dashboard, venues, events, bookings, and booking requests. New users can register via the Register page; they do not receive the Admin role unless granted separately.

---

## Running Locally

1. Clone the repository and open the solution (e.g. `EventEase.slnx` or the folder containing `EventEaseApp`).

2. Apply the database schema. Either:
   - Run `Database/Schema.sql` against a SQL Server instance (LocalDB or full SQL Server), or
   - Let the app run with the in-memory database (no SQL Server required; data is lost on restart).

3. Configure local overrides (optional). For a real database or Blob Storage locally, use one of:
   - `EventEaseApp/appsettings.Development.local.json` (gitignored), or
   - .NET User Secrets:  
     `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=EventEaseDB;..." --project EventEaseApp`  
     `dotnet user-secrets set "AzureBlobStorage:ConnectionString" "DefaultEndpointsProtocol=https;..." --project EventEaseApp`

4. Run the application:
   ```bash
   cd EventEaseApp
   dotnet run
   ```
   Open https://localhost:5001 or http://localhost:5000 (or the URLs shown in the console).

5. Log in with the credentials in the table above.

---

## Configuration

The application reads configuration from (in order of precedence):

- Environment variables (e.g. Azure App Service Application settings and Connection strings)
- `appsettings.{Environment}.json`
- `appsettings.json`

Relevant keys:

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string. Required for persistent data; if missing or pointing to LocalDB only, the app may use an in-memory database. |
| `ConnectionStrings:AzureBlobStorage` | Azure Blob Storage connection string. Used for venue and event image uploads. If not set, image upload is disabled; the UI accepts image URLs instead. |
| `AzureBlobStorage:ConnectionString` | Alternative app-setting name for the Blob connection string (e.g. `AzureBlobStorage__ConnectionString` in Azure). Empty string in `appsettings.json` must not override the value from Azure; the code prefers `GetConnectionString("AzureBlobStorage")` first. |
| `AzureBlobStorage:VenueContainerName` | Blob container for venue images (default: `venue-images`). |
| `AzureBlobStorage:EventContainerName` | Blob container for event images (default: `event-images`). |

Do not commit connection strings or secrets to the repository. Use User Secrets locally and Azure Application settings / Connection strings in production.

---

## Azure Deployment

Production uses:

- **Azure App Service** (Web App) to host the ASP.NET Core application
- **Azure SQL Database** for the main database
- **Azure Blob Storage** for venue and event images

Configure the Web App in Azure Portal:

1. **Connection strings**
   - `DefaultConnection`: type SQL Azure; value = your Azure SQL connection string.
   - `AzureBlobStorage`: type Custom; value = your Blob Storage connection string (from Storage account → Access keys).

2. **Application settings** (optional)
   - `AzureBlobStorage__ConnectionString`: alternative to the Connection string above; use if you prefer an app setting over a connection string entry.

After configuration, deploy the application (e.g. via Visual Studio Publish, `az webapp deploy` with a zip of the publish output, or GitHub Actions). See `docs/DEPLOY_WEBAPP_ST10538419.md` and `docs/AZURE_DEPLOYMENT.md` for step-by-step instructions.

---

## Project Structure

| Path | Description |
|------|-------------|
| `EventEaseApp/` | ASP.NET Core MVC application (Controllers, Views, Models, Services, wwwroot) |
| `EventEaseApp/Services/BlobStorageService.cs` | Azure Blob upload/delete for venue and event images |
| `Database/Schema.sql` | SQL schema and seed data (Venues, Events, EventTypes, Bookings, BookingRequests, vw_BookingDetail) |
| `docs/` | Deployment guides, ERD, POE answers, and other documentation |
| `deploy-to-azure.sh` | Script to build and deploy the app to Azure Web App via zip deploy |

---

## Database

Main tables: **Venues**, **Events**, **EventTypes**, **Bookings**, **BookingRequests**. The view **vw_BookingDetail** joins Bookings, Events, Venues, and EventTypes for the booking overview. Run `Database/Schema.sql` against your SQL Server or Azure SQL database to create tables, constraints, seed data, and the view.

---

## License and Use

This project was developed for academic purposes (CLDV7111). Use the login credentials only in a controlled environment; change default passwords in any production or shared deployment.
