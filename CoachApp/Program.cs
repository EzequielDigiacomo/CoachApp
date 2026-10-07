using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using AccesoDatos;
using AccesoDatos.Repositorios;
using Controladores;
using Controladores.Contratos;
using Controladores.Integraciones;
using Controladores.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddDbContext<CoachDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
builder.Services.AddScoped<IAnotacionRepositorio, AnotacionRepositorio>();
builder.Services.AddScoped<ICuentaGarminRepositorio, CuentaGarminRepositorio>();
builder.Services.AddScoped<PasswordServicio>();
builder.Services.AddScoped<ITokenServicio, TokenServicio>();
builder.Services.AddScoped<SuperAdminSeeder>();

// Los controladores se resuelven a traves de su interfaz, no de la clase concreta.
builder.Services.AddScoped<IAuthController, AuthController>();
builder.Services.AddScoped<IUsuariosController, UsuariosController>();
builder.Services.AddScoped<IAtletasController, AtletasController>();
builder.Services.AddScoped<IEntrenamientosController, EntrenamientosController>();
builder.Services.AddScoped<ITrabajosController, TrabajosController>();
builder.Services.AddScoped<IAnotacionesController, AnotacionesController>();
builder.Services.AddScoped<IIntegracionesController, IntegracionesController>();
builder.Services.AddScoped<IGarminController, GarminController>();
builder.Services.AddScoped<IGarminServicio, GarminServicio>();

// El lector de planillas sale a internet: se le pone un tiempo limite para que
// una hoja lenta no deje colgada la peticion.
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

// El libro bajado se guarda en memoria un rato: pesa varios MB y conviene
// bajarlo una sola vez por sesion de trabajo.
builder.Services.AddMemoryCache();

// ---------------------------------------------------------------------------
// Autenticacion por JWT
// ---------------------------------------------------------------------------
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta configurar 'Jwt:Key' en appsettings.");

const string claveJwtDeEjemplo = "CAMBIAR_ESTA_CLAVE_POR_UNA_SEGURA_DE_AL_MENOS_32_CARACTERES";
if (!builder.Environment.IsDevelopment() && (jwtKey == claveJwtDeEjemplo || jwtKey.Length < 32))
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
        politica.WithOrigins(origenesCors).AllowAnyHeader().AllowAnyMethod()));
}

// ---------------------------------------------------------------------------
// API
// ---------------------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddControllersAsServices();

builder.Services.AddOpenApi();

var app = builder.Build();

// Antes que el resto, para que Render informe el esquema https real.
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// En desarrollo la API se consume por HTTP a traves del proxy de Vite.
// En Render el contenedor tampoco tiene certificado: TLS lo termina el proxy.
if (!app.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(puerto))
{
    app.UseHttpsRedirection();
}

if (origenesCors.Length > 0)
{
    app.UseCors();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () =>
{
    var curl = Environment.GetEnvironmentVariable("GARMIN_CURL");
    return Results.Ok(new
    {
        estado = "ok",
        garminCurl = !string.IsNullOrWhiteSpace(curl) && File.Exists(curl)
    });
});

// Crea el superadmin inicial si todavia no existe ninguna cuenta de ese rol.
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
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex,
            "No se pudo inicializar el superadmin. Verifica que la base de datos este creada (dotnet ef database update).");
    }
}

app.Run();
