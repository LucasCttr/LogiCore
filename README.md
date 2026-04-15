# LogiCore — Learning Project

## Description
- **Purpose**: Example project for learning and practicing clean architecture, RESTful APIs, and .NET best practices.
- **What it does**: Implements an API for managing drivers, vehicles, locations, packages, and shipments with separated layers for `Api`, `Application`, `Domain`, and `Infrastructure`.
  - **Role-Based Access**: Admin and Driver roles with JWT authentication
  - **Package Collection Workflow**: Driver-initiated pickup from sellers with full traceability (Pending → InTransit → AtDepot → Delivered)
  - **Smart State Management**: Handles packages not collected in shipment arrive/complete operations
  - **Shipment Lifecycle**: From creation through dispatch, arrival, and completion with automatic package synchronization
- **Frontend in progress:** [Next.js: https://github.com/LucasCttr/logicore-front]

## Architecture
- **Pattern**: Clean Architecture / Onion — separates responsibilities across projects `LogiCore.Api`, `LogiCore.Application`, `LogiCore.Domain`, and `LogiCore.Infrastructure`.
- **Organization**: `Controllers` in the API layer, `Features` and `DTOs` in Application, entities and business rules in Domain, data access and external services in Infrastructure.

## Tech Stack
- **Language**: `C#`
- **Runtime / Framework**: `.NET 8 (net8.0)`
- **Project Type**: `ASP.NET Core Web API`
- **Database**: `PostgreSQL` (provided by Npgsql / EF Core)
- **Containers**: `Docker` + `docker-compose` (`Dockerfile` and `docker-compose.yml` included)

## Main Libraries and Tools
- **AutoMapper**: mapping between entities and DTOs (`AutoMapper`, `AutoMapper.Extensions.Microsoft.DependencyInjection`).
- **MediatR**: mediator to implement CQRS patterns and separate application logic.
- **FluentValidation**: DTO validation at entry level (`FluentValidation.AspNetCore`).
- **Entity Framework Core + Npgsql**: data access and EF Core migrations with PostgreSQL provider.
- **Serilog**: structured logging (`Serilog.AspNetCore`, `Serilog.Sinks.Console`).
- **Swashbuckle / OpenAPI**: automatic API documentation.
- **Hellang ProblemDetails**: consistent error handling and problem details responses.
- **Prometheus**: metrics (`prometheus-net`) for monitoring.

## Repository Structure (Summary)
- **LogiCore.Api/**: entry point, controllers, middlewares, filters, and configurations (appsettings).
  - Controllers include endpoints for package collection workflow: `/packages/{id}/collect`, `/packages/{id}/deliver`, `/packages/{id}/mark-delivered`
- **LogiCore.Application/**: DTOs, features, mappers, and orchestration rules.
  - **Features**: Includes CollectPackage, MoveToDepot, MarkPackageAsDelivered command handlers
- **LogiCore.Domain/**: entities, value objects, exceptions, and contracts.
  - **Package States**: PendingState (allows StartTransit), AtDepotState (allows Deliver), InTransitState, DeliveredState, etc.
  - **Shipment State Management**: MarkAsArrived syncs only InTransit packages to AtDepot; Pending packages remain unchanged
- **LogiCore.Infrastructure/**: repository implementations, EF context, external services, and integrations.
- **publish/**: deployment-ready artifacts.

## Best Practices and Conventions Applied
- **Layer Separation**: avoids dependency leakage between layers; `Api` only references `Application` and `Infrastructure` through interfaces.
- **Dependency Injection**: all dependencies registered in central `Program.cs`/startup.
- **DTOs and AutoMapper**: avoid exposing entities directly via API.
- **Single-Layer Validation**: `FluentValidation` for validating inputs before reaching business logic.
- **Centralized Error Handling**: global middleware to transform exceptions into `ProblemDetails` (Hellang).
- **Structured Logging**: `Serilog` usage for traceability and search capabilities.
- **Metrics and Observability**: metrics endpoints for Prometheus.
- **Documentation**: integrated OpenAPI/Swagger for testing and consumption.
- **Environment Configuration**: `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`.
- **Partial CQRS Pattern**: separation of commands and queries via `MediatR` when applicable.
- **Migrations Usage**: manage with EF Core to evolve PostgreSQL schema.
- **State Machine Pattern**: Package entity implements state-aware transitions (Pending → InTransit → AtDepot → Delivered) via polymorphic state objects.
- **Smart Synchronization**: Shipment arrival only syncs collected packages (InTransit); pending packages remain pending for retry collection.


## Implemented Patterns
Below are the structural and design patterns applied in the project, with examples and links to relevant implementations within the repo.

- **Repository Pattern**: contracts in the domain layer and adapters in `Infrastructure` for data access. Examples: [LogiCore.Domain/Repositories/IPackageRepository.cs](LogiCore.Domain/Repositories/IPackageRepository.cs#L1) and implementation registered in [LogiCore.Api/Program.cs](LogiCore.Api/Program.cs#L70).

- **Unit of Work**: centralizes commits and transactions, ensuring consistency and publishing domain events before persisting changes. Implementation: [LogiCore.Infrastructure/Persistence/UnitOfWork.cs](LogiCore.Infrastructure/Persistence/UnitOfWork.cs#L1).

- **Domain Events / Event Dispatcher**: entities that emit domain events and decoupled handlers (published via MediatR). Interface: [LogiCore.Domain/Common/Interfaces/IDomainEvent.cs](LogiCore.Domain/Common/Interfaces/IDomainEvent.cs#L1). Example handlers: [LogiCore.Infrastructure/Events/PackageStatusChangedHandler.cs](LogiCore.Infrastructure/Events/PackageStatusChangedHandler.cs#L1).

- **Mediator / CQRS (Partial)**: MediatR usage for commands and queries; decouples controllers from application logic and facilitates testing. Registration and usage: [LogiCore.Api/Program.cs](LogiCore.Api/Program.cs#L127) and controllers (e.g. [LogiCore.Api/Controllers/PackagesController.cs](LogiCore.Api/Controllers/PackagesController.cs#L1)).
  - New commands: `CollectPackageCommand` for driver-initiated pickup workflow

- **Pipeline Behaviors (MediatR)**: behaviors in the pipeline for validation and cross-cutting actions (e.g. commit). Implementations: [LogiCore.Application/Common/Behaviors/RequestValidationBehavior.cs](LogiCore.Application/Common/Behaviors/RequestValidationBehavior.cs#L1) and [LogiCore.Application/Common/Behaviors/SaveChangesBehavior.cs](LogiCore.Application/Common/Behaviors/SaveChangesBehavior.cs#L1).

- **Validation Pipeline**: `FluentValidation` integrated as a pipeline step to ensure valid input before executing logic.

- **Mapper (Adapter)**: `AutoMapper` profiles to transform entities to DTOs and vice versa; example: [LogiCore.Application/Mappers/DriverProfile.cs](LogiCore.Application/Mappers/DriverProfile.cs#L1).

- **Middleware Pipeline / Global Exception Handling**: centralized middleware that transforms exceptions into `ProblemDetails` and maps HTTP codes uniformly. See: [LogiCore.Api/Middlewares/GlobalExceptionHandler.cs](LogiCore.Api/Middlewares/GlobalExceptionHandler.cs#L1).

- **Action Filters**: action filters to standardize responses and cross-cutting behaviors. Example: [LogiCore.Api/Filters/ResultActionFilter.cs](LogiCore.Api/Filters/ResultActionFilter.cs#L1).

- **State Machine Pattern**: Package entity implements polymorphic state objects (PendingState, AtDepotState, InTransitState, DeliveredState) that handle allowed transitions:
  - PendingState handles driver collection via `StartTransit()`
  - AtDepotState now allows both transfer and delivery operations
  - Shipment arrival smartly syncs only collected packages, leaving pending packages unchanged

- **Observer / Instrumentation**: services and adapters for metrics (Prometheus) and observability, for example [LogiCore.Infrastructure/Services/DatabaseMetricsService.cs](LogiCore.Infrastructure/Services/DatabaseMetricsService.cs#L1).

- **Adapter / Abstraction for External Concerns**: interfaces in `Application` with implementations in `Infrastructure` (repositories, external services, current user service), registered in DI in [LogiCore.Api/Program.cs](LogiCore.Api/Program.cs#L67-L75), facilitating substitution and testing.


## How to Run Locally
1. Ensure you have `.NET 8 SDK`, `docker`, and `docker-compose` installed (optional for containers).
2. Copy/adjust the connection string in `LogiCore.Api/appsettings.Development.json`.
3. Build and run from the repo root:

```bash
dotnet restore
dotnet build
dotnet run --project LogiCore.Api/LogiCore.Api.csproj
```

4. Alternative with Docker:

```bash
docker-compose up --build
```

5. OpenAPI documentation is usually available at `http://localhost:5000/swagger` (depending on `Program.cs` configuration).

## Deployment
- Contains `Dockerfile` and `docker-compose.yml` for packaging. There's also a `publish/` folder with ready-to-deploy artifacts.
- There's a `migrations.sql` file that can help in environments where automatic migrations are not applied.

## Next Steps
- Finalize frontend.
- Optimal route implementation via heuristics.
- Add unit and integration tests (xUnit / NUnit + Testcontainers for DB).
- Integrate CI pipeline (GitHub Actions) running `dotnet build`, `dotnet test`, and static analysis.
- Add security policies: headers, rate limiting, and endpoint tests.
 
## Redis — Address Autocomplete

- **What it does**: Redis is used as a lexicographic ZSET for autocompleting frequent addresses.
- **Endpoint**: `GET /api/addresses/autocomplete?q=Av.%20Riv` returns up to 5 suggestions.
- **Seeding**: on development startup the service reads `Locations` and loads addresses into the `addresses:zset` ZSET.
- **Deployment**: `docker-compose.yml` includes a `redis` service (port 6379).


