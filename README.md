# Barangay Centralized Information System

A Windows Forms desktop application built with VB.NET for managing barangay records including residents, activities, ordinances, scholarships, and user accounts.

---

## Prerequisites

Before setting up, make sure the following are installed on your machine:

| Requirement | Download |
|---|---|
| .NET 10 SDK | https://dotnet.microsoft.com/download/dotnet/10.0 |
| Visual Studio 2022 | https://visualstudio.microsoft.com/vs/ |
| MySQL Server 8.x | https://dev.mysql.com/downloads/mysql/ |

> When installing Visual Studio 2022, select the **.NET desktop development** workload.

---

## Setup Guide

### 1. Clone the Repository

```bash
git clone https://github.com/Jymrkkk/barangayinformationsystem.git
cd barangayinformationsystem
```

### 2. Set Up the Database

1. Open **MySQL Workbench** or any MySQL client
2. Create the database and tables by running the schema file:
   ```
   BarangaySystem/Database/barangay_schema.sql
   ```
3. (Optional) Seed sample resident data:
   ```
   BarangaySystem/Database/seed_residents.sql
   ```

### 3. Configure the Database Connection

Open `BarangaySystem/DataAccess/DatabaseConfig.vb` and update the credentials to match your MySQL setup:

```vb
Private Const Server   As String = "localhost"
Private Const Port     As String = "3306"
Private Const Database As String = "barangay_db"
Private Const UserId   As String = "root"
Private Const Password As String = ""   ' ← set your MySQL password here
```

### 4. Open in Visual Studio 2022

1. Open Visual Studio 2022
2. Click **Open a project or solution**
3. Navigate to the cloned folder and select:
   ```
   BarangaySystem/BarangaySystem.vbproj
   ```
4. Wait for NuGet packages to restore (shown in the bottom status bar)

### 5. Build and Run

Press **F5** or click the green **Run** button to build and launch the application.

---

## Default Login Credentials

| Field | Value |
|---|---|
| Username | `admin` |
| Password | `Admin@1234` |

> It is recommended to change the default password after first login.

---

## NuGet Packages Used

| Package | Purpose |
|---|---|
| MySql.Data 8.3.0 | MySQL database connectivity |
| BCrypt.Net-Next 4.0.3 | Password hashing |
| itext7 8.0.3 | PDF report generation |
| EPPlus 7.2.1 | Excel export |

These are restored automatically when you open the project in Visual Studio.

---

## Project Structure

```
BarangaySystem/
├── App/                  # Application entry point
├── BusinessLogic/        # Service layer
├── DataAccess/           # Repository layer + DB config
├── Database/             # SQL schema and seed files
├── Forms/
│   ├── Dialogs/          # Add/Edit dialog forms
│   └── Modules/          # Main panel modules
├── Helpers/              # UI, validation, print helpers
├── Models/               # Data models
├── Resources/            # Embedded assets (logo)
└── Tools/                # Utility tools (hash generator)
```

---

## Notes

- The UI is built entirely in code — there are no `.Designer.vb` files, so the Visual Studio WinForms designer will not show a visual layout. Run the app with **F5** to see the interface.
- Make sure MySQL is running before launching the application.
