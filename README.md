# CoachApp (API)

API .NET 10 para coaching de canotaje: entrenamientos, trabajos, controles, avisos, Garmin y cuentas.

## Estructura

| Proyecto | Rol |
|----------|-----|
| `CoachApp` | Host ASP.NET (Program, middleware, deploy) |
| `Controladores` | Endpoints + integraciones |
| `AccesoDatos` | EF Core + PostgreSQL + repos |
| `Entidades` | Modelo y DTOs |
| `CoachApp.Tests` | xUnit |

## Requisitos

- .NET 10 SDK
- PostgreSQL (local o Neon)

## Configuración local

No versionamos secretos de producción. En Development usá `appsettings.Development.json` o User Secrets:

```powershell
cd CoachApp
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=coachapp;Username=postgres;Password=TU_PASSWORD"
dotnet user-secrets set "Jwt:Key" "UNA_CLAVE_DE_AL_MENOS_32_CARACTERES!!"
dotnet user-secrets set "SuperAdmin:NombreUsuario" "superadmin"
dotnet user-secrets set "SuperAdmin:Password" "TuPasswordSegura!"
dotnet user-secrets set "SuperAdmin:Email" "superadmin@coachapp.local"
```

Producción (Render): variables `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Cors__Origenes`, `SuperAdmin__*`.

## Arranque

La API la corre el usuario desde **Visual Studio** (puerto tipico `5203`). No levantar `dotnet run` en background: bloquea los DLL de `bin`.

Para verificar compilación sin tocar `bin`:

```powershell
dotnet build CoachApp/CoachApp.csproj -o "$env:TEMP/coachapp-build-check"
dotnet test CoachApp.Tests/CoachApp.Tests.csproj -o "$env:TEMP/coachapp-test-check"
```

Health: `GET /health` → `{ "estado": "ok" }`  
OpenAPI: `/openapi/v1.json` en Development/Staging (no Production).

## Roles

- **SuperAdmin** — usuarios del sistema
- **Entrenador** — carga de sesiones, controles, cuentas atleta
- **Atleta** — ve y anota lo propio

## Seguridad (resumen)

- JWT Bearer, rate limit en `/api/auth/login` e integraciones/Garmin
- Passwords con `PasswordHasher`
- Integraciones Google Sheets solo staff
- Headers de correlación: `X-Request-Id`
- Listados con `X-Total-Count` / `X-Page` / `X-Page-Size` (entrenamientos)

## Deploy

Ver `render.yaml` + `Dockerfile`. TLS termina en el proxy de Render.
