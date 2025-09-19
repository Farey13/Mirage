# Mirage Gemini Context

## Project Overview

This project, codenamed "Mirage," is a desktop application designed for the H.A Atoll Hospital laboratory. Its primary purpose is to digitize manual logbooks, ensuring data integrity and providing offline capabilities for an uninterrupted workflow.

The application is based on a 3-tier distributed architecture:

*   **Client:** A WPF application built with .NET 8, using the MVVM pattern.
*   **Backend:** An ASP.NET Core Web API server responsible for business logic and data access.
*   **Database:** SQL Server for the primary database and SQLite for offline caching on the client.

**Note:** The project blueprint describes a multi-project solution structure (`PortalMirage.UI`, `PortalMirage.Api`, etc.), but the current implementation only contains the `Mirage.UI` project. The backend and other class libraries have not been created yet.

## Building and Running

As a standard .NET project, you can use either Visual Studio or the `dotnet` CLI to build and run the application.

### Using Visual Studio:

1.  Open the `Mirage.sln` file in Visual Studio.
2.  Set the `Mirage.UI` project as the startup project.
3.  Press `F5` to build and run the application.

### Using the `dotnet` CLI:

1.  Open a terminal in the root directory of the project.
2.  To build the project, run the following command:
    ```
    dotnet build
    ```
3.  To run the application, navigate to the UI project directory and use the `dotnet run` command:
    ```
    cd Mirage.UI
    dotnet run
    ```

## Development Conventions

*   **MVVM Pattern:** The WPF client should adhere to the Model-View-ViewModel (MVVM) pattern to separate the UI from the business logic.
*   **Dapper:** The data access layer (when created) will use Dapper for object-relational mapping.
*   **API-Centric:** All data access must go through the ASP.NET Core API. The WPF client should not directly access the database.
*   **Audit Trail:** All data changes must be logged in an `AuditLog` table.
