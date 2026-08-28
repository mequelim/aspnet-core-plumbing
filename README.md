# `AspNectCore.Plumbing`

A **reusable ASP.NET Core Class Library** designed to ***eliminate boilerplate infrastructure code*** across APIs. Provides **standardized**, **configurable pipeline components** – middlewares, JSON converters, exception handling, and extension methods – that can be shared across any ASP.NET Core project regardless of domain.

---

## Motivation

In **microservice architectures**, for example, every service tends to repeat the same infrastructure setup: JWT Bearer authentication, authorization policies, CORS configuration, request logging, correlation IDs, exception handling, and JSON serialization options. This duplication leads to *inconsistency*, *maintenance overhead*, and *configuration drift* across services.
 
`AspNetCore.Plumbing` solves this by **centralizing these concerns** into a single **NuGet package**, exposing a **clean**, **fluent API** that each microservice consumes with minimal configuration.

---

## Goals
 
+ **Eliminate duplication** of ASP.NET Core infrastructure setup across microservices;
+ **Enforce consistency** in authentication, authorization, CORS, and middleware behavior;
+ **Remain domain–agnostic** – no business logic, no database drivers, no service–specific dependencies;
+ **Stay configurable** – every behavior can be overridden per microservice via `PlumbingOptions`;
+ **Be composable** – methods are independent and can be used selectively.

---

## What It Does **_NOT_** Do
 
+ Configure databases or ORM contexts (belongs in each microservice or a dedicated package);
+ Register repositories, mappers, or application services (domain–specific concerns);
+ Enforce any specific Identity Provider – works with any OAuth2/OIDC–compliant authority.

---

## Installation

```powershell
dotnet add package AspNetCore.Plumbing
```

> **Target Framework:** `net10.0`
>
> **Dependencies:** `Microsoft.AspNetCore.Authentication.JwtBearer`

---

## Project Structure

```markdown
AspNetCore.Plumbing/
├── Configurations/
│   └── PlumbingOptions.cs
├── Converters/
│   ├── DateOnlyJsonConverter.cs
│   ├── DecimalJsonConverter.cs
│   └── TimeOnlyJsonConverter.cs
├── Exceptions/
│   └── ConflictException.cs
├── Extensions/
│   └── MiddlewareExtensions.cs
├── Middlewares/
│   ├── CorrelationIdMiddleware.cs
│   ├── ExceptionHandlingMiddleware.cs
│   ├── PerformanceMiddleware.cs
│   └── RequestLoggingMiddleware.cs
└── DependencyInjection.cs
```

---

## Configuration

All configurable behavior is centralized in `PlumbingOptions`.

### `PlumbingOptions`

```csharp
namespace AspNetCore.Plumbing.Configurations;

public sealed class PlumbingOptions
{
    /// <summary>
    /// The authority URL of the OAuth2/OIDC provider used for JWT Bearer validation.
    /// Required by ConfigureAuthentication().
    /// Example: "https://identity.myapp.com"
    /// </summary>
    public string AuthorityUrl { get; set; } = string.Empty;

    /// <summary>
    /// The name of the CORS policy registered in the DI container.
    /// Used by AddCorsPolicy() and UseApi().
    /// Default: "DefaultCors"
    /// </summary>
    public string CorsPolicyName { get; set; } = "DefaultCors";

    /// <summary>
    /// The list of allowed origins for the CORS policy.
    /// If empty, the policy falls back to AllowAnyOrigin (permissive mode).
    /// Example: ["https://myapp.com", "https://admin.myapp.com"]
    /// </summary>
    public string[] CorsAllowedOrigins { get; set; } = [];

    /// <summary>
    /// The OAuth2 scope value required by the ApiScope authorization policy.
    /// Required by ConfigureAuthorization().
    /// Example: "my_api"
    /// </summary>
    public string ApiScope { get; set; } = string.Empty;

    /// <summary>
    /// The name of the authorization policy that enforces the ApiScope claim.
    /// Default: "ApiScope"
    /// </summary>
    public string ApiScopePolicyName { get; set; } = "ApiScope";
}
```

---

## API Reference

### `DependencyInjection`

Extension methods on `WebApplicationBuilder` and `WebApplication` that compose the ASP.NET Core pipeline.

#### `ConfigureAuthentication(Action<PlumbingOptions>)`

Registers JWT Bearer authentication using the configured authority URL.

```csharp
builder.ConfigureAuthentication(options =>
{
    options.AuthorityUrl = builder.Configuration["ServicesUrls:IdentityServer"]!;
});
```

**Behavior:**

+ Registers the `Bearer` authentication scheme;
+ Sets `MapInboundClaims = false` to prevent claim type mapping;
+ Disables audience validation (`ValidateAudience = false`);
+ Sets `RoleClaimType` to `"role"`;
+ Logs JWT validation success and failure to `Console`;

**Throws:** `InvalidOperationException` if `AuthorityUrl` is null or whitespace.

---

#### `ConfigureAuthorization(Action<PlumbingOptions>)`

Registers the default authentication policy and a custom scope–based authorization policy.

```csharp
builder.ConfigureAuthorization(options =>
{
    options.ApiScope = "my_api";
    options.ApiScopePolicyName = "ApiScope"; // optional, default: "ApiScope"
});
```

**Behavior:**

+ Sets the default policy to `RequireAuthenticatedUser`.
+ Adds a named policy (`ApiScopePolicyName`) that requires an authenticated user with a `scope` claim matching `ApiScope`;
**Throws:** `InvalidOperationException` if `ApiScope` is null or whitespace.

---

#### `AddCorsPolicy(Action<PlumbingOptions>)`

Registers a CORS policy with configurable allowed origins.

```csharp
// Permissive (development):
builder.AddCorsPolicy(options =>
{
    options.CorsPolicyName = "DefaultCors";
    options.CorsAllowedOrigins = []; // AllowAnyOrigin
});

// Restrictive (production):
builder.AddCorsPolicy(options =>
{
    options.CorsPolicyName = "DefaultCors";
    options.CorsAllowedOrigins = ["https://myapp.com", "https://admin.myapp.com"];
});
```

**Behavior:**

+ If `CorsAllowedOrigins` contains at least one non–empty value, uses `WithOrigins()` with `AllowAnyHeader` and `AllowAnyMethod`.
+ Otherwise, falls back to `AllowAnyOrigin` (permissive mode).

---

#### `UseApi(this WebApplication)`

Configures the middleware pipeline in the correct order.

```csharp
application.UseApi();
```

**Pipeline order:**

1. Request Localization (`pt–BR` default);
2. HTTPS Redirection;
3. Routing;
4. CORS;
5. Authentication;
6. Authorization;
7. Custom Middlewares (via `UseMiddlewares()`);
8. Controller Mapping;

> [!NOTE]
>
> In development, also registers OpenAPI and Scalar API Reference endpoints.

---

### Converters

Custom `System.Text.Json` converters registered automatically via `AddApiServices()`.

| Converter | Description |
|---|---|
| `DateOnlyJsonConverter` | Serializes/deserializes `DateOnly` to/from ISO 8601 date strings. |
| `TimeOnlyJsonConverter` | Serializes/deserializes `TimeOnly` to/from ISO 8601 time strings. |
| `DecimalJsonConverter` | Serializes `decimal` without precision loss in JSON numeric representation. |

---

### Middlewares

Registered automatically by `UseMiddlewares()`, called inside `UseApi()`.

| Middleware | Description |
|---|---|
| `CorrelationIdMiddleware` | Reads or generates a `X–Correlation–Id` header and propagates it through the request context. |
| `ExceptionHandlingMiddleware` | Catches unhandled exceptions and returns standardized `ProblemDetails` responses. |
| `PerformanceMiddleware` | Measures request execution time and logs warnings for slow requests. |
| `RequestLoggingMiddleware` | Logs incoming request details (method, path, status code, duration). |

---

### Exceptions

| Exception | Description |
|---|---|
| `ConflictException` | Thrown when a resource conflict is detected (e.g., duplicate entity). Maps to HTTP `409 Conflict` via `ExceptionHandlingMiddleware`. |

---

## Usage Example

### `Program.cs` (microservice)

```csharp
using AspNetCore.Plumbing;
 
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
    .AddDatabaseServices()        // local – microservice–specific
    .AddApiServices()             // local – repositories, mappings, HttpClients
    .ConfigureAuthentication(options =>
    {
        options.AuthorityUrl = builder.Configuration["ServicesUrls:IdentityServer"]!;
    })
    .ConfigureAuthorization(options =>
    {
        options.ApiScope = builder.Configuration["ApiScope"]!;
    })
    .AddCorsPolicy(options =>
    {
        options.CorsAllowedOrigins = builder.Configuration
            .GetSection("CorsAllowedOrigins")
            .Get<string[]>() ?? [];
    });

WebApplication application = builder.Build();

application.UseApi();

await application.RunAsync();
```

---

## Compatibility

| Target | Version |
|---|---|
| .NET | 10.0+ |
| ASP.NET Core | 10.0+ |
| C# | 14+ (`extension` members) |