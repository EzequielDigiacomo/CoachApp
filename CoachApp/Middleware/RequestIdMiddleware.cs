namespace CoachApp.Middleware
{
    /// <summary>Propaga o genera X-Request-Id para correlacionar logs de un request.</summary>
    public sealed class RequestIdMiddleware
    {
        public const string HeaderName = "X-Request-Id";
        private readonly RequestDelegate _next;

        public RequestIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = context.Request.Headers[HeaderName].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(requestId))
            {
                requestId = Guid.NewGuid().ToString("N");
            }

            context.TraceIdentifier = requestId;
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = requestId;
                return Task.CompletedTask;
            });

            using (Serilog.Context.LogContext.PushProperty("RequestId", requestId))
            {
                await _next(context);
            }
        }
    }
}
