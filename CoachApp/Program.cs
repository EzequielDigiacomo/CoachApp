using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AccesoDatos;
using AccesoDatos.Repositorios;
using CoachApp.Middleware;
using Controladores;
using Controladores.Contratos;
using Controladores.Integraciones;
using Controladores.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((contexto, servicios, configuracion) => configuracion
        .ReadFrom.Configuration(contexto.Configuration)
        .ReadFrom.Services(servicios)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "CoachApp")
        .WriteTo.Console());

    // Render inyecta PORT y termina TLS en su proxy. El contenedor solo habla HTTP.
    var puerto = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrWhiteSpace(puerto))
    {
        builder.WebHost.UseUrls($"http://+:{puerto}");
    }

    builder.Services.Configure<ForwardedHeadersOptions>(opciones =>
    {
        opciones.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        opciones.KnownIPNetworks.Clear();
        opciones.KnownProxies.Clear();
    });

    // ---------------------------------------------------------------------------
    // Base de datos (PostgreSQL)
    // ---------------------------------------------------------------------------
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Falta ConnectionStrings:DefaultConnection (User Secrets, env o appsettings.Development.json).");
    }

    builder.Services.AddDbContext<CoachDbContext>(options =>
        options.UseNpgsql(connectionString));

    // Mismo nombre de aplicacion en la PC y en Render, y las claves en Neon,
    // para que un token de Garmin cifrado en un lado se lea en el otro.
    builder.Services.AddDataProtection()
        .SetApplicationName("CoachApp")
        .PersistKeysToDbContext<CoachDbContext>();

    // ---------------------------------------------------------------------------
    // Repositorios y servicios
    // ---------------------------------------------------------------------------
    builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
    builder.Services.AddScoped<IAtletaRepositorio, AtletaRepositorio>();
    builder.Services.AddScoped<IEntrenamientoRepositorio, EntrenamientoRepositorio>();
    builder.Services.AddScoped<ITrabajoRepositorio, TrabajoRepositorio>();
    builder.Services.AddScoped<IControlRepositorio, ControlRepositorio>();
    builder.Services.AddScoped<IEquipoRepositorio, EquipoRepositorio>();
    builder.Services.AddScoped<IEntrenamientoEquipoRepositorio, EntrenamientoEquipoRepositorio>();
    builder.Services.AddScoped<IAnotacionRepositorio, AnotacionRepositorio>();
    builder.Services.AddScoped<IAnotacionAtletaRepositorio, AnotacionAtletaRepositorio>();
    builder.Services.AddScoped<ISensacionRepositorio, SensacionRepositorio>();
    builder.Services.AddScoped<IAvisoRepositorio, AvisoRepositorio>();
    builder.Services.AddScoped<ICuentaGarminRepositorio, CuentaGarminRepositorio>();
    builder.Services.AddScoped<PasswordServicio>();
    builder.Services.AddScoped<ITokenServicio, TokenServicio>();
    builder.Services.AddScoped<SuperAdminSeeder>();

    builder.Services.AddScoped<IAuthController, AuthController>();
    builder.Services.AddScoped<IUsuariosController, UsuariosController>();
    builder.Services.AddScoped<IAtletasController, AtletasController>();
    builder.Services.AddScoped<IEntrenamientosController, EntrenamientosController>();
    builder.Services.AddScoped<ITrabajosController, TrabajosController>();
    builder.Services.AddScoped<IControlesController, ControlesController>();
    builder.Services.AddScoped<IAnotacionesController, AnotacionesController>();
    builder.Services.AddScoped<IIntegracionesController, IntegracionesController>();
    builder.Services.AddScoped<IGarminController, GarminController>();
    builder.Services.AddScoped<IGarminServicio, GarminServicio>();

    builder.Services.AddHttpClient<ILectorHojasGoogle, LectorHojasGoogle>(cliente =>
    {
        cliente.Timeout = TimeSpan.FromMinutes(3);
    });

    builder.Services.AddHttpClient<IGarminConnectCliente, GarminConnectCliente>(cliente =>
    {
        cliente.Timeout = TimeSpan.FromSeconds(40);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.All
    });

    builder.Services.AddMemoryCache();

    // ---------------------------------------------------------------------------
    // Rate limiting (login e integraciones externas)
    // ---------------------------------------------------------------------------
    builder.Services.AddRateLimiter(opciones =>
    {
        opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        opciones.AddPolicy("login", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

        opciones.AddPolicy("integraciones", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "anon",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
    });

    // ---------------------------------------------------------------------------
    // Autenticacion por JWT
    // ---------------------------------------------------------------------------
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("Falta configurar 'Jwt:Key' en appsettings.");

    const string claveJwtDeEjemplo = "CAMBIAR_ESTA_CLAVE_POR_UNA_SEGURA_DE_AL_MENOS_32_CARACTERES";
    if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    {
        throw new InvalidOperationException(
            "Configura Jwt:Key con al menos 32 caracteres (User Secrets o variable Jwt__Key).");
    }

    if (!builder.Environment.IsDevelopment() && jwtKey == claveJwtDeEjemplo)
    {
        throw new InvalidOperationException(
            "En produccion configura Jwt__Key con una clave propia de al menos 32 caracteres.");
    }

    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
                ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    var origenesCors = (builder.Configuration["Cors:Origenes"] ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    if (origenesCors.Length > 0)
    {
        builder.Services.AddCors(opciones => opciones.AddDefaultPolicy(politica =>
            politica.WithOrigins(origenesCors)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders(
                    "X-Total-Count",
                    "X-Page",
                    "X-Page-Size",
                    RequestIdMiddleware.HeaderName)));
    }

    builder.Services.AddScoped<AuditoriaHttpFilter>();

    builder.Services.AddControllers(opciones =>
        {
            opciones.Filters.AddService<AuditoriaHttpFilter>();
        })
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
        .AddControllersAsServices();

    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseForwardedHeaders();
    app.UseMiddleware<RequestIdMiddleware>();
    app.UseSerilogRequestLogging();

    // OpenAPI en Development y Staging (no en Production).
    if (!app.Environment.IsProduction())
    {
        app.MapOpenApi();
    }

    if (!app.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(puerto))
    {
        app.UseHttpsRedirection();
    }

    if (origenesCors.Length > 0)
    {
        app.UseCors();
    }

    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapGet("/health", () => Results.Ok(new { estado = "ok" }));

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<CoachDbContext>();
            await db.Database.MigrateAsync();

            var seeder = scope.ServiceProvider.GetRequiredService<SuperAdminSeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex,
                "No se pudo inicializar la base o el superadmin. Verifica la connection string.");
        }
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La API no pudo arrancar.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Expuesto para WebApplicationFactory en tests.
public partial class Program;
