---
name: minicrm-api
description: 'Use when calling, testing, explaining, or integrating with the local MiniCRM REST API: customers/kunder, CRUD, name search, JWT login, token refresh/validate/revoke, Swagger, database startup, or error simulation. Documents the actual API at http://localhost:5000, request/response formats, and project limitations.'
---

# MiniCRM Local API

## Purpose and Scope

MiniCRM is a training project, not a production CRM. It demonstrates a .NET 10
ASP.NET Core controller-based REST API, EF Core with SQLite, customer CRUD,
JWT authentication, Swagger, CORS, and simulated latency/failures. A plain
HTML/JavaScript UI uses Tailwind CSS and the unauthenticated customer endpoints.
There are no orders, contacts, accounts, user registration, roles, pagination,
or tenant-specific data. GraphQL packages are installed, but no GraphQL endpoint
or schema is configured.

Use only **http://localhost:5000** unless the user explicitly changes the code
and asks for another address. There is no `/api` prefix and no HTTPS listener.
`Program.cs` explicitly binds Kestrel to localhost port 5000; do not infer the
port from `Properties/launchSettings.json`, `ASPNETCORE_URLS`, or `--urls`.

- API root: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger/index.html`
- OpenAPI JSON: `http://localhost:5000/swagger/v1/swagger.json`
- Browser UI: open `UI/index.html` from the repository in a browser.

## Before Making Requests

1. Probe `GET /version` and `GET /test?text=Hello`. Expected responses are plain
   text `1.1.0.0` (current application version, not framework version) and `Hello`.
2. Reuse an already running MiniCRM instance. Do not kill another process or
   start a second copy on port 5000. Confirm an unknown listener is this API.
3. If needed, run `dotnet build` and `dotnet run` from the repository root using
   a .NET 10 SDK. On Windows use `dotnet` from PATH; on Unix the SDK may be at
   `~/.dotnet/dotnet`.
4. Startup without application arguments creates `customers.db` in the working
   directory and seeds 30 customers from `customers.json` if the table is empty.
   If the database exists, startup asks whether to delete it. Answer `n` to keep
   data. NEVER delete/reseed an existing database without explicit permission.
5. For an already initialized database, use `dotnet run -- /nodbinit`. This skips
   both schema creation and seeding, so it is not a first-run command. Do not
   invent other application arguments: any argument currently bypasses normal
   initialization. The special `init` argument does not initialize the database.
6. For destructive tests, prefer an isolated temporary working directory with
   copies of `appsettings.json` and `customers.json`, running the built assembly
   by absolute path. It still listens on port 5000. Do not replace project data.

Configuration comes from `appsettings.json` and normal ASP.NET Core configuration
providers. Never display or copy the JWT signing secret. Keep access tokens in
memory; do not commit them or print them in logs or answers.

## Request and Response Conventions

- Send request objects with `Content-Type: application/json`.
- Use the camelCase JSON field names below. PATCH field names are case-sensitive.
- Customer lists are bare JSON arrays, not `{data: ...}` envelopes.
- GET and POST customer responses are JSON. PUT, PATCH, and DELETE return
  **204 with an empty body**; do not call `response.json()` on those responses.
- System endpoints return plain text. Token validation returns a JSON boolean.
- Check the HTTP status before interpreting a response. Framework binding or
  validation failures may return 400; unsupported content types may return 415.
  Runtime failures may return 500. Do not assume every error body is JSON or has
  the same shape.
- URL-encode search terms as path segments and echo text as a query value.
- The API allows all CORS origins, methods, and headers. Browser clients may call
  it from the local HTML file. CORS is not authentication.
- Only create, change, or delete records within the user's requested scope.
  Never blindly retry POST after a timeout: read back to determine whether the
  creation succeeded first.

## Customer Model

Example response:

```json
{
  "id": 1,
  "name": "Example Customer",
  "age": 28,
  "country": "Denmark",
  "revenue": 1234.5,
  "createdDate": "2026-09-23T12:00:00",
  "isActive": true,
  "tags": ["training", "example"]
}
```

| Field | Type | Default when omitted on POST/PUT |
| --- | --- | --- |
| `id` | 32-bit integer | 0; omit on POST for a database-generated ID |
| `name` | string | empty string |
| `age` | 32-bit integer | 0 |
| `country` | string | empty string |
| `revenue` | double / JSON number | 0 |
| `createdDate` | DateTime / ISO 8601 string | server local `DateTime.Now` |
| `isActive` | boolean | false |
| `tags` | array of strings | empty array |

There are no explicit business validation rules such as age/revenue ranges or
unique names. ASP.NET Core binding and implicit validation still apply; this is
not a guarantee that arbitrary types or null values will be accepted. Send
numbers and booleans as JSON primitives and tags as a string array.

## Customer Endpoints

Choose one base path:

- `/customers`: public, including write/delete operations; no token required.
- `/auth/customers`: same seven operations and same database, requiring
  `Authorization: Bearer <access_token>` on every request. Not a separate dataset.

In the table, `{base}` means either base path above. `{id}` is an integer.

| Method | Path | Success | Main failure |
| --- | --- | --- | --- |
| GET | `{base}` | 200, all customers as an array | 500 |
| GET | `{base}/{id}` | 200, customer object | 404 |
| GET | `{base}/findbyname/{name}` | 200, matching array, possibly empty | 500 |
| POST | `{base}` | 201, created object with assigned ID | 400 / 500 |
| PUT | `{base}/{id}` | 204, no body | 404 / 400 / 500 |
| PATCH | `{base}/{id}` | 204, no body | 404 / 400 / 500 |
| DELETE | `{base}/{id}` | 204, no body | 404 / 500 |

All protected operations can also return 401 for missing, invalid, expired, or
revoked tokens. The customer/system operations can return simulated 500 errors.

### Search

Name search uses SQLite `LIKE '%{name}%'`, not an exact comparison. `%` and `_`
retain their SQL LIKE wildcard meanings. Case behavior follows SQLite LIKE;
do not promise Unicode-aware case-insensitive matching. No pagination or explicit
sort order is implemented; query parameters for them are not supported.

### Create and Replace

POST accepts a customer object; omit `id`. Use the ID from the JSON response.
**Known bug:** both POST routes return `Location: /api/customers/{id}`, which is
not a registered route. Read back using your chosen `{base}/{id}` instead.

PUT overwrites all seven mutable fields, including `createdDate` and `tags`.
Omitted fields become their model defaults, not their previous values. GET first,
preserve fields you do not want to change, then PUT the complete object. The URL
ID selects the record; PUT does not change its ID based on the request body.

### Partial Update

PATCH takes a **plain JSON object**, not an RFC 6902 JSON Patch operation array:

```json
{"name":"Updated name","age":32,"tags":["vip"]}
```

Only supplied recognized fields change. Allowed keys are `name`, `age`,
`country`, `revenue`, `createdDate`, `isActive`, and `tags`. Unknown keys,
including `id` and incorrectly cased names, are silently ignored. `tags` replaces
the entire array; use `[]` to clear it. Null `name`, `country`, or `tags` preserves
the old value. Other null values, wrong types, or a non-object body can throw
and produce 500 rather than a clean validation error. Send valid typed values.

## Authentication and System Endpoints

The following six operations are public (no Bearer header required):

| Method | Path | Request | Success |
| --- | --- | --- | --- |
| GET | `/version` | none | 200, plain-text application version |
| GET | `/test?text=Hello` | optional query `text` | 200, plain-text echo; empty if omitted |
| POST | `/auth/token` | `{"clientId":"testclient","clientSecret":"testsecret"}` | 200, `{"access_token":"..."}` |
| POST | `/auth/refresh` | `{"token":"..."}` | 200, `{"access_token":"..."}` |
| POST | `/auth/validate` | `{"token":"..."}` | 200, JSON `true` or `false` |
| POST | `/auth/revoke` | `{"token":"..."}` | 200, text `Token revoked successfully` |

- `testclient` / `testsecret` are the repository's public training credentials,
  configured in `Clients`. A credentials mismatch returns 401.
- The response key is `access_token`, not `token`. There is no separate refresh
  token, cookie session, registration flow, or OAuth authorization endpoint.
- Tokens are HS256 JWTs with a `client_id` claim. Expiration is configured by
  `JwtSettings:TokenExpirationMinutes` (normally 60); do not hardcode expiry or
  assume exact second-level rejection because validation can allow clock skew.
- Refresh takes the existing access token and deliberately accepts expired
  tokens. It validates the signature and algorithm but does not check issuer or
  audience. Revoked tokens are rejected. Refresh does not revoke the old token.
- Refresh has 401 branches, but malformed/invalid/revoked tokens can throw
  uncaught security exceptions and return 500. Do not assume every rejection
  is a 401. Obtain a fresh token through `/auth/token` when appropriate.
- Validate returns false for caught `SecurityTokenException` failures, including
  revoked tokens; it is not a guarantee that all malformed requests return 200.
- Revoke returns 400 if the token is blank or does not have three dot-separated
  segments. It does not cryptographically verify the token before recording it.
- Revocation is an in-process static set, lost on restart, with no persistence
  or multi-instance coordination. Bearer middleware checks it on protected calls.
- There is no unique token ID; rapidly repeated issuance can produce identical
  tokens. Do not assume refresh always changes the token string.

## PowerShell Walkthrough

Run only against a test instance where creating a temporary customer is allowed.
This uses the authenticated routes, changes only its own new record, cleans it
up, and never displays the access token. On PowerShell, count a returned JSON
array with `$customers.Count`, not `@(Invoke-RestMethod ...).Count`.

```powershell
$ErrorActionPreference = 'Stop'
$baseUrl = 'http://localhost:5000'
$login = @{ clientId = 'testclient'; clientSecret = 'testsecret' } | ConvertTo-Json
$token = (Invoke-RestMethod "$baseUrl/auth/token" -Method Post -ContentType 'application/json' -Body $login).access_token
$headers = @{ Authorization = "Bearer $token" }
$customers = Invoke-RestMethod "$baseUrl/auth/customers" -Headers $headers
Write-Output "Customers: $($customers.Count)"
$search = [Uri]::EscapeDataString('Lars')
$matches = Invoke-RestMethod "$baseUrl/auth/customers/findbyname/$search" -Headers $headers
$created = $null
try {
    $body = @{ name = 'MiniCRM skill test'; age = 28; country = 'Denmark'; revenue = 100.5; isActive = $true; tags = @('skill-test') } | ConvertTo-Json
    $created = Invoke-RestMethod "$baseUrl/auth/customers" -Method Post -Headers $headers -ContentType 'application/json' -Body $body
    $customerUrl = "$baseUrl/auth/customers/$($created.id)"
    Invoke-RestMethod $customerUrl -Method Patch -Headers $headers -ContentType 'application/json' -Body '{"name":"MiniCRM skill updated"}' | Out-Null
    $updated = Invoke-RestMethod $customerUrl -Headers $headers
    if ($updated.name -ne 'MiniCRM skill updated') { throw 'PATCH did not persist' }
    $updated.revenue = 200
    Invoke-RestMethod $customerUrl -Method Put -Headers $headers -ContentType 'application/json' -Body ($updated | ConvertTo-Json) | Out-Null
    $replaced = Invoke-RestMethod $customerUrl -Headers $headers
    if ($replaced.revenue -ne 200) { throw 'PUT did not persist' }
} finally {
    if ($null -ne $created) {
        Invoke-RestMethod "$baseUrl/auth/customers/$($created.id)" -Method Delete -Headers $headers | Out-Null
    }
}
$tokenBody = @{ token = $token } | ConvertTo-Json
$valid = Invoke-RestMethod "$baseUrl/auth/validate" -Method Post -ContentType 'application/json' -Body $tokenBody
if (-not $valid) { throw 'Token validation failed' }
Invoke-RestMethod "$baseUrl/auth/revoke" -Method Post -ContentType 'application/json' -Body $tokenBody | Out-Null
```

For refresh, send the same `$tokenBody` to `/auth/refresh` **before** revocation,
read `.access_token`, and rebuild the Authorization header. Do not store it in a
file. For browser code, use `fetch`, `JSON.stringify`, and the same headers;
check `response.ok`, parsing JSON only for responses with a JSON body.

## Simulation, Verification, and Troubleshooting

- `AppSettings:errorFactor` (0.0-1.0) randomly causes HTTP 500;
  `AppSettings:delay` adds milliseconds before customer/system actions. These
  checks are not implemented in the four token actions. Defaults are zero.
- For deterministic tests, start a dedicated test process with environment
  overrides `AppSettings__errorFactor=0` and `AppSettings__delay=0`. Do not silently
  change a running user's configuration. Stop only test processes you started.
- After relevant changes, build and check both system endpoints, customer list,
  ID lookup, name search, both CRUD route families, login, protected access,
  validate, refresh, revoke, and rejection of a revoked token. Verify Swagger
  JSON and UI load. A fresh seeded database has 30 customers, but an existing
  database may legitimately have another count.
- Test the UI's add/edit/delete modals and quick name editing. Quick edit uses
  PATCH and updates the table without navigation or a page reload.
- Connection refused: check whether MiniCRM is running on localhost:5000.
  Database errors after `/nodbinit`: initialize without that argument, preserving
  existing data. 401: check the header, credentials, expiration, and revocation.
- Swagger documents 20 operations. It currently has no Bearer security scheme
  configured; use an HTTP client with the explicit header for protected calls.
  Treat implementation code as authoritative where Swagger metadata differs.

## Repository Sources

Paths below are relative to the repository root:

- `Program.cs`: port, configuration, middleware, JWT, CORS, Swagger, startup.
- `Controllers/CustomerController.cs` and `Controllers/CustomerAuthController.cs`:
  actual customer routes and update semantics.
- `Controllers/AuthController.cs`, `Services/jwtservice.cs`: token behavior.
- `Controllers/SystemController.cs`: echo and version behavior.
- `Models/Customer.cs`, `Data/CustomerDbContext.cs`: JSON fields and storage.
- `Test/test.http`, `Test/test-auth.http`: existing manual HTTP requests.
- `Test/test-api.html`, `Test/test-api.js`: browser CORS test.
- `UI/index.html`, `UI/app.js`: browser customer management.
- `.github/copilot-instructions.md`: repository-wide development instructions.

When behavior changes, update this skill against these sources rather than
assuming new features or relying only on the README or generated OpenAPI.