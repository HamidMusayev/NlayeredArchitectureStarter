# NLayered Starter Project

This is a starter project built with ASP.NET Core, featuring a variety of integrated services and configurations to help you get started quickly.

## Features

- **API Versioning**: Supports multiple API versions using URL segments, headers, and media types.
- **Rate Limiting**: Configured to limit the number of requests to prevent abuse.
- **Repository Pattern**: Implements the repository pattern for data access.
- **SignalR**: Real-time web functionality using SignalR.
- **Unit of Work**: Manages transactions and coordinates changes across multiple repositories.
- **Output Caching**: Caches responses to improve performance.
- **Redis**: Configurable Redis support for caching and other purposes.
- **MongoDB**: Configurable MongoDB support for NoSQL database operations.
- **Elasticsearch**: Configurable Elasticsearch support for search and analytics.
- **MediatR**: Implements the mediator pattern for handling requests and notifications.
- **CQRS**: Implements the Command Query Responsibility Segregation pattern.
- **MiniProfiler**: Integrated MiniProfiler for performance profiling.
- **Refit**: Simplifies HTTP API calls with Refit clients.
- **GraphQL**: Supports GraphQL queries and mutations with Voyager UI.
- **Health Checks**: Monitors the health of the application and its dependencies.
- **Authentication**: Configurable authentication settings.
- **CORS**: Configured to allow cross-origin requests.
- **Exception Handling**: Centralized exception handling with NummyExceptionHandler.
- **HTTP Logging**: Logs HTTP requests and responses for debugging and monitoring.
- **Code Logging**: Logs code execution for debugging and monitoring.
- **Swagger**: Integrated Swagger for API documentation.
- **Localization**: Middleware for handling localization.
- **Static Files**: Serves static files.
- **FluentValidation**: Validates models using FluentValidation.
- **AutoMapper**: Maps objects using AutoMapper.
- **Entity Framework Core**: Configured for PostgreSQL database access.
- **Cryptography**: Configurable cryptography settings.
- **SFTP**: Configurable SFTP server settings.
- **JWT Authentication**: Secure authentication using JSON Web Tokens.
- **Background Services**: Support for running background tasks.
- **Hangfire**: Integrated Hangfire for background job processing.
- **Serilog**: Configurable logging with Serilog.
- **Polly**: Resilience and transient-fault-handling library.

## Configuration

The project uses `appsettings.Development.json` for configuration. Key settings include:

- **AuthSettings**: JWT authentication settings.
- **ConnectionStrings**: Database connection strings.
- **MailSettings**: Email server settings.
- **RedisSettings**: Redis connection settings.
- **ElasticSearchSettings**: Elasticsearch connection settings.
- **MongoDbSettings**: MongoDB connection settings.
- **CryptographySettings**: Cryptography settings.
- **SftpSettings**: SFTP server settings.
- **SwaggerSettings**: Swagger UI settings.
- **RequestSettings**: Request settings.
- **ToDoClientSettings**: ToDo client settings.

## Getting Started

1. Clone the repository.
2. Update the configuration settings in `appsettings.Development.json`.
3. Build and run the project using your preferred IDE (e.g., JetBrains Rider).

## Running the Application

To run the application, use the following command:

```sh
dotnet run
```

## License

This project is licensed under the MIT License.