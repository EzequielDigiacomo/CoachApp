using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Controladores.Integraciones
{
    public sealed record SesionGarmin(string AccessToken, string RefreshToken, string ClientId, DateTime ExpiraUtc);

    public sealed record PerfilGarmin(string DisplayName, string? NombreCompleto);

    public sealed record AmigoGarmin(string DisplayName, string NombreCompleto);

    /// <summary>Actividad vista en el feed de conexiones, con el dueno y el dia.</summary>
    public sealed record ActividadAmigoGarmin(string DisplayName, string Nombre, DateOnly Fecha);

    /// <summary>Un tramo de la actividad: una vuelta o un parcial de Garmin.</summary>
    public sealed record TramoGarmin(double DistanciaMetros, double DuracionSegundos, double? Cadencia);

    /// <summary>Actividad leida de Garmin, todavia sin asociar a un atleta.</summary>
    public sealed record ActividadGarminLeida(
        long ActividadId,
        string Nombre,
        string? Tipo,
        DateTime? InicioLocal,
        double? DistanciaMetros,
        double? DuracionSegundos,
        int? FcPromedio,
        int? FcMaxima,
        double? Cadencia,
        int? Calorias,
        IReadOnlyList<TramoGarmin>? Tramos = null);

    public interface IGarminConnectCliente
    {
        Task<SesionGarmin> IniciarSesionAsync(string usuario, string password, CancellationToken cancelacion);

        Task<SesionGarmin> RefrescarAsync(string refreshToken, string clientId, CancellationToken cancelacion);

        Task<PerfilGarmin> ObtenerPerfilAsync(string accessToken, CancellationToken cancelacion);

        Task<IReadOnlyList<AmigoGarmin>> ObtenerAmigosAsync(string accessToken, CancellationToken cancelacion);

        Task<IReadOnlyList<ActividadAmigoGarmin>> ObtenerFeedEntreAsync(
            string accessToken,
            DateOnly desde,
            DateOnly hasta,
            CancellationToken cancelacion);

        Task<IReadOnlyList<ActividadGarminLeida>> ObtenerActividadesDelDiaAsync(
            string accessToken,
            string displayName,
            DateOnly fecha,
            CancellationToken cancelacion);

        Task<IReadOnlyList<ActividadGarminLeida>> ObtenerActividadesEntreAsync(
            string accessToken,
            string displayName,
            DateOnly desde,
            DateOnly hasta,
            CancellationToken cancelacion);

        Task<ActividadGarminLeida> CompletarAsync(
            string accessToken,
            ActividadGarminLeida actividad,
            CancellationToken cancelacion);
    }

    /// <summary>
    /// Lee Garmin Connect con la sesion del entrenador. No es la API oficial:
    /// es el mismo acceso que usa la app, y Garmin puede cambiarlo.
    /// </summary>
    public class GarminConnectCliente : IGarminConnectCliente
    {
        private const string ClientIdIos = "GCM_IOS_DARK";
        private const string ServiceUrlIos = "https://mobile.integration.garmin.com/gcm/ios";
        private const string GrantTicket =
            "https://connectapi.garmin.com/di-oauth2-service/oauth/grant/service_ticket";

        private static readonly string[] ClientIds =
        [
            "GARMIN_CONNECT_MOBILE_ANDROID_DI_2025Q2",
            "GARMIN_CONNECT_MOBILE_ANDROID_DI_2024Q4",
            "GARMIN_CONNECT_MOBILE_ANDROID_DI",
            "GARMIN_CONNECT_MOBILE_IOS_DI"
        ];

        private readonly HttpClient _http;

        public GarminConnectCliente(HttpClient http)
        {
            _http = http;
        }

        public async Task<SesionGarmin> IniciarSesionAsync(
            string usuario,
            string password,
            CancellationToken cancelacion)
        {
            var url = "https://sso.garmin.com/mobile/api/login?clientId="
                + Uri.EscapeDataString(ClientIdIos)
                + "&locale=en-US&service="
                + Uri.EscapeDataString(ServiceUrlIos);

            using var pedido = new HttpRequestMessage(HttpMethod.Post, url);
            pedido.Headers.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (iPhone; CPU iPhone OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148");
            pedido.Headers.TryAddWithoutValidation("Accept", "application/json");
            pedido.Headers.TryAddWithoutValidation("Origin", "https://sso.garmin.com");
            pedido.Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    username = usuario,
                    password,
                    rememberMe = true,
                    captchaToken = ""
                }),
                Encoding.UTF8,
                "application/json");

            using var respuesta = await _http.SendAsync(pedido, cancelacion);
            var texto = await respuesta.Content.ReadAsStringAsync(cancelacion);

            if (respuesta.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new GarminExcepcion("Garmin limitó los intentos de ingreso. Probá más tarde.", limite: true);
            }

            if (respuesta.StatusCode == HttpStatusCode.Forbidden || !PareceJson(texto))
            {
                throw new GarminExcepcion("Garmin bloqueó el inicio de sesión. Probá de nuevo en unos minutos.");
            }

            using var documento = JsonDocument.Parse(texto);
            var raiz = documento.RootElement;
            var tipo = LeerTipoEstado(raiz);

            if (tipo == "INVALID_USERNAME_PASSWORD")
            {
                throw new GarminExcepcion("El usuario o la contraseña de Garmin no son correctos.");
            }

            if (tipo == "MFA_REQUIRED")
            {
                throw new GarminExcepcion(
                    "Garmin pidió un código de verificación. Esta vinculación no puede completarlo: usá una cuenta sin verificación en dos pasos.");
            }

            if (tipo != "SUCCESSFUL" || !raiz.TryGetProperty("serviceTicketId", out var ticketEl))
            {
                throw new GarminExcepcion("Garmin no aceptó el inicio de sesión.");
            }

            var ticket = ticketEl.GetString();
            if (string.IsNullOrWhiteSpace(ticket))
            {
                throw new GarminExcepcion("Garmin no aceptó el inicio de sesión.");
            }

            return await CanjearTicketAsync(ticket, cancelacion);
        }

        public Task<SesionGarmin> RefrescarAsync(
            string refreshToken,
            string clientId,
            CancellationToken cancelacion) =>
            PedirTokenAsync(
                clientId,
                new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = clientId,
                    ["refresh_token"] = refreshToken
                },
                "No se pudo renovar la sesión de Garmin. Vinculá la cuenta de nuevo.",
                cancelacion);

        public async Task<PerfilGarmin> ObtenerPerfilAsync(string accessToken, CancellationToken cancelacion)
        {
            using var documento = await GetJsonAsync(
                "https://connectapi.garmin.com/userprofile-service/socialProfile",
                accessToken,
                cancelacion);

            var raiz = documento.RootElement;
            var display = LeerTexto(raiz, "displayName");
            if (string.IsNullOrWhiteSpace(display))
            {
                throw new GarminExcepcion("Garmin no devolvió el perfil de la cuenta.");
            }

            return new PerfilGarmin(display, LeerTexto(raiz, "fullName"));
        }

        public async Task<IReadOnlyList<AmigoGarmin>> ObtenerAmigosAsync(
            string accessToken,
            CancellationToken cancelacion)
        {
            var amigos = new List<AmigoGarmin>();
            var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var inicio in new[] { 0, 1 })
            {
                for (var start = inicio; start < 500; start += 100)
                {
                    var url =
                        $"https://connectapi.garmin.com/connection-service/connection/connections?start={start}&limit=100";
                    using var documento = await GetOpcionalAsync(url, accessToken, cancelacion);
                    if (documento is null)
                    {
                        break;
                    }

                    var pagina = LeerAmigos(documento.RootElement);
                    var nuevos = 0;
                    foreach (var amigo in pagina)
                    {
                        if (vistos.Add(amigo.DisplayName))
                        {
                            amigos.Add(amigo);
                            nuevos++;
                        }
                    }

                    if (pagina.Count < 100)
                    {
                        break;
                    }

                    if (nuevos == 0)
                    {
                        break;
                    }
                }

                if (amigos.Count > 0)
                {
                    break;
                }
            }

            return amigos;
        }

        public async Task<IReadOnlyList<ActividadAmigoGarmin>> ObtenerFeedEntreAsync(
            string accessToken,
            DateOnly desde,
            DateOnly hasta,
            CancellationToken cancelacion)
        {
            var actividades = new List<ActividadAmigoGarmin>();
            var vistos = new HashSet<long>();

            for (var start = 0; start < 800; start += 100)
            {
                var url =
                    $"https://connectapi.garmin.com/activitylist-service/activities/subscription/feed?start={start}&limit=100";
                using var documento = await GetOpcionalAsync(url, accessToken, cancelacion);
                if (documento is null)
                {
                    break;
                }

                var pagina = 0;
                var anteriores = 0;
                foreach (var item in Enumerar(documento.RootElement, "activityList", "activities", "items"))
                {
                    pagina++;
                    var id = LeerLong(item, "activityId") ?? 0;
                    if (id > 0 && !vistos.Add(id))
                    {
                        continue;
                    }

                    var inicio = LeerFecha(item, "startTimeLocal") ?? LeerFecha(item, "startTimeGMT");
                    if (inicio is null)
                    {
                        continue;
                    }

                    var dia = DateOnly.FromDateTime(inicio.Value);
                    if (dia < desde)
                    {
                        anteriores++;
                        continue;
                    }

                    if (dia > hasta)
                    {
                        continue;
                    }

                    var dueno = LeerDueno(item);
                    if (dueno is null)
                    {
                        continue;
                    }

                    actividades.Add(new ActividadAmigoGarmin(dueno.Value.Display, dueno.Value.Nombre, dia));
                }

                if (pagina < 100 || anteriores > 0)
                {
                    break;
                }
            }

            return actividades;
        }

        public async Task<IReadOnlyList<ActividadGarminLeida>> ObtenerActividadesDelDiaAsync(
            string accessToken,
            string displayName,
            DateOnly fecha,
            CancellationToken cancelacion)
        {
            var nombre = Uri.EscapeDataString(displayName);
            var dia = fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var urls = new[]
            {
                $"https://connectapi.garmin.com/activitylist-service/activities/{nombre}?start=0&limit=30&startDate={dia}&endDate={dia}",
                $"https://connectapi.garmin.com/activitylist-service/activities/{nombre}?start=0&limit=30"
            };

            foreach (var url in urls)
            {
                using var documento = await GetOpcionalAsync(url, accessToken, cancelacion);
                if (documento is null)
                {
                    continue;
                }

                var delDia = LeerActividades(documento.RootElement)
                    .Where(a => EsDelDia(a, fecha))
                    .Where(a => a.ActividadId > 0)
                    .ToList();

                if (delDia.Count > 0 || url.Contains("startDate", StringComparison.Ordinal))
                {
                    return delDia;
                }
            }

            return [];
        }

        public async Task<IReadOnlyList<ActividadGarminLeida>> ObtenerActividadesEntreAsync(
            string accessToken,
            string displayName,
            DateOnly desde,
            DateOnly hasta,
            CancellationToken cancelacion)
        {
            var nombre = Uri.EscapeDataString(displayName);
            var inicio = desde.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var fin = hasta.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var actividades = new List<ActividadGarminLeida>();
            var vistos = new HashSet<long>();

            for (var start = 0; start < 200; start += 50)
            {
                var url =
                    $"https://connectapi.garmin.com/activitylist-service/activities/{nombre}?start={start}&limit=50&startDate={inicio}&endDate={fin}";

                using var documento = await GetOpcionalAsync(url, accessToken, cancelacion);
                if (documento is null)
                {
                    break;
                }

                var pagina = LeerActividades(documento.RootElement)
                    .Where(a => a.ActividadId > 0 && a.InicioLocal is not null)
                    .ToList();

                var nuevas = 0;
                foreach (var actividad in pagina)
                {
                    var dia = DateOnly.FromDateTime(actividad.InicioLocal!.Value);
                    if (dia < desde || dia > hasta || !vistos.Add(actividad.ActividadId))
                    {
                        continue;
                    }

                    actividades.Add(actividad);
                    nuevas++;
                }

                if (pagina.Count < 50 || nuevas == 0)
                {
                    break;
                }
            }

            return actividades;
        }

        public async Task<ActividadGarminLeida> CompletarAsync(
            string accessToken,
            ActividadGarminLeida actividad,
            CancellationToken cancelacion)
        {
            var url = $"https://connectapi.garmin.com/activity-service/activity/{actividad.ActividadId}";
            using var documento = await GetOpcionalAsync(url, accessToken, cancelacion);
            if (documento is null)
            {
                return actividad;
            }

            var detallada = LeerActividad(documento.RootElement);
            if (detallada is null)
            {
                return actividad;
            }

            var tramos = LeerTramos(documento.RootElement);
            if (tramos.Count == 0)
            {
                var urlSplits = $"https://connectapi.garmin.com/activity-service/activity/{actividad.ActividadId}/splits";
                using var splits = await GetOpcionalAsync(urlSplits, accessToken, cancelacion);
                if (splits is not null)
                {
                    tramos = LeerTramos(splits.RootElement);
                }
            }

            return actividad with
            {
                Nombre = string.IsNullOrWhiteSpace(detallada.Nombre) ? actividad.Nombre : detallada.Nombre,
                Tipo = detallada.Tipo ?? actividad.Tipo,
                InicioLocal = detallada.InicioLocal ?? actividad.InicioLocal,
                DistanciaMetros = detallada.DistanciaMetros ?? actividad.DistanciaMetros,
                DuracionSegundos = detallada.DuracionSegundos ?? actividad.DuracionSegundos,
                FcPromedio = detallada.FcPromedio ?? actividad.FcPromedio,
                FcMaxima = detallada.FcMaxima ?? actividad.FcMaxima,
                Cadencia = detallada.Cadencia ?? actividad.Cadencia,
                Calorias = detallada.Calorias ?? actividad.Calorias,
                Tramos = tramos
            };
        }

        private async Task<SesionGarmin> CanjearTicketAsync(string ticket, CancellationToken cancelacion)
        {
            GarminExcepcion? ultimo = null;

            foreach (var clientId in ClientIds)
            {
                try
                {
                    return await PedirTokenAsync(
                        clientId,
                        new Dictionary<string, string>
                        {
                            ["client_id"] = clientId,
                            ["service_ticket"] = ticket,
                            ["grant_type"] = GrantTicket,
                            ["service_url"] = ServiceUrlIos
                        },
                        "No se pudo abrir la sesión de Garmin.",
                        cancelacion);
                }
                catch (GarminExcepcion ex) when (!ex.SesionVencida && !ex.Limite)
                {
                    ultimo = ex;
                }
            }

            throw ultimo ?? new GarminExcepcion("No se pudo abrir la sesión de Garmin.");
        }

        private async Task<SesionGarmin> PedirTokenAsync(
            string clientId,
            Dictionary<string, string> formulario,
            string mensajeFallo,
            CancellationToken cancelacion)
        {
            using var pedido = new HttpRequestMessage(
                HttpMethod.Post,
                "https://diauth.garmin.com/di-oauth2-service/oauth/token");
            AplicarCabecerasNativas(pedido);
            pedido.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":")));
            pedido.Content = new FormUrlEncodedContent(formulario);

            using var respuesta = await _http.SendAsync(pedido, cancelacion);
            if (respuesta.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new GarminExcepcion("Garmin limitó los intentos. Probá más tarde.", limite: true);
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new GarminExcepcion(mensajeFallo);
            }

            using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(cancelacion));
            var access = LeerTexto(documento.RootElement, "access_token");
            var refresh = LeerTexto(documento.RootElement, "refresh_token");
            if (string.IsNullOrWhiteSpace(access) || string.IsNullOrWhiteSpace(refresh))
            {
                throw new GarminExcepcion(mensajeFallo);
            }

            var clientDelToken = ClientIdDelToken(access) ?? clientId;
            return new SesionGarmin(access, refresh, clientDelToken, ExpiraDelToken(access));
        }

        private async Task<JsonDocument> GetJsonAsync(string url, string accessToken, CancellationToken cancelacion)
        {
            var documento = await GetOpcionalAsync(url, accessToken, cancelacion);
            return documento ?? throw new GarminExcepcion("Garmin no devolvió los datos pedidos.");
        }

        private async Task<JsonDocument?> GetOpcionalAsync(
            string url,
            string accessToken,
            CancellationToken cancelacion)
        {
            using var pedido = new HttpRequestMessage(HttpMethod.Get, url);
            AplicarCabecerasNativas(pedido);
            pedido.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var respuesta = await _http.SendAsync(pedido, cancelacion);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new GarminExcepcion("La sesión de Garmin venció.", sesionVencida: true);
            }

            if (respuesta.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
            {
                return null;
            }

            if (respuesta.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new GarminExcepcion("Garmin limitó las consultas. Probá más tarde.", limite: true);
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new GarminExcepcion("Garmin no dejó leer los datos (" + (int)respuesta.StatusCode + ").");
            }

            var texto = await respuesta.Content.ReadAsStringAsync(cancelacion);
            if (!PareceJson(texto))
            {
                throw new GarminExcepcion("Garmin bloqueó la consulta. Probá de nuevo en unos minutos.");
            }

            return JsonDocument.Parse(texto);
        }

        private static void AplicarCabecerasNativas(HttpRequestMessage pedido)
        {
            pedido.Headers.TryAddWithoutValidation("User-Agent", "GCM-Android-5.23");
            pedido.Headers.TryAddWithoutValidation(
                "X-Garmin-User-Agent",
                "com.garmin.android.apps.connectmobile/5.23; ; Google/sdk_gphone64_arm64/google; Android/33; Dalvik/2.1.0");
            pedido.Headers.TryAddWithoutValidation("X-Garmin-Paired-App-Version", "10861");
            pedido.Headers.TryAddWithoutValidation("X-Garmin-Client-Platform", "Android");
            pedido.Headers.TryAddWithoutValidation("X-App-Ver", "10861");
            pedido.Headers.TryAddWithoutValidation("X-Lang", "en");
            pedido.Headers.TryAddWithoutValidation("X-GCExperience", "GC5");
            pedido.Headers.TryAddWithoutValidation("Accept", "application/json");
        }

        private static List<AmigoGarmin> LeerAmigos(JsonElement raiz)
        {
            var amigos = new List<AmigoGarmin>();
            foreach (var item in Enumerar(raiz, "userConnections", "connections", "connectionList", "items"))
            {
                var amigo = LeerAmigo(item);
                if (amigo is not null)
                {
                    amigos.Add(amigo);
                }
            }

            return amigos;
        }

        /// <summary>
        /// Acepta al amigo aunque Garmin no mande el nombre completo: alcanza
        /// el usuario. Si el dato viene anidado, lo busca un nivel adentro.
        /// </summary>
        private static AmigoGarmin? LeerAmigo(JsonElement item)
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var display = LeerTexto(item, "displayName") ?? LeerTexto(item, "userName");
            var nombre = LeerTexto(item, "fullName")
                ?? LeerTexto(item, "name")
                ?? LeerTexto(item, "userFullName");

            if (!string.IsNullOrWhiteSpace(display))
            {
                return new AmigoGarmin(display, string.IsNullOrWhiteSpace(nombre) ? display : nombre);
            }

            foreach (var propiedad in item.EnumerateObject())
            {
                if (propiedad.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var anidado = LeerAmigo(propiedad.Value);
                if (anidado is not null)
                {
                    return anidado;
                }
            }

            return null;
        }

        private static (string Display, string Nombre)? LeerDueno(JsonElement item)
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var display = LeerTexto(item, "ownerDisplayName")
                ?? LeerTexto(item, "userDisplayName")
                ?? LeerTexto(item, "displayName")
                ?? LeerTexto(item, "userName");

            if (string.IsNullOrWhiteSpace(display))
            {
                foreach (var propiedad in item.EnumerateObject())
                {
                    if (propiedad.Value.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    var anidado = LeerDueno(propiedad.Value);
                    if (anidado is not null)
                    {
                        return anidado;
                    }
                }

                return null;
            }

            var nombre = LeerTexto(item, "ownerFullName")
                ?? LeerTexto(item, "fullName")
                ?? LeerTexto(item, "userFullName")
                ?? display;
            return (display, nombre);
        }

        private static List<ActividadGarminLeida> LeerActividades(JsonElement raiz)
        {
            var actividades = new List<ActividadGarminLeida>();
            foreach (var item in Enumerar(raiz, "activityList", "activities", "items"))
            {
                var actividad = LeerActividad(item);
                if (actividad is not null)
                {
                    actividades.Add(actividad);
                }
            }

            return actividades;
        }

        private static ActividadGarminLeida? LeerActividad(JsonElement item)
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var resumen = item.TryGetProperty("summaryDTO", out var summary) && summary.ValueKind == JsonValueKind.Object
                ? summary
                : item;

            var id = LeerLong(item, "activityId") ?? LeerLong(resumen, "activityId");
            if (id is null or <= 0)
            {
                return null;
            }

            return new ActividadGarminLeida(
                id.Value,
                LeerTexto(item, "activityName") ?? LeerTexto(resumen, "activityName") ?? "Actividad",
                EtiquetaTipo(LeerTipo(item) ?? LeerTipo(resumen)),
                LeerFecha(item, "startTimeLocal") ?? LeerFecha(resumen, "startTimeLocal"),
                LeerDouble(resumen, "distance") ?? LeerDouble(item, "distance"),
                LeerDouble(resumen, "duration") ?? LeerDouble(item, "duration"),
                LeerEntero(resumen, "averageHR") ?? LeerEntero(item, "averageHR"),
                LeerEntero(resumen, "maxHR") ?? LeerEntero(item, "maxHR"),
                LeerCadencia(resumen) ?? LeerCadencia(item),
                LeerEntero(resumen, "calories") ?? LeerEntero(item, "calories"));
        }

        private static List<TramoGarmin> LeerTramos(JsonElement raiz)
        {
            var tramos = new List<TramoGarmin>();

            foreach (var item in Enumerar(raiz, "lapDTOs", "splitSummaries", "splits"))
            {
                if (item.ValueKind != JsonValueKind.Object || EsDescanso(item))
                {
                    continue;
                }

                var distancia = LeerDouble(item, "distance") ?? 0;
                var duracion = LeerDouble(item, "duration", "elapsedDuration", "movingDuration") ?? 0;
                if (distancia < 1 && duracion < 1)
                {
                    continue;
                }

                tramos.Add(new TramoGarmin(distancia, duracion, LeerCadencia(item)));
            }

            return tramos;
        }

        private static bool EsDescanso(JsonElement item)
        {
            var texto = LeerTexto(item, "intensity") ?? LeerTexto(item, "intensityType");
            if (item.TryGetProperty("intensityDTO", out var dto) && dto.ValueKind == JsonValueKind.Object)
            {
                texto ??= LeerTexto(dto, "typeKey") ?? LeerTexto(dto, "type");
            }

            return texto is not null && texto.Contains("REST", StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsDelDia(ActividadGarminLeida actividad, DateOnly fecha) =>
            actividad.InicioLocal is not null && DateOnly.FromDateTime(actividad.InicioLocal.Value) == fecha;

        private static IEnumerable<JsonElement> Enumerar(JsonElement raiz, params string[] listas)
        {
            if (raiz.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in raiz.EnumerateArray())
                {
                    yield return item;
                }

                yield break;
            }

            if (raiz.ValueKind != JsonValueKind.Object)
            {
                yield break;
            }

            foreach (var nombre in listas)
            {
                if (!raiz.TryGetProperty(nombre, out var lista) || lista.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var item in lista.EnumerateArray())
                {
                    yield return item;
                }

                yield break;
            }
        }

        private static string? LeerTipoEstado(JsonElement raiz)
        {
            if (!raiz.TryGetProperty("responseStatus", out var estado))
            {
                return null;
            }

            if (estado.ValueKind == JsonValueKind.String)
            {
                return estado.GetString();
            }

            return estado.ValueKind == JsonValueKind.Object ? LeerTexto(estado, "type") : null;
        }

        private static string? LeerTipo(JsonElement item)
        {
            if (!item.TryGetProperty("activityType", out var tipo) &&
                !item.TryGetProperty("activityTypeDTO", out tipo))
            {
                return null;
            }

            if (tipo.ValueKind == JsonValueKind.String)
            {
                return tipo.GetString();
            }

            return tipo.ValueKind == JsonValueKind.Object
                ? LeerTexto(tipo, "typeKey") ?? LeerTexto(tipo, "type")
                : null;
        }

        private static string? EtiquetaTipo(string? clave) => clave switch
        {
            "rowing" or "indoor_rowing" => "Remo",
            "kayaking" or "boating" or "stand_up_paddleboarding" => "Kayak",
            "running" or "treadmill_running" => "Carrera",
            "cycling" or "indoor_cycling" => "Ciclismo",
            "swimming" or "lap_swimming" or "open_water_swimming" => "Natación",
            "strength_training" => "Fuerza",
            "walking" => "Caminata",
            null or "" => null,
            _ => clave.Replace('_', ' ')
        };

        private static double? LeerCadencia(JsonElement item) =>
            LeerDouble(item,
                "avgStrokeCadence",
                "averageStrokeRate",
                "averageRunCadence",
                "averageBikingCadence",
                "averageSwimCadence",
                "averageSwimmingCadence",
                "averageCadence");

        private static string? LeerTexto(JsonElement item, string nombre)
        {
            if (!item.TryGetProperty(nombre, out var valor) || valor.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var texto = valor.GetString()?.Trim();
            return string.IsNullOrWhiteSpace(texto) ? null : texto;
        }

        private static long? LeerLong(JsonElement item, string nombre)
        {
            if (!item.TryGetProperty(nombre, out var valor))
            {
                return null;
            }

            if (valor.ValueKind == JsonValueKind.Number && valor.TryGetInt64(out var numero))
            {
                return numero;
            }

            return valor.ValueKind == JsonValueKind.String &&
                long.TryParse(valor.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var texto)
                ? texto
                : null;
        }

        private static int? LeerEntero(JsonElement item, params string[] nombres)
        {
            var numero = LeerDouble(item, nombres);
            return numero is null ? null : (int)Math.Round(numero.Value);
        }

        private static double? LeerDouble(JsonElement item, params string[] nombres)
        {
            foreach (var nombre in nombres)
            {
                if (!item.TryGetProperty(nombre, out var valor))
                {
                    continue;
                }

                if (valor.ValueKind == JsonValueKind.Number && valor.TryGetDouble(out var numero))
                {
                    return numero;
                }

                if (valor.ValueKind == JsonValueKind.String &&
                    double.TryParse(valor.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var texto))
                {
                    return texto;
                }
            }

            return null;
        }

        private static DateTime? LeerFecha(JsonElement item, string nombre)
        {
            var texto = LeerTexto(item, nombre);
            if (texto is null || !DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            {
                return null;
            }

            return DateTime.SpecifyKind(fecha, DateTimeKind.Unspecified);
        }

        private static string? ClientIdDelToken(string jwt)
        {
            var partes = jwt.Split('.');
            if (partes.Length < 2)
            {
                return null;
            }

            try
            {
                var json = Encoding.UTF8.GetString(Base64Url(partes[1]));
                using var documento = JsonDocument.Parse(json);
                return LeerTexto(documento.RootElement, "client_id");
            }
            catch (FormatException)
            {
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static DateTime ExpiraDelToken(string jwt)
        {
            var partes = jwt.Split('.');
            if (partes.Length < 2)
            {
                return DateTime.UtcNow.AddHours(1);
            }

            try
            {
                var json = Encoding.UTF8.GetString(Base64Url(partes[1]));
                using var documento = JsonDocument.Parse(json);
                var exp = LeerLong(documento.RootElement, "exp");
                return exp is null
                    ? DateTime.UtcNow.AddHours(1)
                    : DateTimeOffset.FromUnixTimeSeconds(exp.Value).UtcDateTime;
            }
            catch (FormatException)
            {
                return DateTime.UtcNow.AddHours(1);
            }
            catch (JsonException)
            {
                return DateTime.UtcNow.AddHours(1);
            }
        }

        private static byte[] Base64Url(string texto)
        {
            var padded = texto.Replace('-', '+').Replace('_', '/');
            padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
            return Convert.FromBase64String(padded);
        }

        private static bool PareceJson(string texto)
        {
            var recortado = texto.TrimStart();
            return recortado.StartsWith('{') || recortado.StartsWith('[');
        }
    }
}
