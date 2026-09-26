# Guildly Identity API

Guildly Identity API is a web service responsible for user identity features: 
- registration;
- email confirmation;
- login; 
- refresh token rotation;
- logout;
- password change/reset;
- updating email and phone number.

## Tech Stack

This project is built with **C#**, **ASP.NET Core Minimal API**, and **.NET 10**.

It also uses:

- Entity Framework Core
- PostgreSQL
- FluentValidation
- JWT Bearer Authentication
- OpenAPI
- Scalar UI
- Docker

## Tests

Running tests:

```powershell
dotnet test Guildly.Identity.slnx
```

## Running With Docker

Start the database:

```powershell
docker compose -f .\docker-compose-database-only.yml up -d
```

Build docker image:

```powershell
docker build -t guildly-identity-api .
```

Run docker container (yeah, too many env):

```powershell
docker run -d `
  --name guildly-identity-api `
  -p 8080:8080 `
  -e Database__Server=host.docker.internal `
  -e Database__Port=5005 `
  -e Database__Name=guildly_db_tm `
  -e Database__Username=postgres `
  -e Database__Password=postgres `
  -e Jwt__SecretKey=secret-key-for-json-web-token-32 `
  -e Jwt__Issuer=http://localhost:8080 `
  -e Jwt__Audience=guildly-api `
  -e RefreshToken__SecretKey=secret-key-for-refresh-token-32c `
  -e RefreshToken__Length=32 `
  -e VerificationCode__SecretKey=secret-key-for-verification-code `
  -e VerificationSession__AllowedAttempts=5 `
  -e VerificationSession__ExpirationTimeInMinutes=20 `
  guildly-identity-api
```

The API will be available at:
```http://localhost:8080```

OpenAPI documentation will be available at:
```http://localhost:8080/scalar```