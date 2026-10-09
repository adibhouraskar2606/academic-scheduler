# Academic Scheduler

A web application built with ASP.NET Core Blazor for managing student course assignments and validating academic eligibility.

## Technology Stack

- **Frontend:** Blazor Web App (.NET 8), Bootstrap
- **Backend:** C#, ASP.NET Core, service layer
- **Database:** SQLite with Entity Framework Core
- **Data Import:** CsvHelper
- **Testing:** xUnit, SQLite in-memory databases

## Features

### Student Management
- View students and academic information.
- Search students by name, email, or house.
- Review completed courses and credit limits.

### Course Catalog
- Browse available courses.
- Search by course number or name.
- Filter by department and instructor.

### Course Scheduling
- View each student's upcoming schedule.
- Assign eligible courses.
- Remove existing course assignments.
- Automatically recalculate assigned credits and remaining capacity.
- Filter courses by eligibility, department, and search term.

### Eligibility Validation
Course assignments are validated against the following rules:

1. Students cannot repeat completed courses.
2. Students cannot be assigned the same course twice.
3. Required prerequisites must already be completed.
4. Assignments cannot exceed the student's maximum credit load.

Courses assigned for the upcoming semester do not satisfy prerequisites for other courses in that same semester.

## Architecture

The application uses a layered structure:

- **Blazor Components:** User interface and interactions
- **Services:** Student retrieval, course retrieval, and enrollment business rules
- **Entity Framework Core:** Data access and relationships
- **SQLite:** Persistent storage
- **CSV Seeder:** Initial student and course data

Eligibility validation is centralized in `EnrollmentService` and reused by the UI and assignment operations.

## Getting Started

**Requirements:** .NET 8 SDK

From the repository root:

```bash
dotnet restore AcademicScheduler.csproj
dotnet run --project AcademicScheduler.csproj
```

The application automatically applies database migrations and imports starter CSV data when the database is empty.

Open the local URL displayed in the terminal.

## Running Tests

```bash
dotnet test Tests/AcademicScheduler.Tests.csproj
```

Automated tests cover prerequisite validation, completed-course restrictions, duplicate assignments, and credit-limit boundaries.

## Design Decisions

- **SQLite:** Lightweight persistence without requiring an external database server.
- **Entity Framework Core:** Relational modeling, migrations, and query support.
- **Service Layer:** Keeps business rules separate from Blazor components.
- **Server-Side Validation:** Rechecks eligibility before saving assignments.
- **SQLite In-Memory Tests:** Exercises business logic against a relational database without modifying application data.

## Assumptions and Limitations

- Starter data is imported from the supplied CSV files.
- Course prerequisites must be completed before enrollment.
- The starter course data does not specify course capacity, so seat availability is not enforced.
- Authentication, authorization, and concurrent scheduling conflict handling are outside the current scope.

## Future Improvements

- Authentication and advisor-specific permissions
- Course capacity and waitlists
- Transactional handling of concurrent enrollment requests
- Additional integration tests
- More detailed student academic reports
