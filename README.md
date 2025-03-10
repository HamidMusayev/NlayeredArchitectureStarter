# Net7ApiStarter

**Code contributors**

![Alt](https://repobeats.axiom.co/api/embed/f8c50b5c55ce520d8198a81cb6f63150cec32209.svg "Repobeats analytics image")

**Core Layers and Their Roles**

**1\. API**

- **Purpose**: This is the presentation layer, containing:
    - **Controllers**: Handle HTTP requests and delegate to BLL or MediatR.
    - **ActionFilters** and **Middlewares**: Implement cross-cutting concerns (e.g., validation, logging, localization, exception handling).
    - **GraphQL** and **Hubs**: Add support for advanced communication protocols like GraphQL and SignalR.
    - **Services**: Specialized services like RedisIndexCreatorService.

**Architecture Alignment**: Clearly part of a **Layered Architecture** for the presentation tier.

**2\. BLL (Business Logic Layer)**

- **Purpose**: Contains the core business logic.
    - **Abstract**: Interfaces for business services.
    - **Concrete**: Implementations of business services.
    - **MediatR**: Shows the implementation of CQRS:
        - Commands, Queries, and Handlers are defined for tasks like AddOrganizationCommand.

**Architecture Alignment**: Incorporates **CQRS principles** with a focus on separation of read and write operations.

**3\. CORE**

- **Purpose**: Contains reusable, application-agnostic logic and utilities:
    - **Constants** and **Config**: Centralized configuration management.
    - **Helper**: General-purpose helpers (e.g., file operations, security).
    - **Logging** and **Localization**: Cross-cutting concerns.

**Architecture Alignment**: Acts as the **Core Layer** in **Clean Architecture** to centralize reusable logic and decouple it from other layers.

**4\. DAL (Data Access Layer)**

- **Purpose**: Manages database interactions.
    - **Abstract/Concrete**: Repository pattern for data access.
    - **CustomMigrations** and **DatabaseContext**: Support for Entity Framework Core.
    - **GenericRepositories**: Provides a generic approach for repository operations.
    - **UnitOfWorks**: Manages database transactions.

**Architecture Alignment**: Typical **Data Access Layer** in a **Layered Architecture** setup.

**5\. DTO**

- **Purpose**: Data Transfer Objects used for communication between layers.
    - Contains validators, such as UserValidators, to validate DTOs before processing.

**Architecture Alignment**: Supports the **Layered Architecture** and **Clean Architecture** principles by separating transport objects from domain models.

**6\. ENTITIES**

- **Purpose**: Represents domain models and enums used across the application.
    - Includes subfolders for Redis entities, logging entities, and user entities.

**Architecture Alignment**: Part of the **Domain Layer** in **Clean Architecture**.

**7\. SOURCE**

- **Purpose**: A specialized folder for additional helpers, builders, models, and workers.
    - Likely used for modular or independent utilities.

**Architecture Alignment**: Modular utility helpers, typically part of a **Clean Architecture** utility layer.

**Other Observations**

1. **MediatR and CQRS**: The presence of commands, queries, and handlers under BLL/MediatR shows an effort to implement **CQRS principles**.
2. **GraphQL**: Indicates support for advanced querying, aligning with modern API design.
3. **Redis**: Redis entities and services suggest caching or session management.
4. **Cross-Cutting Concerns**: The use of middlewares and helpers in CORE and API demonstrates adherence to **separation of concerns**.

**Architectural Classification**

The project integrates multiple architectural patterns:

1. **Layered Architecture**: Clear separation of API, BLL, DAL, and CORE.
2. **Clean Architecture**:
    - CORE acts as a domain layer.
    - Dependencies flow inward, with reusable utilities and abstractions.
3. **CQRS**: Found in BLL/MediatR with commands and queries.

This is a **hybrid architecture**, combining the strengths of **Layered Architecture**, **Clean Architecture**, and **CQRS** to achieve scalability, maintainability, and modularity.

**Structure** //todo

    │── ── ── ── ── ── ── ── ── ── ── ── ── ──
    │ .NET 7.0 WebApi Starter Project
    │── ── ── ── ── ── ── ── ── ── ── ── ── ──
    │
    ├── API 
    │   ├── ActionFilters
    │   │   ├── LogActionFilter
    │   │   └── ModelValidatorActionFilter
    │   ├── Attributes
    │   │   ├── ValidateForgeryTokenAttribute
    │   │   └── ValidateTokenAttribute
    │   ├── Containers
    │   │   └── DependencyContainer
    │   ├── Controllers
    │   │   ├── UserController
    │   │   └── ...
    │   ├── Graphql
    │   │   ├── Role
    │   │   │   ├── Mutation
    │   │   │   └── Query
    │   │   └── ...
    │   ├── Hubs
    │   │   ├── UserHub
    │   │   └── ...
    │   ├── Middlewares
    │   │   ├── AntiForgery
    │   │   │   ├── AntiForgeryTokenValidator
    │   │   │   └── ValidateAntiForgeryTokenMiddleware
    │   │   ├── ExceptionMiddleware
    │   │   └── LocalizationMiddleware
    │   └── Services
    │       └── RedisIndexCreatorService
    │
    │── ── ── ── ── ── ── ── ── ── ── ── ── ──
    │
    ├── BLL     
    │   ├── Abstract
    │   │   ├── IUserService
    │   │   └── ...
    │   ├── Concrete
    │   │   ├── UserService
    │   │   └── ...
    │   ├── Mappers
    │   │   ├── Automapper
    │   │   ├── UserMapper
    │   │   └── ...
    │   └── MediatR
    │       ├── OrganizationCQRS
    │       │   ├── Commands
    │       │   │   ├── AddOrganizationCommand
    │       │   │   └── ...
    │       │   ├── Handlers
    │       │   │   ├── AddOrganizationHandler
    │       │   │   ├── GetOrganizationListHandler
    │       │   │   └── ...
    │       │   └── Queries
    │       │       ├── GetOrganizationListQuery
    │       │       └── ...
    │       └── ...
    │       //TODO ADD RABBITMQ HERE
    │ 
    │── ── ── ── ── ── ── ── ── ── ── ── ── ──
    │
    ├── CORE
    │   ├── Abstract
    │   │   └── ISftpService
    │   │   └── IUtilService
    │   ├── Concrete
    │   │   └── SftpService
    │   │   └── UtilService
    │   ├── Constants
    │   │   ├── Constants
    │   │   └── LocalizationConstants
    │   ├── Config
    │   │   ├── AuthSettings
    │   │   ├── ConfigSettings
    │   │   ├── ConnectionStrings
    │   │   ├── Controllable
    │   │   ├── CryptographySettings
    │   │   ├── HttpClientSettings
    │   │   ├── HttpHeader
    │   │   ├── MailSettings
    │   │   ├── RedisSettings
    │   │   ├── RequestSettings
    │   │   ├── SentrySettings
    │   │   ├── SftpSettings
    │   │   └── SwaggerSettings
    │   ├── Helper
    │   │   ├── ExpressionHelper
    │   │   ├── FileHelper
    │   │   ├── FilterHelper
    │   │   ├── ObjectSerializer
    │   │   └── SecurityHelper
    │   ├── Logging
    │   │   ├── ILoggerManager
    │   │   └── LoggerManager
    │   └── Localization
    │       ├── TranslatorExtension
    │       ├── Messages
    │       ├── MsgResource.az
    │       ├── MsgResource.en
    │       └── MsgResource.ru
    │
    │── ── ── ── ── ── ── ── ── ── ── ── ── ──
    │
    ├── DAL
    │   ├── Abstract
    │   │   ├── IUserRepository
    │   │   └── ...
    │   ├── Concrete
    │   │   ├── UserRepository
    │   │   └── ...
    │   ├── CustomMigrations
    │   │   └── DataSeed
    │   ├── DatabaseContext
    │   │   └── DataContext
    │   ├── GenericRepositories
    │   │   ├── Abstract
    │   │   │   └── IGenericRepository
    │   │   └── Concrete
    │   │       └── GenericRepository
    │   ├── Migrations
    │   │   └── ...
    │   ├── UnitOfWorks
    │   │   ├── Abstract
    │   │   │   └── IUnitOfWork
    │   │   └── Concrete
    │   │       └── UnitOfWork
    │   └── Utility
    │       ├── PaginatedList 
    │       └── PaginationInfo
    │
    │── ── ── ── ── ── ── ── ── ── ── ── ── ──
    │
    ├── DTO  
    │   ├── User
    │   │   ├── UserValidators
    │   │   │   ├── AddDtoValidator
    │   │   │   └── ...
    │   │   ├── UserToAddDto
    │   │   └── ...
    │   └── ...
    │
    │── ── ── ── ── ── ── ── ── ── ── ── ── ──
    │
    ├── ENTITIES
    │   ├── Entities
    │   │   ├── Redis
    │   │   │   └── ..
    │   │   ├── Logging
    │   │   │   └── ..
    │   │   ├── User
    │   │   └── ..
    │   ├── Enums
    │   │   ├── UserType
    │   │   └── ..
    │   └── IEntity
    │ 
    │── ── ── ── ── ── ── ── ── ── ── ── ── ──
    │
    ├── SOURCE
    │   ├── Builders
    │   ├── Helpers
    │   ├── Models
    │   └── Workers
    │
    └── ── ── ── ── ── ── ── ── ── ── ── ── ──
