# Barangay Centralized Information System
## System Manual — v1.0

**Prepared for:** Barangay Local Government Unit  
**Platform:** Windows Desktop Application (.NET 10, Windows Forms)  
**Database:** MySQL 8.x  

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [System Requirements](#2-system-requirements)
3. [Installation & Setup](#3-installation--setup)
4. [User Roles & Permissions](#4-user-roles--permissions)
5. [Logging In](#5-logging-in)
6. [Main Interface](#6-main-interface)
7. [Modules](#7-modules)
   - 7.1 [Dashboard](#71-dashboard)
   - 7.2 [Residents Information](#72-residents-information)
   - 7.3 [Activities](#73-activities)
   - 7.4 [Ordinances & Resolutions](#74-ordinances--resolutions)
   - 7.5 [Student Records](#75-student-records)
   - 7.6 [Reports & Analytics](#76-reports--analytics)
   - 7.7 [Event Logs](#77-event-logs)
   - 7.8 [Account Management](#78-account-management)
8. [Toolbar Actions](#8-toolbar-actions)
9. [Upcoming Event Alerts](#9-upcoming-event-alerts)
10. [Printing Records](#10-printing-records)
11. [Exporting Records](#11-exporting-records)
12. [Database Migrations](#12-database-migrations)
13. [Default Credentials](#13-default-credentials)
14. [Troubleshooting](#14-troubleshooting)

---

## 1. System Overview

The **Barangay Centralized Information System (BCIS)** is a desktop application designed to digitize and centralize the records management of a barangay (local government unit) in the Philippines. It replaces manual paper-based processes with a structured, role-controlled digital system.

**Core capabilities:**

- Resident records management with birth certificate storage
- Barangay activity scheduling and tracking
- Ordinance and resolution registry
- Student and scholarship records
- Certificate issuance (Barangay Clearance, Residency, Indigency, etc.)
- Automated PDF and Excel report generation
- Audit trail via event logs
- Upcoming event popup notifications
- Role-based access control (Admin, Encoder, Viewer)

---

## 2. System Requirements

| Component | Minimum |
|-----------|---------|
| Operating System | Windows 10 / Windows 11 (64-bit) |
| .NET Runtime | .NET 10.0 (Windows) |
| Database | MySQL 8.0 or higher |
| RAM | 4 GB |
| Storage | 500 MB free disk space |
| Display | 1024 × 600 minimum resolution |
| PDF Viewer | Adobe Acrobat Reader or equivalent |
| Excel Viewer | Microsoft Excel, LibreOffice Calc, or equivalent |

---

## 3. Installation & Setup

### 3.1 Database Setup

1. Install **MySQL 8.x** on your server or local machine.
2. Open MySQL Workbench, HeidiSQL, or the MySQL command line.
3. Run the main schema file to create the database and all tables:

```sql
SOURCE /path/to/BarangaySystem/Database/barangay_schema.sql;
```

4. If upgrading from an earlier version, run any pending migration files in order:

```sql
SOURCE /path/to/BarangaySystem/Database/migrate_add_solo_parent.sql;
SOURCE /path/to/BarangaySystem/Database/migrate_add_birth_certificate.sql;
```

### 3.2 Application Configuration

1. Open `BarangaySystem/DataAccess/DatabaseConfig.vb`.
2. Update the connection string with your MySQL server details:

```vb
Server=localhost
Database=barangay_db
User=your_mysql_user
Password=your_mysql_password
```

### 3.3 Running the Application

- **Development:** Open `BarangaySystem.sln` in Visual Studio and press **F5**.
- **Production:** Run the compiled `BarangaySystem.exe` from the `bin/Release` folder.

---

## 4. User Roles & Permissions

The system has three roles. Access is enforced at both the UI and data layers.

| Action | Admin | Encoder | Viewer |
|--------|-------|---------|--------|
| View all records | ✅ | ✅ | ✅ |
| Add records | ✅ | ✅ | ❌ |
| Update records | ✅ | ✅ | ❌ |
| Delete records | ✅ | ❌ | ❌ |
| Print records | ✅ | ✅ | ✅ |
| Export records | ✅ | ✅ | ✅ |
| Manage user accounts | ✅ | ❌ | ❌ |
| View event logs | ✅ | ✅ | ✅ |

> **Note:** The Account Management module is only visible to Admin users.

---

## 5. Logging In

1. Launch the application. The **Login Form** appears.
2. Enter your **Username** and **Password**.
3. Check **Show password** to reveal the password field if needed.
4. Click **LOGIN** or press **Enter**.

On successful login, the **Main Form** opens and the Dashboard is displayed.

**Default credentials** (change after first login):

| Username | Password | Role |
|----------|----------|------|
| admin | Admin@1234 | Admin |
| encoder1 | Encoder@1234 | Encoder |
| encoder2 | Encoder@1234 | Encoder |
| viewer1 | Viewer@1234 | Viewer |

---

## 6. Main Interface

The main window is divided into four areas:

```
┌─────────────────────────────────────────────────────────┐
│  HEADER BAR  (module title, subtitle, date/time)        │
├─────────────────────────────────────────────────────────┤
│  TOOLBAR  (+ Add | ✎ Update | ✕ Delete | ⎙ Print |     │
│            ↓ Export | [Search box] | 🔍 Search)         │
├──────────┬──────────────────────────────────────────────┤
│          │                                              │
│ SIDEBAR  │           CONTENT AREA                      │
│ (nav)    │           (active module panel)             │
│          │                                              │
├──────────┴──────────────────────────────────────────────┤
│  STATUS BAR  (● Online | DB | User | Module | Clock)    │
└─────────────────────────────────────────────────────────┘
```

### Sidebar Navigation

Click any item in the left sidebar to switch modules:

- 🏠 Home (Dashboard)
- 👥 Residents Info
- 📅 Activities
- 🛡 Ordinances
- 🎓 Student Records
- 📊 Reports
- 🕐 Event Logs
- 👤 Account Mgmt *(Admin only)*
- 🚪 Logout *(bottom of sidebar)*

---

## 7. Modules

### 7.1 Dashboard

The Dashboard provides a real-time summary of the barangay's key statistics.

**Stat cards displayed:**
- Total Residents
- Active Residents
- Upcoming Activities
- Total Ordinances
- Total Students
- Active Voters

**Charts and breakdowns:**
- Residents by Purok (progress bars)
- Activity status distribution

The Dashboard is read-only. No Add/Update/Delete actions are available here.

---

### 7.2 Residents Information

Manages the complete resident registry of the barangay.

#### Tabs

| Tab | Description |
|-----|-------------|
| Resident List | Full grid of all residents with filters |
| Demographics | Visual charts: gender, age groups, marital status, special categories |
| Certificates | Issued barangay certificates per resident |

#### Filtering Residents

Use the **Purok** and **Status** dropdowns at the top, then click **Filter**. You can also type in the search box in the toolbar for real-time name/code search.

#### Adding a Resident

1. Click **+ Add** in the toolbar.
2. Fill in all required fields (marked with `*`):
   - Last Name, First Name, Birth Date, Gender
   - Civil Status, Address, Purok, Status
3. Optional fields: Middle Name, Contact No., Email, Occupation
4. Check **Registered Voter**, **Solo Parent** as applicable.
5. The **Senior Citizen (60+)** checkbox is auto-detected from the birth date — it cannot be manually changed.
6. **Birth Certificate section** (bottom of form):
   - Click **📂 Upload Image** to attach a scanned birth certificate (JPG, PNG, BMP).
   - Files larger than 5 MB will show a warning before uploading.
   - Click **✕ Remove** to clear the uploaded image.
7. Click **Save**.

#### Viewing a Resident (double-click)

Double-clicking a row opens the resident in **View Only** mode. All fields are read-only. If a birth certificate is on file, click **🔍 View Full Size** to open it in a full-screen viewer.

#### Editing a Resident

1. Select a row in the grid.
2. Click **✎ Update** in the toolbar.
3. Modify the fields as needed.
4. Click **Save**.

#### Deleting a Resident

1. Select a row.
2. Click **✕ Delete**.
3. Confirm the deletion in the dialog. This action is permanent.

#### Issuing a Certificate

1. Switch to the **Certificates** tab.
2. Click **+ Issue Certificate**.
3. Select the resident, certificate type, purpose, issued by, date, OR number, and amount.
4. Click **Save**.

---

### 7.3 Activities

Manages barangay activities and events.

#### Grid Columns

| Column | Description |
|--------|-------------|
| Act. ID | Auto-generated activity code |
| Activity Name | Name of the event |
| Date | Scheduled date |
| Venue | Location |
| Organizer | Person/office responsible |
| Participants | Expected headcount |
| Status | Upcoming / Ongoing / Completed / Cancelled |

#### Adding an Activity

1. Click **+ Add**.
2. Fill in Activity Name, Date, Venue, Organizer, Participants, and Status.
3. Click **Save**.

#### Upcoming Event Alerts

If an activity has **Status = Upcoming** and its date is **tomorrow**, a popup notification will automatically appear 3 seconds after login. See [Section 9](#9-upcoming-event-alerts) for details.

---

### 7.4 Ordinances & Resolutions

Manages the barangay's legislative records.

#### Tabs

| Tab | Description |
|-----|-------------|
| Ordinance List | All barangay ordinances (BO numbers) |
| Resolutions | All barangay resolutions |

#### Ordinance Fields

| Field | Description |
|-------|-------------|
| BO Number | Must follow format `BO-YYYY-NNN` (e.g. `BO-2025-001`) |
| Introduced By | Kagawad who introduced the ordinance |
| Description | Brief summary |
| Date Enacted | Date the ordinance was passed |
| Approved By | Approving official |
| Status | Active / Inactive / Repealed |

#### Resolution Fields

| Field | Description |
|-------|-------------|
| Resolution No. | Unique resolution number |
| Subject | Subject matter |
| Sponsor | Sponsoring kagawad |
| Date Passed | Date the resolution was passed |
| Status | Approved / Pending / Rejected |

---

### 7.5 Student Records

Manages student information and scholarship records for barangay residents.

#### Tabs

| Tab | Description |
|-----|-------------|
| Student List | Full student registry with import/export |
| By School | Enrollment count per school (bar chart) |
| Scholarships | Scholarship grants per student |

#### Adding a Student

1. Click **+ Add**.
2. Fill in Last Name, First Name, Address, Purok, Grade/Year, School Year.
3. Select School from the dropdown (populated from the `schools` table).
4. Set Status (Enrolled / Dropped / Graduated) and check **Scholarship Recipient** if applicable.
5. Click **Save**.

#### Importing Students from Excel

1. Click **📥 Import Excel** at the top of the Student List tab.
2. Select an `.xlsx` file. The expected column order is:

| Col | Field | Required |
|-----|-------|----------|
| A | Last Name | ✅ |
| B | First Name | ✅ |
| C | Middle Name | |
| D | Birth Date (MM/dd/yyyy) | |
| E | Gender (Male/Female/Other) | |
| F | Address | ✅ |
| G | Purok | ✅ |
| H | School Name (must match exactly) | |
| I | Grade/Year | ✅ |
| J | School Year (e.g. 2024-2025) | ✅ |
| K | Scholar (Yes/No) | |
| L | Status (Enrolled/Dropped/Graduated) | |

3. Row 1 must be the header row. Data starts from row 2.
4. After import, a summary shows how many records were added and how many rows were skipped (with reasons).

#### Downloading the Import Template

Click **📄 Download Template** to generate and open a pre-formatted Excel file with the correct headers and a sample row.

#### Scholarships

1. Switch to the **Scholarships** tab.
2. Click **+ Add Scholarship**.
3. Select the student, enter grant type, amount, school year, and status.
4. Click **Save**.

---

### 7.6 Reports & Analytics

Provides automated report generation for all modules.

Available reports (accessible via the Export button):

| Report | Format |
|--------|--------|
| Residents Report | PDF or Excel |
| Students Report | Excel |
| Ordinances Report | PDF |
| Activities Report | Excel |

All exported files are automatically opened after a successful export using the system's default application (e.g., Adobe Reader for PDF, Excel for .xlsx).

---

### 7.7 Event Logs

A read-only audit trail of all system actions.

**Logged events include:**
- LOGIN / LOGOUT
- INSERT (record added)
- UPDATE (record modified)
- DELETE (record removed)
- EXPORT (file exported)
- PRINT (record printed)

Each log entry shows: Log Code, User, Event Type, Module, Description, IP Address, and Timestamp.

No Add/Update/Delete actions are available in this module.

---

### 7.8 Account Management

*(Admin only)*

Manages system user accounts.

#### Adding a User

1. Click **+ Add**.
2. Enter Full Name, Username, Email, Role, and Password.
3. Password requirements:
   - Minimum 8 characters
   - At least one uppercase letter
   - At least one digit
4. Click **Save**.

#### Editing a User

1. Select a user row.
2. Click **✎ Update**.
3. Modify fields as needed. Leave the password field blank to keep the existing password.
4. Click **Save**.

#### Deactivating a User

Set the user's **Status** to **Inactive** via the Edit dialog. Inactive users cannot log in.

---

## 8. Toolbar Actions

The toolbar at the top of the content area provides context-sensitive actions depending on the active module.

| Button | Action | Availability |
|--------|--------|-------------|
| **+ Add** | Open the Add dialog for the current module | Admin, Encoder |
| **✎ Update** | Open the Edit dialog for the selected record | Admin, Encoder |
| **✕ Delete** | Delete the selected record (with confirmation) | Admin only |
| **⎙ Print** | Print the current grid to the default printer | All roles |
| **↓ Export** | Export the current module's data to PDF or Excel | All roles |
| **🔍 Search** | Filter the grid by the text in the search box | All roles |

> Toolbar buttons are automatically disabled when the current module or role does not support that action (e.g., Delete is hidden for Encoders; Add/Update/Delete are hidden on the Dashboard and Event Logs).

---

## 9. Upcoming Event Alerts

The system automatically checks for activities scheduled for **tomorrow** and notifies the logged-in user.

**Trigger conditions:**
- Activity `status = 'Upcoming'`
- Activity `activity_date = tomorrow's date`

**When it fires:**
- **3 seconds** after successful login (first check)
- Every **1 hour** thereafter while the application is open

**The popup notification:**
- Appears in the **bottom-right corner** of the screen
- Shows a card for each upcoming event with: name, date, venue, and organizer
- Is **draggable** — you can move it anywhere on screen
- Has a **📅 View Activities** button that navigates directly to the Activities module
- Has a **Dismiss** button and an **✕** close button
- Fades in smoothly on appearance

If no events are scheduled for tomorrow, no popup appears.

---

## 10. Printing Records

1. Navigate to the module you want to print (Residents, Activities, Ordinances, Students, etc.).
2. Apply any filters or search terms to narrow the records if needed.
3. Click **⎙ Print** in the toolbar.
4. The standard Windows **Print Dialog** appears.
5. Select your printer and click **Print**.

The printout includes:
- Report title and generation timestamp
- Column headers (maroon background)
- Alternating row colors for readability
- Page number and total row count in the footer

---

## 11. Exporting Records

1. Navigate to the module you want to export.
2. Click **↓ Export** in the toolbar.
3. A dialog asks: **YES = Excel (.xlsx)** / **NO = PDF (.pdf)** / **Cancel**.
4. Choose the format and select a save location in the file dialog.
5. The file is saved and **automatically opened** in your default application.

**Export formats by module:**

| Module | PDF | Excel |
|--------|-----|-------|
| Residents | ✅ | ✅ |
| Activities | ❌ | ✅ |
| Ordinances | ✅ | ❌ |
| Students | ❌ | ✅ |

---

## 12. Database Migrations

When updating the system, run any new migration files against the database before launching the updated application. Migration files are located in:

```
BarangaySystem/Database/
```

| File | Purpose |
|------|---------|
| `barangay_schema.sql` | Full initial schema — run once on fresh install |
| `seed_residents.sql` | Sample resident data for testing |
| `migrate_add_solo_parent.sql` | Adds `is_solo_parent` column to residents |
| `migrate_add_birth_certificate.sql` | Adds `birth_certificate` LONGBLOB column to residents |

**Always run migrations in the order listed above.**

---

## 13. Default Credentials

| Username | Password | Role | Notes |
|----------|----------|------|-------|
| admin | Admin@1234 | Admin | Full access — change immediately |
| encoder1 | Encoder@1234 | Encoder | Data entry access |
| encoder2 | Encoder@1234 | Encoder | Data entry access |
| viewer1 | Viewer@1234 | Viewer | Read-only access (inactive by default) |

> **Security notice:** Change all default passwords immediately after the first login, especially the `admin` account.

---

## 14. Troubleshooting

### "Connection error. Check DB settings."

- Verify MySQL is running.
- Check the connection string in `DatabaseConfig.vb` (server, database name, username, password).
- Ensure the MySQL user has SELECT, INSERT, UPDATE, DELETE privileges on `barangay_db`.

### "Export error: Unknown PdfException"

- Make sure the destination folder is writable.
- Close any previously opened PDF of the same filename before re-exporting.
- Ensure the `itext7` NuGet package is properly restored (`dotnet restore`).

### "We can't open this file" (Excel)

- Ensure Microsoft Excel or LibreOffice Calc is installed.
- Try opening the file manually from the saved location.
- Re-export and make sure no other process has the file open.

### Birth certificate image won't preview

- Only JPG, JPEG, PNG, and BMP formats are supported.
- Files must not be corrupted. Try opening the image in Windows Photos first.
- Files larger than 5 MB will show a warning — consider compressing the image before uploading.

### Upcoming event alert not appearing

- Verify the activity's `status` is set to `'Upcoming'` (not Ongoing or Completed).
- Verify the `activity_date` is exactly tomorrow's date in the database.
- Check that the MySQL connection is active — the alert silently skips if the DB is unreachable.

### Print dialog does not open

- Ensure at least one printer (including a PDF printer like Microsoft Print to PDF) is installed on Windows.
- Check that the DataGridView has at least one visible row before printing.

### Application won't start

- Ensure **.NET 10.0 Windows Runtime** is installed. Download from: https://dotnet.microsoft.com/download
- Run `dotnet --version` in a terminal to confirm the runtime is available.

---

*End of System Manual — Barangay Centralized Information System v1.0*  
*© 2026 Barangay System*
