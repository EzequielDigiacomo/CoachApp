using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CoachApp.Middleware
{
    /// <summary>Registra quien hizo POST/PUT/PATCH/DELETE (auditoria ligera ISO 27001).</summary>
    public sealed class AuditoriaHttpFilter : IAsyncActionFilter
    {
        private static readonly HashSet<string> Metodos =
            new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH", "DELETE" };

        private readonly ILogger<AuditoriaHttpFilter> _logger;

        public AuditoriaHttpFilter(ILogger<AuditoriaHttpFilter> logger) => _logger = logger;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var ejecutado = await next();
            var http = context.HttpContext;
            var metodo = http.Request.Method;

            if (!Metodos.Contains(metodo))
            {
                return;
            }

            var status = ejecutado.Exception is null
                ? http.Response.StatusCode
                : 500;

            if (status is < 200 or >= 400)
            {
                return;
            }

            var usuario = http.User.FindFirstValue(ClaimTypes.Name)
                ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "anonimo";

            _logger.LogInformation(
                "Auditoria {Metodo} {Ruta} usuario={Usuario} status={Status} requestId={RequestId}",
                metodo,
                http.Request.Path.Value,
                usuario,
                status,
                http.TraceIdentifier);
        }
    }
}
