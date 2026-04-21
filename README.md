# LogiCore - Proyecto de Aprendizaje

## Descripcion
- **Proposito**: Proyecto de ejemplo para aprender y practicar arquitectura limpia, APIs RESTful y buenas practicas en .NET.
- **Que hace**: Implementa una API para gestionar conductores, vehiculos, ubicaciones, paquetes y envios con capas separadas en `Api`, `Application`, `Domain` e `Infrastructure`.
  - **Acceso basado en roles**: Roles Admin y Driver con autenticacion JWT.
  - **Flujo de recoleccion de paquetes**: Recoleccion iniciada por el conductor desde vendedores con trazabilidad completa (Pending -> InTransit -> AtDepot -> Delivered).
  - **Gestion inteligente de estado**: Maneja paquetes no recolectados en operaciones de llegada/finalizacion de envios.
  - **Ciclo de vida del envio**: Desde la creacion hasta despacho, llegada y finalizacion con sincronizacion automatica de paquetes.
- **Frontend en progreso**: [Next.js: https://github.com/LucasCttr/logicore-front]

## Arquitectura
- **Patron**: Clean Architecture / Onion - separa responsabilidades entre los proyectos `LogiCore.Api`, `LogiCore.Application`, `LogiCore.Domain` y `LogiCore.Infrastructure`.
- **Organizacion**: `Controllers` en la capa API, `Features` y `DTOs` en Application, entidades y reglas de negocio en Domain, acceso a datos y servicios externos en Infrastructure.

## Stack Tecnologico
- **Lenguaje**: `C#`
- **Runtime / Framework**: `.NET 8 (net8.0)`
- **Tipo de proyecto**: `ASP.NET Core Web API`
- **Base de datos**: `PostgreSQL` (provista por Npgsql / EF Core)
- **Contenedores**: `Docker` + `docker-compose` (`Dockerfile` y `docker-compose.yml` incluidos)

## Librerias y Herramientas Principales
- **AutoMapper**: mapeo entre entidades y DTOs (`AutoMapper`, `AutoMapper.Extensions.Microsoft.DependencyInjection`).
- **MediatR**: mediador para implementar patrones CQRS y separar la logica de aplicacion.
- **FluentValidation**: validacion de DTOs al ingreso (`FluentValidation.AspNetCore`).
- **Entity Framework Core + Npgsql**: acceso a datos y migraciones de EF Core con proveedor PostgreSQL.
- **Serilog**: logging estructurado (`Serilog.AspNetCore`, `Serilog.Sinks.Console`).
- **Swashbuckle / OpenAPI**: documentacion automatica de la API.
- **Hellang ProblemDetails**: manejo consistente de errores y respuestas problem details.
- **Prometheus**: metricas (`prometheus-net`) para monitoreo.

## Estructura del Repositorio (Resumen)
- **LogiCore.Api/**: punto de entrada, controllers, middlewares, filtros y configuraciones (appsettings).
  - Los controllers incluyen endpoints para el flujo de recoleccion de paquetes: `/packages/{id}/collect`, `/packages/{id}/deliver`, `/packages/{id}/mark-delivered`.
- **LogiCore.Application/**: DTOs, features, mappers y reglas de orquestacion.
  - **Features**: incluye handlers de comandos CollectPackage, MoveToDepot y MarkPackageAsDelivered.
- **LogiCore.Domain/**: entidades, value objects, excepciones y contratos.
  - **Estados de paquete**: PendingState (permite StartTransit), AtDepotState (permite Deliver), InTransitState, DeliveredState, etc.
  - **Gestion de estado de envio**: MarkAsArrived sincroniza solo paquetes InTransit a AtDepot; los Pending quedan sin cambios.
- **LogiCore.Infrastructure/**: implementaciones de repositorios, contexto EF, servicios externos e integraciones.
- **publish/**: artefactos listos para despliegue.

## Buenas Practicas y Convenciones Aplicadas
- **Separacion de capas**: evita fugas de dependencias entre capas; `Api` solo referencia `Application` e `Infrastructure` mediante interfaces.
- **Inyeccion de dependencias**: todas las dependencias registradas de forma central en `Program.cs`/startup.
- **DTOs y AutoMapper**: evita exponer entidades directamente por API.
- **Validacion en una sola capa**: `FluentValidation` para validar entradas antes de llegar a la logica de negocio.
- **Manejo centralizado de errores**: middleware global para transformar excepciones a `ProblemDetails` (Hellang).
- **Logging estructurado**: uso de `Serilog` para trazabilidad y busqueda.
- **Metricas y observabilidad**: endpoints de metricas para Prometheus.
- **Documentacion**: OpenAPI/Swagger integrado para pruebas y consumo.
- **Configuracion por entorno**: `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`.
- **Patron CQRS parcial**: separacion de comandos y consultas via `MediatR` cuando aplica.
- **Uso de migraciones**: gestion con EF Core para evolucionar el esquema en PostgreSQL.
- **Patron State Machine**: la entidad Package implementa transiciones segun estado (Pending -> InTransit -> AtDepot -> Delivered) via objetos de estado polimorficos.
- **Sincronizacion inteligente**: la llegada de envio solo sincroniza paquetes recolectados (InTransit); los pendientes permanecen pending para reintento.

## Patrones Implementados
A continuacion se listan los patrones estructurales y de diseno aplicados en el proyecto, con ejemplos y enlaces a implementaciones relevantes dentro del repositorio.

- **Repository Pattern**: contratos en la capa de dominio y adaptadores en `Infrastructure` para acceso a datos. Ejemplos: [LogiCore.Domain/Repositories/IPackageRepository.cs](LogiCore.Domain/Repositories/IPackageRepository.cs#L1) e implementacion registrada en [LogiCore.Api/Program.cs](LogiCore.Api/Program.cs#L70).

- **Unit of Work**: centraliza commits y transacciones, garantizando consistencia y publicando eventos de dominio antes de persistir cambios. Implementacion: [LogiCore.Infrastructure/Persistence/UnitOfWork.cs](LogiCore.Infrastructure/Persistence/UnitOfWork.cs#L1).

- **Domain Events / Event Dispatcher**: entidades que emiten eventos de dominio y handlers desacoplados (publicados via MediatR). Interfaz: [LogiCore.Domain/Common/Interfaces/IDomainEvent.cs](LogiCore.Domain/Common/Interfaces/IDomainEvent.cs#L1). Handlers de ejemplo: [LogiCore.Infrastructure/Events/PackageStatusChangedHandler.cs](LogiCore.Infrastructure/Events/PackageStatusChangedHandler.cs#L1).

- **Mediator / CQRS (Parcial)**: uso de MediatR para comandos y consultas; desacopla controllers de la logica de aplicacion y facilita testing. Registro y uso: [LogiCore.Api/Program.cs](LogiCore.Api/Program.cs#L127) y controllers (por ejemplo [LogiCore.Api/Controllers/PackagesController.cs](LogiCore.Api/Controllers/PackagesController.cs#L1)).
  - Nuevos comandos: `CollectPackageCommand` para flujo de recoleccion iniciado por conductor.

- **Pipeline Behaviors (MediatR)**: behaviors en el pipeline para validacion y acciones transversales (por ejemplo commit). Implementaciones: [LogiCore.Application/Common/Behaviors/RequestValidationBehavior.cs](LogiCore.Application/Common/Behaviors/RequestValidationBehavior.cs#L1) y [LogiCore.Application/Common/Behaviors/SaveChangesBehavior.cs](LogiCore.Application/Common/Behaviors/SaveChangesBehavior.cs#L1).

- **Validation Pipeline**: `FluentValidation` integrado como paso de pipeline para asegurar entradas validas antes de ejecutar logica.

- **Mapper (Adapter)**: perfiles de `AutoMapper` para transformar entidades a DTOs y viceversa; ejemplo: [LogiCore.Application/Mappers/DriverProfile.cs](LogiCore.Application/Mappers/DriverProfile.cs#L1).

- **Middleware Pipeline / Global Exception Handling**: middleware centralizado que transforma excepciones en `ProblemDetails` y mapea codigos HTTP de forma uniforme. Ver: [LogiCore.Api/Middlewares/GlobalExceptionHandler.cs](LogiCore.Api/Middlewares/GlobalExceptionHandler.cs#L1).

- **Action Filters**: filtros de accion para estandarizar respuestas y comportamientos transversales. Ejemplo: [LogiCore.Api/Filters/ResultActionFilter.cs](LogiCore.Api/Filters/ResultActionFilter.cs#L1).

- **State Machine Pattern**: la entidad Package implementa objetos de estado polimorficos (PendingState, AtDepotState, InTransitState, DeliveredState) que manejan transiciones permitidas:
  - PendingState maneja la recoleccion por conductor via `StartTransit()`.
  - AtDepotState ahora permite operaciones de transferencia y entrega.
  - La llegada de envio sincroniza inteligentemente solo paquetes recolectados, dejando los pendientes sin cambios.

- **Observer / Instrumentation**: servicios y adaptadores para metricas (Prometheus) y observabilidad, por ejemplo [LogiCore.Infrastructure/Services/DatabaseMetricsService.cs](LogiCore.Infrastructure/Services/DatabaseMetricsService.cs#L1).

- **Adapter / Abstraction for External Concerns**: interfaces en `Application` con implementaciones en `Infrastructure` (repositorios, servicios externos, servicio de usuario actual), registradas en DI en [LogiCore.Api/Program.cs](LogiCore.Api/Program.cs#L67-L75), facilitando sustitucion y testing.

## Proximos Pasos
- Implementacion de rutas optimas mediante heuristicas.
- Agregar tests unitarios y de integracion (xUnit / NUnit + Testcontainers para DB).
- Agregar politicas de seguridad: headers, rate limiting y pruebas de endpoints.

## Redis - Autocompletado de Direcciones
- **Que hace**: Redis se usa como ZSET lexicografico para autocompletar direcciones frecuentes.
- **Endpoint**: `GET /api/addresses/autocomplete?q=Av.%20Riv` devuelve hasta 5 sugerencias.
- **Seeding**: al iniciar en desarrollo, el servicio lee `Locations` y carga direcciones en el ZSET `addresses:zset`.
- **Deployment**: `docker-compose.yml` incluye un servicio `redis` (puerto 6379).
