# Spythere Games Services — Game Developer & Backend Platform

A cross-platform game backend and developer website built with .NET 10 Minimal APIs, PostgreSQL (EF Core 10), Google OAuth 2.0 / Apple Game Center verification, and React 19 / TypeScript / Tailwind CSS v4.

## Features

- **Leaderboards** — players compete for top scores across multiple game titles, with real-time rankings and personal best tracking
- **Score submission** — game clients submit scores through a protected API; identity is verified server-side via Google OAuth or Apple Game Center
- **Player profiles** — automatic registration through Google sign-in or Game Center, with display names and platform info
- **Account deletion** — players can permanently delete their profile and all associated scores (GDPR-friendly)
- **Game registry** — a central catalog of all games active on the platform
- **Developer website** — a React SPA
---

## Technology Stack

### Backend (.NET 10)
- **Framework & API:** .NET 10, ASP.NET Core Minimal APIs, native OpenAPI generation (`Microsoft.AspNetCore.OpenApi`)
- **Database & ORM:** PostgreSQL (`Npgsql`), Entity Framework Core 10 with Code-First Migrations
- **Authentication & Security:** Google OAuth 2.0 server-side token verification (`Google.Apis.Auth`), Apple Game Center identity signature verification (RSA-SHA256), custom HTTP header validation (`X-Api-Key`)
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
[PlayerAuthService] --(Dispatch by Platform)--> Identity Verified
       |-- android --> GoogleAuthService (Google OAuth token exchange)
       |-- ios     --> GameCenterAuthService (Apple signature verification)
       |
       v
[Domain Service Execution (Score Upsert / Player Registration)]
```

1. **Layer 1: API Key Authorization (`ApiKeyMiddleware`)**
   - All write and delete requests (`POST`, `PUT`, `DELETE`) pass through a custom middleware checking the `X-Api-Key` HTTP header against environment configuration.
   - Unauthorized requests are dropped immediately before payload deserialization or database allocation.
   - Read-only queries (`GET`, `HEAD`) bypass this layer so public web visitors can browse leaderboards without API credentials.

2. **Layer 2: Server-Side Identity Verification (`PlayerAuthService`)**
   - When registering accounts or submitting scores, clients do not send unverified player IDs.
   - `PlayerAuthService` dispatches verification to a platform-specific provider based on the request's `Platform` (or the presence of a `GameCenter` payload).
   - **Android (`GoogleAuthService`):** the client sends a short-lived `AuthCode` obtained via Google sign-in. The backend exchanges it with Google servers and extracts the player's true identity via the Play Games API.
   - **iOS (`GameCenterAuthService`):** the client sends the `GameCenter` payload produced by `GKLocalPlayer.fetchItemsForIdentityVerificationSignature()` (`PlayerId`/`teamPlayerID`, `BundleId`, `PublicKeyUrl`, `Signature`, `Salt`, `Timestamp`). The backend downloads Apple's public certificate (HTTPS + host allowlist), checks timestamp freshness, and verifies the RSA-SHA256 signature over `playerId + bundleId + timestamp + salt`.
   - This ensures score entries cannot be spoofed by modifying client requests.
   - Game Center verification is gated by `GameCenter:Enabled` (default `false`) until an Apple Developer account and bundle IDs are configured.

---

## API Endpoints

### Leaderboard & Scores (`ScoresEndpoints`)
| HTTP Method | Path | Access Level | Description |
| :---: | :--- | :---: | :--- |
| `GET` | `/api/games/{gameKey}/scores/top?count={n}` | **Public** | Retrieves the top `N` scores with player display names and platforms. |
| `GET` | `/api/games/{gameKey}/scores/player/{externalId}?platform={p}` | **Public** | Returns a specific player's best score and calculated global rank (`platform` defaults to `android`). |
| `POST` | `/api/games/{gameKey}/scores` | **Protected** *(API Key + platform auth)* | Validates input bounds and identity (Google `AuthCode` or `GameCenter` payload), then upserts a high score. |

### Player & Identity (`PlayersEndpoints`)
| HTTP Method | Path | Access Level | Description |
| :---: | :--- | :---: | :--- |
| `GET` | `/api/players/{id}` | **Public** | Returns public profile info (`Id`, `DisplayName`, `Platform`). |
| `POST` | `/api/players` | **Protected** *(API Key + platform auth)* | Verifies Google OAuth or Game Center credentials and idempotently registers a player profile. |
| `DELETE` | `/api/players/me` | **Protected** *(API Key + platform auth)* | Verifies ownership via Google OAuth or Game Center and permanently deletes the player profile and scores. |

### Game Registry & Diagnostics (`GamesEndpoints`, `HealthEndpoints`)
| HTTP Method | Path | Access Level | Description |
| :---: | :--- | :---: | :--- |
| `GET` | `/api/games` | **Public** | Lists all registered games active on the platform. |
| `GET` | `/api/games/{key}` | **Public** | Retrieves metadata for a specific game title. |
| `GET` | `/api/health` | **Public** | Runs a lightweight database query (`SELECT 1`) for uptime diagnostics. |


