# Spythere Games Services — Game Developer & Backend Platform

A cross-platform game backend and developer website built with .NET 10 Minimal APIs, PostgreSQL (EF Core 10), Google OAuth 2.0 verification, and React 19 / TypeScript / Tailwind CSS v4.

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

## Automated Testing

The project includes a **36-test** end-to-end suite built with **Selenium WebDriver 4.27** and **NUnit 4** targeting the live deployment at `spythere-games.vercel.app`. Tests run in headless Chrome and follow the **Page Object Model** pattern for maintainability.

### Test Suites

| Suite | Tests | What It Covers |
| :--- | :---: | :--- |
| `HealthApiTests` | 10 | Health endpoint status & JSON body, `HEAD` request support, games list schema validation (`Id`/`Key`/`Name`), invalid game key → 404, `POST` without `X-Api-Key` → 401 on scores & players, content-type header, top scores retrieval. |
| `HeroPageTests` | 9 | Logo visibility (opacity animation), `alt` text & `src` attribute, GitHub/YouTube button presence & labels, GitHub link opens correct profile in new tab, hero section full-viewport height, both CTA buttons visible. |
| `LeaderboardTests` | 9 | Section heading text, table column headers order (`#` / `Player` / `Score` / `Platform`), game tabs rendering & default active tab, tab switching updates scores, first row data non-empty, rank ascending order, platform icon `<img>` per row. |
| `NavigationTests` | 8 | Page title non-empty, URL correctness, `<section>` presence, leaderboard heading visible after scroll, `<footer>` in DOM, GitHub link href & `target="_blank"`, mobile viewport (375×667) responsiveness. |

