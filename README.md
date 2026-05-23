# Posts API

An ASP.NET Core Web API for user authentication and post management. The API uses JWT bearer authentication, Entity Framework Core with SQL Server, Redis-backed response caching for post listings, and Swagger/OpenAPI documentation in development.

## Features

- User registration and login with JWT tokens
- Password hashing with BCrypt
- Authenticated CRUD operations for posts
- Ownership checks for updating and deleting posts
- Paginated post listing
- Redis caching for post list responses
- SQL Server persistence with EF Core
- Global JSON error handling
- Swagger UI in development
- Seeded demo user and sample posts on first run

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Redis
- JWT bearer authentication
- Swagger / OpenAPI

## Project Structure

```text
Posts/
├── Controllers/      # API controllers
├── DTOs/             # Request and response contracts
├── Data/             # EF Core DbContext and database seeding
├── Helpers/          # JWT and password utilities
├── Middleware/       # Global exception middleware
├── Models/           # Entity models
├── Repositories/     # Data access abstractions and implementations
├── Services/         # Business logic
├── Migrations/       # EF Core migrations
└── Program.cs        # Application startup and dependency injection
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server running locally
- Redis running locally

The default development configuration expects:

```text
SQL Server: localhost,1433
Database: Posts
Redis: localhost:6379
```

## Configuration

The app reads configuration from `appsettings.json`, environment variables, and the usual ASP.NET Core configuration providers.

Default connection strings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=Posts;User Id=sa;Password=SqlServer@5000;TrustServerCertificate=True;MultipleActiveResultSets=True",
    "Redis": "localhost:6379"
  }
}
```

JWT settings:

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "PostsApi",
    "Audience": "PostsApiClients",
    "ExpirationMinutes": 60
  }
}
```

For local development, keep secrets out of source control by using user secrets or environment variables.

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=Posts;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=True"
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379"
dotnet user-secrets set "Jwt:Key" "replace-with-a-long-random-secret-key"
```

## Run Locally

Restore dependencies:

```bash
dotnet restore
```

Apply the database migration:

```bash
dotnet ef database update
```

Run the API:

```bash
dotnet run
```

The development profile exposes:

- HTTP: `http://localhost:5248`
- HTTPS: `https://localhost:7109`
- Swagger UI: `https://localhost:7109/swagger`

The database initializer seeds a demo user and three sample posts when the database is empty.

```text
Email: demo@example.com
Password: Demo@123
```

## API Endpoints

### Authentication

| Method | Endpoint | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | No | Register a new user |
| POST | `/api/auth/login` | No | Login and receive a JWT |

### Posts

| Method | Endpoint | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/posts` | Yes | Create a post |
| GET | `/api/posts?page=1&pageSize=10` | Yes | Get paginated posts |
| GET | `/api/posts/{id}` | Yes | Get a post by ID |
| PUT | `/api/posts/{id}` | Yes | Update one of your posts |
| DELETE | `/api/posts/{id}` | Yes | Delete one of your posts |

## Example Requests

Register:

```bash
curl -X POST https://localhost:7109/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"newuser@example.com","password":"Secure@123"}'
```

Login:

```bash
curl -X POST https://localhost:7109/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@example.com","password":"Demo@123"}'
```

Create a post:

```bash
curl -X POST https://localhost:7109/api/posts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{"title":"My First Post","description":"This is the description of my first post."}'
```

Get posts:

```bash
curl -X GET "https://localhost:7109/api/posts?page=1&pageSize=10" \
  -H "Authorization: Bearer {token}"
```

Update a post:

```bash
curl -X PUT https://localhost:7109/api/posts/{id} \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{"title":"Updated Post Title","description":"Updated description."}'
```

Delete a post:

```bash
curl -X DELETE https://localhost:7109/api/posts/{id} \
  -H "Authorization: Bearer {token}"
```

## Response Examples

Successful login:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "email": "demo@example.com",
  "userId": "00000000-0000-0000-0000-000000000000",
  "expiresAt": "2026-05-24T12:00:00Z"
}
```

Paginated posts:

```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000000",
      "title": "Getting Started with ASP.NET Core",
      "description": "ASP.NET Core is a cross-platform, high-performance framework for building modern cloud-based apps.",
      "createdAt": "2026-05-24T12:00:00Z",
      "userId": "00000000-0000-0000-0000-000000000000"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 3,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

Error response:

```json
{
  "status": 404,
  "error": "Post with ID {id} not found.",
  "detail": "The requested resource was not found."
}
```

## Development Commands

Build the project:

```bash
dotnet build
```

Add a new EF Core migration:

```bash
dotnet ef migrations add MigrationName
```

Update the database:

```bash
dotnet ef database update
```

Run with the HTTPS launch profile:

```bash
dotnet run --launch-profile https
```
