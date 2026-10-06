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

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
# Render asigna PORT (por defecto 10000). Program.cs lo toma al arrancar.
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CoachApp.dll"]
