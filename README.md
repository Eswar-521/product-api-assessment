# Product API Assessment

RESTful backend API for Products using .NET 8, ASP.NET Core Web API, SQL Server, Entity Framework Core, JWT authentication with refresh tokens, FluentValidation, Swagger/OpenAPI, Docker, and xUnit tests.

## Architecture

```text
Client / Swagger / Postman
        -> API Controllers
        -> Application Services + Validators
        -> Repository + Unit of Work
        -> Entity Framework Core
        -> SQL Server
```

## Project Structure

```text
src/
  ProductApi.API              ASP.NET Core Web API, controllers, middleware, Swagger
  ProductApi.Application      DTOs, interfaces, validators, services
  ProductApi.Domain           Entities and domain exceptions
  ProductApi.Infrastructure   EF Core DbContext, repositories, auth/token infrastructure
tests/
  ProductApi.Application.Tests
  ProductApi.API.Tests
```

## Main Endpoints

```http
POST   /api/v1/auth/register
POST   /api/v1/auth/login
POST   /api/v1/auth/refresh-token

GET    /api/v1/products
GET    /api/v1/products/{id}
POST   /api/v1/products
PUT    /api/v1/products/{id}
DELETE /api/v1/products/{id}
GET    /api/v1/products/{id}/items

GET    /health
```

Product endpoints require a JWT access token. `DELETE /api/v1/products/{id}` requires the `Admin` role.

## Run With Docker

```powershell
docker compose up --build
```

Open Swagger:

```text
http://localhost:8080/swagger
```

The Docker setup starts SQL Server and automatically creates the required tables.

## Run Locally

Install .NET 8 SDK and SQL Server/LocalDB first. Then update `src/ProductApi.API/appsettings.json` if your SQL Server connection string is different.

```powershell
dotnet restore
dotnet build
dotnet run --project src/ProductApi.API
```

Open the Swagger URL printed in the terminal, usually:

```text
https://localhost:7000/swagger
```

## Test Auth In Swagger

1. Call `POST /api/v1/auth/register`.
2. Use this sample body:

```json
{
  "userName": "admin",
  "email": "admin@example.com",
  "password": "Admin1234",
  "role": "Admin"
}
```

3. Copy the `accessToken` from the response.
4. Click `Authorize` in Swagger.
5. Paste the token.
6. Now call the product endpoints.

## Sample Product Body

```json
{
  "productName": "Laptop",
  "quantity": 5
}
```

## Run Tests

```powershell
dotnet test
```

## Notes

- EF Core uses `AsNoTracking()` for read-only queries.
- Collection endpoints support pagination with `pageNumber`, `pageSize`, and `search`.
- Errors are returned in a consistent JSON format through custom middleware.
- JWT refresh tokens are hashed before storage and rotated on refresh.
