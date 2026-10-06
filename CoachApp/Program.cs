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
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Base de datos (PostgreSQL)
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<CoachDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// ---------------------------------------------------------------------------
// API
// ---------------------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddControllersAsServices();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// En desarrollo la API se consume por HTTP a traves del proxy de Vite:
// redirigir a HTTPS manda al navegador a otro origen (y a un certificado
// de desarrollo) y las llamadas fallan. Solo redirigimos fuera de desarrollo.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Crea el superadmin inicial si todavia no existe ninguna cuenta de ese rol.
using (var scope = app.Services.CreateScope())
{
    try
    {
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
