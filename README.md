# Spythere Games Services — Game Developer & Backend Platform

A cross-platform game backend and developer website built with .NET 10 Minimal APIs, PostgreSQL (EF Core 10), Google OAuth 2.0 verification, and React 19 / TypeScript / Tailwind CSS v4.

---

## Architecture & System Overview

Spythere Games Services provides a backend API and interactive web frontend for cross-platform game score aggregation, player identity verification, and leaderboard sorting.

The project is split into two distinct applications:
- **Backend Service (`SpythereGamesServices`)**: A stateless .NET 10 Web API utilizing Minimal APIs, Entity Framework Core 10 ORM connected to PostgreSQL, and dual-layer security middleware for write endpoints.
- **Frontend Presentation Layer (`frontend`)**: A single-page application built with React 19, Vite 8, TypeScript 6, and Tailwind CSS v4.

---

## Technology Stack

### Backend (.NET 10)
- **Framework & API:** .NET 10, ASP.NET Core Minimal APIs, native OpenAPI generation (`Microsoft.AspNetCore.OpenApi`)
- **Database & ORM:** PostgreSQL (`Npgsql`), Entity Framework Core 10 with Code-First Migrations
- **Authentication & Security:** Google OAuth 2.0 server-side token verification (`Google.Apis.Auth`), custom HTTP header validation (`X-Api-Key`)
- **Pipeline & Performance:** Dependency injection (`AddScoped`), extension-method endpoint mapping, `CancellationToken` support across HTTP handlers and Entity Framework queries, `AsNoTracking` read optimizations, and `ExecuteDeleteAsync` bulk deletions

### Frontend (React 19)
- **Framework & Build:** React 19, Vite 8, TypeScript 6 (`strict` mode enabled)
- **Styling:** Tailwind CSS v4 (`@tailwindcss/vite`), custom CSS retro gaming styles, persistent dark/light theme toggle
- **Linting & Analysis:** Oxlint with type-aware TypeScript rules (`oxc`, `oxlint-tsgolint`)

---

## Dual-Layer Security & Authentication

To protect leaderboard integrity against unauthorized score manipulation and bot flooding, state-mutating endpoints enforce a two-tier verification flow:

```
[Game Client / Web SPA] 
       |
       +-- (1) HTTP Request with X-Api-Key Header
       +-- (2) Google OAuth AuthCode Payload
       v
[ApiKeyMiddleware] --(Invalid Key)--> 401 Unauthorized
       |
       v (Valid Key)
[GoogleAuthService] --(Exchange & Validate via Google API)--> Identity Verified
       |
       v
[Domain Service Execution (Score Upsert / Player Registration)]
```

1. **Layer 1: API Key Authorization (`ApiKeyMiddleware`)**
   - All write and delete requests (`POST`, `PUT`, `DELETE`) pass through a custom middleware checking the `X-Api-Key` HTTP header against environment configuration.
   - Unauthorized requests are dropped immediately before payload deserialization or database allocation.
   - Read-only queries (`GET`, `HEAD`) bypass this layer so public web visitors can browse leaderboards without API credentials.

2. **Layer 2: Server-Side Identity Verification (`GoogleAuthService`)**
   - When registering accounts or submitting scores, clients do not send unverified player IDs.
   - Instead, the client sends a short-lived `AuthCode` obtained via Google OAuth login.
   - The backend directly contacts Google servers (`Google.Apis.Auth`) to validate token claims and extract the player's true external identity (`ExternalId`) and display name. This ensures score entries cannot be spoofed by modifying client requests.

---

## Data Model & Query Optimization

The relational database (`SpythereGamesServicesContext`) is structured for data consistency and fast leaderboard queries:

- **`Player` Entity:**
  - Stores unique external identifiers (`ExternalId`), `DisplayName`, and target `Platform` (PC, Web, Mobile).
  - Supports GDPR compliance via a self-service account deletion endpoint (`DELETE /api/players/me`) that permanently removes the user and all associated records.
- **`Game` Entity:**
  - Holds configuration metadata (`Key`, `Name`, `Description`) for titles integrated with the platform.
- **`Score` Entity & Conditional Upserts:**
  - Links `PlayerId` and `GameId` with the numerical `Value` (`long`) and submission timestamp (`SubmittedAt`).
  - **Upsert Strategy:** Instead of inserting every score attempt into an unbounded historical table, `LeaderboardService.SubmitScoreAsync` performs a conditional update. A new record is inserted if the player has no entry for the game; otherwise, the existing record is updated only if the new score (`scoreValue`) is higher than `existingScore.Value`.
  - **Rank Computation:** Leaderboards are computed via relational `Join` queries across `Scores` and `Players` tables, returning deterministic relative ranks (`LeaderboardEntryResponse`).
  - **Bulk Deletions & Read-Only Queries:** Read operations explicitly use `.AsNoTracking()` to avoid unnecessary change-tracking overhead, while account deletions utilize `.ExecuteDeleteAsync()` to purge player data directly in PostgreSQL without loading entity graphs into application memory.

---

## API Endpoints

### Leaderboard & Scores (`ScoresEndpoints`)
| HTTP Method | Path | Access Level | Description |
| :---: | :--- | :---: | :--- |
| `GET` | `/api/games/{gameKey}/scores/top?count={n}` | **Public** | Retrieves the top `N` scores with player display names and platforms. |
| `GET` | `/api/games/{gameKey}/scores/player/{externalId}` | **Public** | Returns a specific player's best score and calculated global rank. |
| `POST` | `/api/games/{gameKey}/scores` | **Protected** *(API Key + AuthCode)* | Validates input bounds and identity via Google OAuth, then upserts a high score. |

### Player & Identity (`PlayersEndpoints`)
| HTTP Method | Path | Access Level | Description |
| :---: | :--- | :---: | :--- |
| `GET` | `/api/players/{id}` | **Public** | Returns public profile info (`Id`, `DisplayName`, `Platform`). |
| `POST` | `/api/players` | **Protected** *(API Key + AuthCode)* | Verifies Google OAuth credentials and idempotently registers a player profile. |
| `DELETE` | `/api/players/me` | **Protected** *(API Key + AuthCode)* | Verifies ownership via Google OAuth and permanently deletes the player profile and scores. |

### Game Registry & Diagnostics (`GamesEndpoints`, `HealthEndpoints`)
| HTTP Method | Path | Access Level | Description |
| :---: | :--- | :---: | :--- |
| `GET` | `/api/games` | **Public** | Lists all registered games active on the platform. |
| `GET` | `/api/games/{key}` | **Public** | Retrieves metadata for a specific game title. |
| `GET` | `/api/health` | **Public** | Runs a lightweight database query (`SELECT 1`) for uptime diagnostics. |

---

## Middleware & Resilience

- **`GlobalExceptionMiddleware`:** Catches unhandled runtime exceptions across the request lifecycle, logging diagnostic details while returning clean `500 Internal Server Error` JSON payloads without stack-trace leakage.
- **Request Cancellation Awareness:** All Minimal API endpoints receive `CancellationToken` parameters and pass them through the service layer to Entity Framework Core and `HttpClient` calls. If a client drops the connection, database and external HTTP queries abort immediately.
- **Automated Database Migrations:** On startup (`app.MigrateDatabase()`), the backend checks database connectivity and applies pending Entity Framework Core migrations to PostgreSQL before accepting requests.
