# ExpenseTracker API

A .NET Core Web API for tracking expenses with authentication, caching, and detailed documentation.

## Technologies Used

- .NET 9.0
- Entity Framework Core 9.0
- SQL Server
- Redis (for caching)
- JWT Authentication
- Swagger/OpenAPI
- AutoMapper

## Features

- User authentication with JWT tokens and refresh tokens
- Redis caching for improved performance
- Expense tracking and management
- Swagger documentation
- Exception handling middleware
- CORS support

## Configuration

The application uses the following key configurations:

- Database connection string in `appsettings.json`
- JWT settings (Issuer, Audience, Key)
- Redis connection
- CORS policy

## Authentication

The API uses JWT Bearer token authentication:

- Tokens expire after 1 hour
- Refresh token mechanism implemented
- Secured endpoints require valid JWT token

## API Documentation

API documentation is available through Swagger UI at `/swagger` when running in development mode.

## Caching

Redis caching is implemented for:

- Expense queries with sliding expiration
- Cache invalidation on data updates
- Configurable cache duration
