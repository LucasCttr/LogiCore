# LogiCore — Proyecto de aprendizaje

## Descripción
- **Propósito**: Proyecto de ejemplo para fines de aprendizaje y práctica de arquitectura limpia, APIs RESTful y buenas prácticas en .NET.
- **Qué hace**: Implementa una API para gestionar conductores, vehículos, ubicaciones, paquetes y envíos con capas separadas para `Api`, `Application`, `Domain` e `Infrastructure`.

## Arquitectura
- **Patrón**: Clean Architecture / Onion — separa responsabilidades en proyectos `LogiCore.Api`, `LogiCore.Application`, `LogiCore.Domain` y `LogiCore.Infrastructure`.
- **Organización**: `Controllers` en la capa API, `Features` y `DTOs` en Application, entidades y reglas de negocio en Domain, acceso a datos y servicios externos en Infrastructure.

## Stack tecnológico
- **Lenguaje**: `C#`
- **Runtime / Framework**: `.NET 8 (net8.0)`
- **Tipo de proyecto**: `ASP.NET Core Web API`
- **Base de datos**: `PostgreSQL` (provisto por Npgsql / EF Core)
- **Contenedores**: `Docker` + `docker-compose` (archivos `Dockerfile` y `docker-compose.yml` incluidos)

## Librerías y herramientas principales
- **AutoMapper**: mapeo entre entidades y DTOs (`AutoMapper`, `AutoMapper.Extensions.Microsoft.DependencyInjection`).
- **MediatR**: mediador para implementar patrones CQRS y separar lógica de aplicación.
- **FluentValidation**: validación de DTOs a nivel de entrada (`FluentValidation.AspNetCore`).
- **Entity Framework Core + Npgsql**: acceso a datos y migraciones EF Core con proveedor PostgreSQL.
- **Serilog**: logging estructurado (`Serilog.AspNetCore`, `Serilog.Sinks.Console`).
- **Swashbuckle / OpenAPI**: documentación automática de la API.
- **Hellang ProblemDetails**: manejo consistente de errores y respuestas problem details.
- **Prometheus**: métricas (`prometheus-net`) para monitorización.

## Estructura del repositorio (resumen)
- **LogiCore.Api/**: punto de entrada, controllers, middlewares, filtros y configuraciones (appsettings).
- **LogiCore.Application/**: DTOs, features, mappers y reglas de orquestación.
- **LogiCore.Domain/**: entidades, value objects, excepciones y contratos.
- **LogiCore.Infrastructure/**: implementación de repositorios, contexto de EF, servicios externos e integraciones.
- **publish/**: artefactos listos para despliegue.

## Buenas prácticas y convenciones aplicadas
- **Separación de capas**: evita fugas de dependencias entre capas; `Api` sólo referencia `Application` e `Infrastructure` a través de interfaces.
- **Inyección de dependencias**: todas las dependencias se registran en `Program.cs`/startup central.
- **DTOs y AutoMapper**: evitar exponer entidades directamente por la API.
- **Validación en una sola capa**: `FluentValidation` para validar entradas antes de llegar a la lógica de negocio.
- **Manejo centralizado de errores**: middleware global para transformar excepciones en `ProblemDetails` (Hellang).
- **Logging estructurado**: uso de `Serilog` para trazabilidad y facilidad de búsqueda.
- **Métricas y observabilidad**: endpoints de métricas para Prometheus.
- **Documentación**: OpenAPI/Swagger integrado para facilitar pruebas y consumo.
- **Configuración por entorno**: `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`.
- **Patrón CQRS parcial**: separación de comandos y consultas vía `MediatR` cuando aplica.
- **Uso de migraciones**: gestionarlas con EF Core para evolucionar el esquema de PostgreSQL.

## Cómo ejecutar localmente
1. Asegúrate de tener instalado `.NET 8 SDK`, `docker` y `docker-compose` (opcional para contenedores).
2. Copia/ajusta la cadena de conexión en `LogiCore.Api/appsettings.Development.json`.
3. Construir y ejecutar desde la raíz del repo:

```bash
dotnet restore
dotnet build
dotnet run --project LogiCore.Api/LogiCore.Api.csproj
```

4. Alternativa con Docker:

```bash
docker-compose up --build
```

5. La documentación OpenAPI suele quedar en `http://localhost:5000/swagger` (según configuración de `Program.cs`).

## Despliegue
- Contiene `Dockerfile` y `docker-compose.yml` para empaquetado. También hay un `publish/` con artefactos preparados.
- Hay un archivo `migrations.sql` que puede ayudar en entornos donde no se aplican migraciones automáticas.

## Sugerencias y próximos pasos (para aprendizaje)
- Implementacion de rutas optimas mediante heurísticas.
- Añadir pruebas unitarias e integración (xUnit / NUnit + Testcontainers para DB).
- Integrar pipeline CI (GitHub Actions) que ejecute `dotnet build`, `dotnet test` y análisis estático.
- Añadir políticas de seguridad: headers, rate limiting y pruebas de endpoints.

