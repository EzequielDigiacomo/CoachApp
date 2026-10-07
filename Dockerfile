# Imagen de la API para Render. El contexto de build es la raiz del repo.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Entidades/Entidades.csproj Entidades/
COPY AccesoDatos/AccesoDatos.csproj AccesoDatos/
COPY Controladores/Controladores.csproj Controladores/
COPY CoachApp/CoachApp.csproj CoachApp/
RUN dotnet restore CoachApp/CoachApp.csproj

COPY . .
RUN dotnet publish CoachApp/CoachApp.csproj -c Release -o /app/publish /p:UseAppHost=false

# Se baja aca, en la imagen de compilacion, que si corre como root.
# La imagen final de ASP.NET no: un apt-get ahi tumba el build de Render
# y queda publicada la version anterior, sin este cliente.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl \
    && curl -fsSL -o /tmp/curl-imp.tar.gz \
        https://github.com/lexiforest/curl-impersonate/releases/download/v1.5.6/curl-impersonate-v1.5.6.x86_64-linux-gnu.tar.gz \
    && mkdir -p /opt/curl-impersonate \
    && tar -xzf /tmp/curl-imp.tar.gz -C /opt/curl-impersonate \
    && chmod 755 /opt/curl-impersonate/curl-impersonate \
    && rm -rf /var/lib/apt/lists/* /tmp/curl-imp.tar.gz

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Garmin bloquea el TLS de .NET cuando la consulta sale de Render.
# Este binario se presenta como Chrome y solo se usa si Garmin responde 403.
COPY --from=build /opt/curl-impersonate/curl-impersonate /opt/curl-impersonate/curl-impersonate
ENV GARMIN_CURL=/opt/curl-impersonate/curl-impersonate
ENV ASPNETCORE_ENVIRONMENT=Production
# Render asigna PORT (por defecto 10000). Program.cs lo toma al arrancar.
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CoachApp.dll"]
