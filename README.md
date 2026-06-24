# Didakt API
A multi-service game backend implementing player authentication and leaderboards. Built with microservice architecture in .NET 10 with minimal APIs and orchestrated by Docker Compose

.NET 10 · PostgreSQL · Redis · Docker · xUnit

## Services

### Auth Service
##### `Didakt.Api.Auth`
Handles user login and registration using signed JWTs for cross service authentication

#### Stack
.NET 10, PostgreSQL, EF Core, Npgsql, JWT Bearer, FluentValidation, Scalar UI

#### Endpoints
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/auth/register` | — | Register a new player |
| POST | `/auth/login` | — | Authenticate and receive a JWT |

### Leaderboard Service
##### `Didakt.Api.Leaderboard`
Stores and ranks player scores by category using Redis.

#### Stack
.NET 10, Redis, JWT Bearer, Scalar UI

#### Endpoints
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/leaderboard/{category}/score` | Required | Submit a score |
| GET | `/leaderboard/{category}/score?player=` | — | Retrieve a player's score |
| GET | `/leaderboard/{category}/top?count=` | — | Retrieve top N ranked players |

---
 
## Local Setup
 
**Prerequisites:** Docker Desktop, .NET 10 SDK

```bash
# Clone
git clone https://github.com/wsellinger/didakt-api.git
cd didakt-api
 
# Create shared secrets file
cp .env.shared.example .env.shared   # then fill in Jwt:Secret, Issuer, Audience
 
# Start all services
docker compose up --build
```

Service urls:
 
- Auth API: `http://localhost:5224`
- Leaderboard API: `http://localhost:5223`

Each service also has an `.http` file for manual endpoint testing from within Visual Studio.
 
## Run Tests
 
```bash
dotnet test
```
 
Test projects mirror service structure using one test file per method. Tests utilize xUnit, Moq, and EF Core in-memory providers.
 
---
 
## Design Notes

### Endpoints
Endpoints are implemented in "Minimal API" style to reduce boilerplate and improve performance. Encapsulation is maintained via Dependency Injection and separate files for service logic.

### Authentication
Cross service authentication is handled by auth tokens via JWT. These are returned upon login and included in all subsequent calls that require authentication.

### Relational Database
Identity is handled by a relational database via PostgreSQL. Structured persistent data benefits most from rigid data frameworks and relational mappings.

### Non-Relational Database
Leaderboard score data is handled by a non-relational database via Redis. This allows quick insertion, lookup, and ranking of data within a simple interface.
