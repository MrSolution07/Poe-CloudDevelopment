# ST10538419 - CLDV7111 Part 3 Submission Checklist

Use this as your step-by-step guide before submitting on Arc/LMS.

## Final Files

- Final Word document name: `ST10538419_CLDV7111_Part3.docx`
- Source report: `ST10538419_CLDV7111_Part3.md`
- GitHub URL: https://github.com/MrSolution07/Poe-CloudDevelopment
- Web app URL: https://st10538419-eventease-ebbpdwa4dsbpg6cs.switzerlandnorth-01.azurewebsites.net/

## 1. Take Screenshot 1 - EventTypes Table

What the marker must see: the Azure SQL Query Editor showing the `EventTypes` table with the eight category rows.

1. Open https://portal.azure.com.
2. Sign in with your Azure student account.
3. In the top search bar, search for **SQL databases**.
4. Open your EventEase SQL database.
5. In the left menu, open **Query editor** or **Query editor (preview)**.
6. Sign in with your SQL server username and password.
7. Paste this query:

```sql
SELECT * FROM EventTypes;
```

8. Click **Run**.
9. Confirm the results show: Conference, Wedding, Concert, Workshop, Exhibition, Corporate, Birthday Party and Other.
10. Take a screenshot:
    - Mac: `Cmd + Shift + 4`, then drag around the query and results.
    - Windows: `Win + Shift + S`, then drag around the query and results.
11. Save as `01_EventTypes_QueryEditor.png`.

If the table is missing or empty, run the relevant `EventTypes` section from `Database/Schema.sql`, then repeat the screenshot.

## 2. Take Screenshot 2 - Venue Availability Field

What the marker must see: the Azure SQL Query Editor showing the `IsAvailable` field in the `Venues` table.

1. Stay in the same Azure SQL Query Editor.
2. Paste this query:

```sql
SELECT VenueId, VenueName, IsAvailable FROM Venues;
```

3. Click **Run**.
4. Confirm the result grid shows venue names and the `IsAvailable` column.
5. Take a screenshot of the query and results.
6. Save as `02_Venues_IsAvailable_QueryEditor.png`.

If `IsAvailable` is missing, run:

```sql
ALTER TABLE Venues ADD IsAvailable BIT NOT NULL DEFAULT 1;
```

Then rerun the `SELECT` query and take the screenshot again.

## 3. Take Screenshot 3 - Booking Overview Filters in the Live App

What the marker must see: the live deployed app showing the Booking Overview page with advanced filters visible.

1. Open the deployed app:
   https://st10538419-eventease-ebbpdwa4dsbpg6cs.switzerlandnorth-01.azurewebsites.net/
2. Click **Sign In**.
3. Sign in as admin.
   - If unchanged, try:
     - Email: `admin@eventease.co.za`
     - Password: `Admin123`
   - If that fails, check Azure App Service -> **Configuration** for `AdminSeed:Email` and `AdminSeed:Password`.
4. Open **Booking Overview** from the navigation menu.
5. Expand **Advanced filters**.
6. Make sure these are visible:
   - Event type dropdown
   - Event date from
   - Event date to
   - Venue availability dropdown
7. Optional but recommended: select one filter and click **Search** so the page shows filtered evidence.
8. Take a screenshot that includes the browser URL, the filters and the booking results table.
9. Save as `03_BookingOverview_Filters.png`.

If there are no booking rows, create one venue, one event with an event type, and one booking. Then return to Booking Overview and repeat the screenshot.

## 4. Optional Screenshot - Consolidated Booking View

This is not explicitly listed under Part 3, but it strengthens the evidence that the Part 2 view still works with Part 3 fields.

1. In Azure SQL Query Editor, run:

```sql
SELECT TOP 5 * FROM vw_BookingDetail;
```

2. Confirm the result includes columns such as `EventTypeName`, `VenueName` and `IsAvailable`.
3. Save the screenshot as `04_vw_BookingDetail_QueryEditor.png`.

If the view is missing, run the `vw_BookingDetail` section from `Database/Schema.sql`.

## 5. Optional Screenshot - Azure App Service Overview

This strengthens deployment evidence.

1. In Azure Portal, search for **App Services**.
2. Open the EventEase App Service.
3. Take a screenshot of the **Overview** blade showing:
   - Status: Running
   - Default domain
   - Resource group
4. Save as `05_Azure_AppService_Overview.png`.

## 6. Build the Word Document

1. Open Microsoft Word.
2. Create a new document.
3. Add the cover information:
   - Student number: ST10538419
   - Module code: CLDV7111
   - Assessment: Portfolio of Evidence - Part 3
   - GitHub URL
   - Web app URL
4. Copy the content from `ST10538419_CLDV7111_Part3.md`.
5. Start each major section on a new page:
   - Section A - Advanced Filtering
   - Section B - Azure Deployment Updates
   - Section C - Reflective Technical Report
   - Code Attribution
   - Reference List
6. Insert Screenshot 1 under Section B.
7. Insert Screenshot 2 under Section B.
8. Insert Screenshot 3 under Section B.
9. Insert optional screenshots if you took them.
10. Keep the Harvard reference list at the end of the document.
11. Save as `ST10538419_CLDV7111_Part3.docx`.

## 7. Rubric Check for 100%

| Rubric item | Evidence to include |
|---|---|
| Advanced filtering | Section A explanation, live app screenshot, codebase with `EventTypes`, date range and availability filters. |
| Azure deployment updates | Web app URL, Azure SQL screenshots for `EventTypes` and `Venues.IsAvailable`, live app screenshot. |
| Feature list | Section C.1 full feature list with explanations. |
| Component discussion | Section C.2 Azure services, technologies, alternatives and theory. |
| Project reflection | Section C.4 journey, challenges, lessons and cloud architecture understanding. |
| Referencing | Harvard-style in-text citations and matching reference list. |

## 8. Final Checks

- Confirm the live web app opens.
- Confirm the GitHub URL opens.
- Confirm the Word document includes all three required screenshots.
- Confirm the reference list is present and formatted consistently.
- Confirm no `.env`, password or connection string is included in the submitted code.
- Submit `ST10538419_CLDV7111_Part3.docx` on Arc/LMS.
