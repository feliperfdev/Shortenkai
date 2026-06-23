# Shortenkai

A URL shortener REST API built with ASP.NET Core 10, PostgreSQL, and Redis.

## Tech Stack

- **.NET 10** / ASP.NET Core
- **Entity Framework Core** with PostgreSQL (Npgsql)
- **Redis** (StackExchange.Redis) for response caching
- **Swagger UI** (Swashbuckle)

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/shortenkai` | Shorten a URL |
| `GET` | `/api/shortenkai/all` | List all shortened URLs |
| `GET` | `/api/shortenkai/{code}` | Get URL metadata by short code |
| `GET` | `/api/shortenkai/url/{code}` | Redirect to the original URL |

### POST `/api/shortenkai`

**Request body:**
```json
{
  "url": "https://example.com/some/very/long/url",
  "slug": "optional-custom-slug"
}
```

**Response `201 Created`:**
```json
{
  "shortCode": "abc123def4",
  "keyCode": "c123def456...",
  "slug": "optional-custom-slug"
}
```

### GET `/api/shortenkai/all`

Returns the metadata (shortCode, keyCode, slug) for every shortened URL.

### GET `/api/shortenkai/{code}`

Returns the metadata (shortCode, keyCode, slug) for the given short code.

### GET `/api/shortenkai/url/{code}`

Performs an HTTP redirect to the original URL associated with the given short code **or** slug. The lookup is cached in Redis (30-day TTL, keyed by slug if provided, otherwise short code) to avoid hitting PostgreSQL on every redirect.

## How It Works

The original URL is SHA-256 hashed into a 64-character hex digest, which is then sliced into two distinct fields:

| Field | Slice | Purpose |
|-------|-------|---------|
| **ShortCode** | `hashHex[0..10]` — first 10 hex chars | The actual lookup key. It's what `GetByCode` and `GetUrlByCode` match against, and what goes in the short URL itself (e.g. `/api/shortenkai/url/abc123def4`). |
| **KeyCode** | `hashHex[9..]` — remaining ~55 hex chars | A longer reference to the same hash, returned to the caller as a canonical/display identifier. It is **not** used for lookups anywhere — metadata only. |

Both slices come from the same digest and overlap by one character (index 9 is the last character of `ShortCode` and also the first character of `KeyCode`) — that's expected, not a bug, since `ShortCode` only needs to be short and unique enough to serve as a cache/DB key, while `KeyCode` exists purely as a longer reference for clients.

On creation, the original URL is also written to Redis under the slug (or `ShortCode`, if no slug was provided) so that subsequent redirects are served from cache instead of hitting PostgreSQL.

## Project Structure

The solution follows Clean Architecture, split into four projects with dependencies pointing inward (`API` → `Infrastructure`/`Application` → `Domain`):

```
Shortenkai/
├── Shortenkai.Domain/                  # No dependencies — core entities
│   └── Models/
│       ├── ShortenkaiUrl.cs            # EF Core entity
│       └── RequestShortenkai.cs        # POST request model
├── Shortenkai.Application/             # Depends on Domain — use-case contracts
│   ├── Common/
│   │   └── FAResult.cs                 # Typed result wrapper (Success/NotFound/Failure)
│   ├── DTOs/
│   │   └── ShortenedUrlDto.cs          # API response shape
│   └── Services/
│       └── IShortenkaiService.cs
├── Shortenkai.Infrastructure/          # Depends on Domain + Application — implementation details
│   ├── Database/
│   │   └── ShortenkaiUrlDb.cs          # DbContext
│   └── Services/
│       ├── ShortenkaiService.cs        # Business logic & hashing
│       └── CacheService.cs             # Generic Redis cache wrapper
└── Shortenkai.API/                     # Depends on Application + Infrastructure — HTTP host
    ├── Controllers/
    │   └── ShortenkaiController.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── appsettings.json
    └── Program.cs                       # App bootstrap & DI setup
```

## Running Locally

### Prerequisites

- .NET 10 SDK
- PostgreSQL instance
- Redis instance (defaults to `localhost:6379`, configured in `Program.cs`)

### Setup

1. Clone the repository
2. Set the database connection string via [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) instead of editing `appsettings.json` directly — this keeps the password out of source control:

```bash
cd Shortenkai.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=Shortenkai;Username=postgres;Password=<your-password>"
```

3. Start the app:

```bash
dotnet run --project Shortenkai.API
```

Swagger UI is served at the root path (`/`) in development mode.

## Inspecting the Redis Cache

If Redis is running in a Docker container, open a `redis-cli` session inside it:

```bash
docker exec -it <container_name_or_id> redis-cli
```

> Not sure of the name? Run `docker ps` and look for the Redis image.

### Listing cached keys

All keys are prefixed with the configured `InstanceName` (`Shortenkai:`):

```bash
KEYS Shortenkai:*
```

### Reading a cached value

`IDistributedCache` (what `CacheService` wraps) does **not** store entries as plain Redis strings — `Microsoft.Extensions.Caching.StackExchangeRedis` stores each entry as a **hash**, with fields `data` (the serialized value) and `absexp` (absolute expiration, as .NET ticks). A plain `GET` on the key returns nothing useful; use `HGETALL` instead:

```bash
HGETALL "Shortenkai:abc123def4"
```

Example output:
```
1) "absexp"
2) "638508096000000000"
3) "data"
4) "\"https://example.com/some/very/long/url\""
```

`data` holds the JSON produced by `CacheService.SetAsync` — since the cached value here is a `string`, it's serialized as a JSON string (hence the surrounding escaped quotes).
