# ST10538419 — CLDV7111 Part 3 Submission Checklist

Use this as your step-by-step guide. Tick each box as you complete it.

**Final deliverable:** One Word document named `ST10538419_CLDV7111_Part3.docx` submitted on Arc/LMS.

**Your links (copy these exactly):**

| Item | URL |
|---|---|
| GitHub | https://github.com/MrSolution07/Poe-CloudDevelopment |
| Live app | https://st10538419-eventease-ebbpdwa4dsbpg6cs.switzerlandnorth-01.azurewebsites.net/ |

---

## Phase 1 — Fix the source document before converting to Word

- [ ] Open `ST10538419_CLDV7111_Part3.md`
- [ ] Replace the placeholder GitHub URL with: `https://github.com/MrSolution07/Poe-CloudDevelopment`
- [ ] Confirm the deployed web app URL is correct (see table above)
- [ ] Re-read Sections A, B, and C — content is already written; you are mainly adding screenshots and fixing the URL

---

## Phase 2 — Take all screenshots (do this before building the Word doc)

Complete the three screenshot guides below. Save images to a folder such as `Part3_Screenshots/` with clear names:

- `01_EventTypes_QueryEditor.png`
- `02_Venues_IsAvailable_QueryEditor.png`
- `03_BookingOverview_Filters.png`

Optional (recommended for stronger Section B evidence):

- `04_vw_BookingDetail_QueryEditor.png`
- `05_Azure_AppService_Overview.png`

---

### Screenshot Guide 1 — EventTypes table in Azure SQL Query Editor

**What the marker needs to see:** 8 rows — Conference, Wedding, Concert, Workshop, Exhibition, Corporate, Birthday Party, Other.

1. Open a browser and go to [https://portal.azure.com](https://portal.azure.com)
2. Sign in with your Azure student account
3. In the top search bar, type **SQL databases** and open it
4. Click your EventEase database (likely named **EventEaseDB** or similar under resource group **EventEase-rg**)
5. In the left menu, under **Query editor (preview)** or **Query editor**, click **Query editor**
6. Sign in with SQL authentication if prompted:
   - Use the SQL server admin username and password you set when creating Azure SQL
   - If you forgot them: SQL server → **Reset password** in Azure Portal
7. In the query pane, paste and run:

```sql
SELECT * FROM EventTypes;
```

8. Wait for results in the bottom panel — you should see **8 rows**
9. Capture the screenshot:
   - **Mac:** `Cmd + Shift + 4`, drag to select the query + results
   - **Windows:** `Win + Shift + S`, select the area
   - Include: the SQL text, the results grid, and enough of the Azure Portal header so it is clearly Azure SQL Query Editor
10. Save as `01_EventTypes_QueryEditor.png`

**If the table is empty or missing:**

1. Redeploy/restart the web app so EF Core `EnsureCreated()` and seed data run, **or**
2. Open `Database/Schema.sql` from your repo, copy the `EventTypes` CREATE + INSERT section, run it in Query Editor, then re-run `SELECT * FROM EventTypes;`

---

### Screenshot Guide 2 — Venues.IsAvailable column in Azure SQL Query Editor

**What the marker needs to see:** Venue rows with an **IsAvailable** column (values 0/1 or true/false).

1. Stay in the same Azure SQL Query Editor (same steps 1–6 as Guide 1 if you closed it)
2. Paste and run:

```sql
SELECT VenueId, VenueName, IsAvailable FROM Venues;
```

3. Confirm results show venues (e.g. Grand Ballroom, Garden Pavilion, Rooftop Terrace) with **IsAvailable**
4. Capture screenshot (query + results grid)
5. Save as `02_Venues_IsAvailable_QueryEditor.png`

**If IsAvailable column is missing:**

1. Run the `Venues` table section from `Database/Schema.sql`, **or**
2. Run: `ALTER TABLE Venues ADD IsAvailable BIT NOT NULL DEFAULT 1;`
3. Re-run the SELECT and screenshot again

---

### Screenshot Guide 3 — Live app Booking Overview with Advanced Filters

**What the marker needs to see:** The Booking Overview page with filter controls visible (Event Type, Date From/To, Venue Availability) and booking results.

1. Open: https://st10538419-eventease-ebbpdwa4dsbpg6cs.switzerlandnorth-01.azurewebsites.net/
2. Click **Login** (or go directly to `/Account/Login`)
3. Sign in with your admin account:
   - **Default seeded login** (if you did not change Azure App Settings):  
     - Email: `admin@eventease.co.za`  
     - Password: `Admin123`
   - If that fails, check Azure Portal → your Web App → **Configuration** → **Application settings** for `AdminSeed:Email` and `AdminSeed:Password`
4. After login, go to **Bookings** → **Overview** (or open `/Bookings/Overview`)
5. Expand or scroll to the **Advanced Filters** section so these are visible:
   - Event Type dropdown
   - Event Date From / Event Date To
   - Venue Availability dropdown
6. (Recommended) Select a filter and click **Search** or **Apply** so the page shows filtered results — proves filters work
7. Capture a full-browser screenshot showing:
   - The page URL in the address bar (`.../Bookings/Overview`)
   - The filter controls
   - At least one booking row in the table (if data exists)
8. Save as `03_BookingOverview_Filters.png`

**If you have no bookings to display:**

1. Log in as admin
2. Create at least one Venue, Event (with an Event Type), and Booking
3. Return to Booking Overview and screenshot again

---

### Screenshot Guide 4 (Optional) — vw_BookingDetail view

**What this shows:** Part 2 view still works and includes event/venue/type columns.

1. In Azure SQL Query Editor, run:

```sql
SELECT TOP 5 * FROM vw_BookingDetail;
```

2. Confirm columns such as `EventTypeName`, `VenueName`, `IsAvailable` appear
3. Screenshot and save as `04_vw_BookingDetail_QueryEditor.png`

**If the view does not exist:** Open `Database/Schema.sql`, find the `vw_BookingDetail` section, run it in Query Editor, then re-run the SELECT.

---

### Screenshot Guide 5 (Optional) — Azure App Service running

1. Azure Portal → search **App Services**
2. Open **st10538419-eventease** (or your app name)
3. Screenshot the **Overview** blade showing:
   - Status: **Running**
   - Default domain URL matching your submission URL
4. Save as `05_Azure_AppService_Overview.png`

---

## Phase 3 — Build the Word document (`ST10538419_CLDV7111_Part3.docx`)

POE rules: typed MS Word, each major section starts on a **new page**, include student number, module code, GitHub link, web app URL, all answers, screenshots, and references.

### Cover / first page

- [ ] Student Number: **ST10538419**
- [ ] Module Code: **CLDV7111**
- [ ] Assessment: **Portfolio of Evidence — Part 3**
- [ ] GitHub Repository URL: `https://github.com/MrSolution07/Poe-CloudDevelopment`
- [ ] Deployed Web Application URL: `https://st10538419-eventease-ebbpdwa4dsbpg6cs.switzerlandnorth-01.azurewebsites.net/`

---

### Section A — Advanced Filtering *(new page)*

Copy from `ST10538419_CLDV7111_Part3.md` Section A:

- [ ] **A.1** EventType lookup table (table of 8 categories + SQL evidence)
- [ ] **A.2** Filter by Event Type (controller + view code snippets + explanation)
- [ ] **A.3** Filter by Date Range (controller + view code snippets + explanation)
- [ ] **A.4** Filter by Venue Availability (model + controller + view snippets + explanation)

Section A is mostly text/code — no mandatory screenshots in the rubric, but your written evidence is complete.

---

### Section B — Azure Deployment Updates *(new page)*

Copy from `ST10538419_CLDV7111_Part3.md` Section B:

- [ ] **B.1** Deployed web application URL and route verification table
- [ ] **B.2** Database updates table (EventTypes, EventTypeId, IsAvailable, vw_BookingDetail)
- [ ] **B.2** SQL verification queries (the three SELECT statements)
- [ ] **Embed Screenshot 1** — `SELECT * FROM EventTypes` (8 rows) — directly under B.2
- [ ] **Embed Screenshot 2** — `SELECT VenueId, VenueName, IsAvailable FROM Venues`
- [ ] **Embed Screenshot 3** — Live Booking Overview with Advanced Filters expanded
- [ ] *(Optional)* Embed Screenshot 4 — `vw_BookingDetail` query results
- [ ] *(Optional)* Embed Screenshot 5 — Azure App Service Overview (Running)

---

### Section C — Reflective Technical Report *(new page)*

Copy from `ST10538419_CLDV7111_Part3.md` Section C:

- [ ] **C.1** System Feature List (all 20 features with “How It Works” column)
- [ ] **C.2** Component Discussion:
  - [ ] Azure App Service (+ why + alternative)
  - [ ] Azure SQL Database (+ why + alternative)
  - [ ] Azure Blob Storage (+ why + alternative)
  - [ ] **Theoretical:** Cosmos DB vs traditional databases
  - [ ] **Theoretical:** Logic Apps — sensitive data considerations
  - [ ] **Theoretical:** Event Grid combined with other services
  - [ ] ASP.NET Core MVC (+ alternative)
  - [ ] Entity Framework Core (+ alternative)
  - [ ] SixLabors.ImageSharp
- [ ] **C.3** Project Reflection:
  - [ ] Development journey (Parts 1–3)
  - [ ] Challenges faced
  - [ ] Lessons learned
  - [ ] Cloud development understanding
  - [ ] What you would do differently

---

### Reference List *(new page)*

- [ ] Include the full Reference List from the end of `ST10538419_CLDV7111_Part3.md`
- [ ] **Recommended:** Reformat citations to **IEEE** style (ICT module requirement) to avoid referencing penalty:
  - In-text: `[1]`, `[2]`, etc.
  - Reference list numbered `[1]` … `[9]`
  - Example entry:  
    `[1] Microsoft, "Azure App Service documentation," Microsoft Docs, 2026. [Online]. Available: https://learn.microsoft.com/en-us/azure/app-service/. [Accessed: 11-Jun-2026].`

---

### Assessment sheet (if your lecturer requires it)

- [ ] Print or copy **Rubric 3 (POE Part 3)** from the POE PDF (pages 18–19)
- [ ] Fill in: Module name, module code, your name, student number
- [ ] Attach as the last page of the Word doc **or** submit separately if Arc instructions say so

---

## Phase 4 — Word formatting checklist

- [ ] File name is exactly: **`ST10538419_CLDV7111_Part3.docx`**
- [ ] Each major section (A, B, C, References) starts on a **new page**
- [ ] All screenshots are **embedded** (not just file links)
- [ ] Code blocks are readable (monospace font, e.g. Consolas 9–10 pt)
- [ ] Tables render correctly (EventTypes table, feature list, route verification)
- [ ] Spell-check the document
- [ ] Save and close the file before uploading

---

## Phase 5 — GitHub (separate from Word, but required by POE)

- [ ] Push latest code to: https://github.com/MrSolution07/Poe-CloudDevelopment
- [ ] Confirm Part 3 filter code is on `main` (`EventTypes`, `IsAvailable`, `BookingsController.Overview` filters)
- [ ] *(Optional but good practice)* Add `ST10538419_CLDV7111_Part3.md` or the final `.docx` to the repo
- [ ] Do **not** commit `.env`, real connection strings, or passwords

---

## Phase 6 — Final checks before LMS upload

- [ ] Open the live app URL — home page loads
- [ ] Log in and test all three filters on Booking Overview:
  - [ ] Event Type filter
  - [ ] Date range filter
  - [ ] Venue Availability filter
- [ ] GitHub URL in the Word doc matches the real repo (not the placeholder)
- [ ] Web app URL in the Word doc opens the deployed site
- [ ] All 3 required screenshots are inside Section B
- [ ] Reference list is present
- [ ] Document is `.docx` (not `.md` or `.pdf` unless Arc specifically allows PDF)

---

## Phase 7 — Submit on Arc/LMS

- [ ] Log in to Arc / student portal
- [ ] Open module **CLDV7111**
- [ ] Find the **Part 3** submission link
- [ ] Upload **`ST10538419_CLDV7111_Part3.docx`**
- [ ] Upload assessment sheet separately if required
- [ ] Confirm upload succeeded (green tick / confirmation message)
- [ ] Keep a backup copy of the Word doc and screenshots

---

## Quick rubric map — “did I include everything for 100%?”

| Rubric area | What must be in the Word doc | Done? |
|---|---|---|
| A. Advanced Filtering (16–20) | Section A text + code evidence; filters work in live app | [ ] |
| B. Azure Deployment (16–20) | Web app URL + **3 screenshots** (EventTypes, IsAvailable, live filters) | [ ] |
| C.1 Feature list (16–20) | Section C.1 — 20 features with explanations | [ ] |
| C.2 Component discussion (16–20) | Section C.2 — Azure services, tech, alternatives, **3 theory topics** | [ ] |
| C.3 Reflection (16–20) | Section C.3 — journey, challenges, lessons, cloud understanding | [ ] |
| Referencing | IEEE-style reference list (recommended) | [ ] |

---

## Troubleshooting

| Problem | What to do |
|---|---|
| Query Editor login fails | Reset SQL server password in Azure Portal; use SQL auth not Azure AD |
| `EventTypes` table empty | Restart web app or run seed SQL from `Database/Schema.sql` |
| `vw_BookingDetail` invalid object | Run view script from `Database/Schema.sql` in Query Editor |
| Admin login fails | Try `admin@eventease.co.za` / `Admin123`; check App Service Configuration |
| Booking Overview is empty | Create sample venue, event, and booking while logged in as admin |
| Filters do not appear | Hard-refresh browser (`Ctrl+F5`); confirm latest code is deployed |

---

*Generated for ST10538419 — CLDV7111 Part 3. Source content: `ST10538419_CLDV7111_Part3.md`.*
