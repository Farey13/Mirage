# 🏛️ Project Mirage: The Final & Complete Blueprint
**Version:** 2.1 (Detailed Fields)
**Last Updated:** September 19, 2025

## 1. Project Vision & Architecture

### Vision
To develop a modern, intuitive, and highly modular desktop application for the H.A Atoll Hospital laboratory. The system will digitize all manual logbooks, ensure complete data integrity through robust auditing, and provide seamless offline capability for uninterrupted workflow.

### Confirmed Architecture: 3-Tier Distributed Model
The project will be built using a modern, secure, and scalable 3-tier architecture using .NET 8.

```
+----------------+      <-- HTTP/HTTPS Requests -->      +---------------------------------+
|                |      (e.g., GET, POST, PUT)           |                                 |
|   WPF Client   |  <-------------------------------->   |   ASP.NET Core API Server       |
| (User Interface)      |                                |   (Business Logic & Data Access)|
|                |                                       |                                 |
+----------------+                                       +---------------------------------+

```

## 2. Technical Stack & Project Structure

### Confirmed Technical Stack
| Layer | Technology / Pattern |
| :--- | :--- |
| **Platform** | .NET 8 |
| **Desktop Client** | **WPF (Windows Presentation Foundation)** with **MVVM** |
| **API Backend** | **ASP.NET Core Web API** |
| **Data Access** | **Dapper** (Lightweight Object-Relational Mapper) |
| **Primary Database**| **SQL Server** (Express Edition is sufficient) |
| **Offline Cache** | SQLite (`.sqlite` on the client) |

## 2.1 Connection String
`Server=localhost;Database=MirageDB;Trusted_Connection=True;`

### Visual Studio Solution Structure
* **`PortalMirage.UI` (WPF Project):** The desktop application.
* **`PortalMirage.Api` (ASP.NET Core Project):** The new backend server.
* **`PortalMirage.Business` (Class Library):** The Business Logic Layer (BLL), referenced by the API.
* **`PortalMirage.Data` (Class Library):** The Data Access Layer (DAL), referenced by the BLL.
* **`PortalMirage.Core` (Class Library):** Shared models and interfaces used by all projects.

---

## 3. Core Systems & Features

* **Database & Storage**
    * **Primary Database (SQL Server):** The central `PortalMirageDB` running on your SQL Server instance.
    * **External Patient Database (Read-Only):** A secure, read-only connection to the main hospital patient database to auto-fill patient details.
    * **Offline Cache (SQLite):** A local database on each client PC to enable full offline functionality.

* **Security**
    * **API-Centric Security:** The SQL Server is completely isolated from clients. All data access is forced through the secure ASP.NET Core API.
    * **Authentication:** Users will log in via the API, receiving a secure token (e.g., JWT) to authenticate subsequent requests.
    * **Customizable Roles & Permissions:** Admins can create custom roles and assign granular permissions for each module.

* **Key System-Wide Features**
    * **Granular Audit Trail:** An `AuditLog` table records every data change, including old and new values.
    * **Digital Signatures & Report Locking:** Supervisors can digitally sign and lock reports, making them read-only and adding a "Verified by [User] on [Date]" footer.

---

## 4. Detailed Module Blueprints with Fields

#### Module 1: Repeat Sample Book
* **Purpose:** To log and track all instances where a patient sample needs to be repeated.
* **Database Schema (`RepeatSampleLog`):**
| Field Name | Data Type | Notes |
| :--- | :--- | :--- |
| `RepeatID` | `INT` | Primary Key, Auto-increment |
| `PatientIdCardNumber`| `NVARCHAR(100)` | Indexed |
| `PatientName` | `NVARCHAR(255)` | Not Null |
| `InformedPersonOrDept`| `NVARCHAR(255)` | |
| `ReasonText` | `NVARCHAR(MAX)` | |
| `LogDateTime` | `DATETIME2` | Not Null, Default Current Time |
| `LoggedByUserID` | `INT` | Not Null, Foreign Key to `Users` table |
* **Key Features:** The user enters the `PatientIdCardNumber`, and the system queries the external hospital DB to auto-populate the `PatientName`.

#### Module 2: Kit Validation Book
* **Purpose:** To record validation results for new reagent kits and lots.
* **Database Schema (`KitValidations`):**
| Field Name | Data Type | Notes |
| :--- | :--- | :--- |
| `ValidationID` | `INT` | Primary Key, Auto-increment |
| `KitName` | `NVARCHAR(255)` | Not Null |
| `KitLotNumber` | `NVARCHAR(100)` | Not Null, Indexed |
| `KitExpiryDate` | `DATE` | Not Null |
| `ValidationStatus` | `NVARCHAR(50)` | Not Null ("Accepted", "Rejected") |
| `Comments` | `NVARCHAR(MAX)` | |
| `ValidationDateTime` | `DATETIME2` | Not Null, Default Current Time |
| `ValidatedByUserID` | `INT` | Not Null, Foreign Key to `Users` table |
* **Key Features:** The dashboard can have a widget to show alerts for kits nearing their `KitExpiryDate`.

#### Module 3: Calibration Log
* **Purpose:** To log routine QC and calibration events.
* **Database Schema (`CalibrationLogs`):**
| Field Name | Data Type | Notes |
| :--- | :--- | :--- |
| `CalibrationID` | `INT` | Primary Key, Auto-increment |
| `TestName` | `NVARCHAR(255)` | Not Null, Indexed |
| `QcResult` | `NVARCHAR(50)` | Not Null ("Passed", "Failed") |
| `Reason` | `NVARCHAR(MAX)` | |
| `CalibrationDateTime`| `DATETIME2` | Not Null, Default Current Time |
| `PerformedByUserID`| `INT` | Not Null, Foreign Key to `Users` table |
* **Key Features:** The reporting engine will allow generating reports filtered by `TestName`.

#### Module 4: Sample Storage Book
* **Purpose:** To track sample storage and test completion status.
* **Database Schema (`SampleStorage`):**
| Field Name | Data Type | Notes |
| :--- | :--- | :--- |
| `StorageID` | `INT` | Primary Key, Auto-increment |
| `PatientSampleID` | `NVARCHAR(100)` | Not Null, Indexed |
| `StorageDateTime` | `DATETIME2` | Not Null, Default Current Time |
| `StoredByUserID` | `INT` | Not Null, Foreign Key to `Users` table |
| `IsTestDone` | `BIT`| Not Null, Default `0` (False) |
| `TestDoneDateTime` | `DATETIME2` | Nullable |
| `TestDoneByUserID` | `INT` | Nullable, Foreign Key to `Users` table |
* **Key Features:** A "Pending Tests" view will allow users to search for incomplete items and mark them as done from the results grid.

#### Module 5: Handover Book
* **Purpose:** To create a formal, auditable log of shift handovers.
* **Database Schema (`Handovers`):**
| Field Name | Data Type | Notes |
| :--- | :--- | :--- |
| `HandoverID` | `INT` | Primary Key, Auto-increment |
| `HandoverNotes` | `NVARCHAR(MAX)` | Not Null |
| `GivenDateTime` | `DATETIME2` | Not Null, Default Current Time |
| `GivenByUserID` | `INT` | Not Null, Foreign Key to `Users` table |
| `IsReceived` | `BIT` | Not Null, Default `0` (False) |
| `ReceivedDateTime` | `DATETIME2` | Nullable |
| `ReceivedByUserID` | `INT` | Nullable, Foreign Key to `Users` table |
* **Key Features:** The dashboard will prominently display an alert for pending handovers.

#### Module 6: Machine Breakdown Log
* **Purpose:** To log equipment malfunctions and track resolutions.
* **Database Schema (`MachineBreakdowns`):**
| Field Name | Data Type | Notes |
| :--- | :--- | :--- |
| `BreakdownID` | `INT` | Primary Key, Auto-increment |
| `MachineName` | `NVARCHAR(255)` | Not Null, Indexed |
| `BreakdownReason`| `NVARCHAR(MAX)` | Not Null |
| `ReportedDateTime`| `DATETIME2` | Not Null, Default Current Time |
| `ReportedByUserID`| `INT` | Not Null, Foreign Key to `Users` table |
| `IsResolved` | `BIT` | Not Null, Default `0` (False) |
| `ResolvedDateTime`| `DATETIME2` | Nullable |
| `ResolvedByUserID` | `INT` | Nullable, Foreign Key to `Users` table |
* **Key Features:** A dashboard widget will list all currently unresolved issues.

#### Module 7: Media Sterility Book
* **Purpose:** To log the sterility testing of prepared culture media.
* **Database Schema (`MediaSterilityChecks`):**
| Field Name | Data Type | Notes |
| :--- | :--- | :--- |
| `SterilityCheckID`| `INT` | Primary Key, Auto-increment |
| `MediaName` | `NVARCHAR(255)` | Not Null |
| `MediaLotNumber` | `NVARCHAR(100)` | Not Null, Indexed |
| `MediaQuantity` | `NVARCHAR(100)` | |
| `Result37C` | `NVARCHAR(50)` | Not Null ("No Growth", "Growth Seen") |
| `Result25C` | `NVARCHAR(50)` | Not Null ("No Growth", "Growth Seen") |
| `OverallStatus` | `NVARCHAR(50)` | Not Null ("Passed", "Failed") |
| `Comments` | `NVARCHAR(MAX)` | |
| `CheckDateTime` | `DATETIME2` | Not Null, Default Current Time |
| `PerformedByUserID`| `INT` | Not Null, Foreign Key to `Users` table |
* **Key Features:** The `OverallStatus` will be automatically calculated by the BLL in the API.

#### Module 8: Daily Task Log
* **Purpose:** A dynamic checklist to manage and track routine daily tasks.
* **Database Schemas:**
    * **`Tasks` (Definitions):** `TaskID` (PK, INT), `TaskName` (NVARCHAR), `TaskCategory` ("Morning"/"Evening"), `IsActive` (BIT).
    * **`DailyTaskLogs` (Records):** `LogID` (PK, INT), `TaskID` (FK), `LogDate` (DATE), `Status` ("Pending"/"Completed"/"Not Available"), `CompletedByUserID` (FK), `CompletedDateTime` (DATETIME2).
* **Key Features:** The UI will feature progress bars for each shift, with the completion percentage calculated by the API.
