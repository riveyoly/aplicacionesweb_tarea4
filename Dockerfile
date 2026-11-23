# ---------- Etapa 1: compilar y publicar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo el .csproj: la restauración de paquetes queda en caché
COPY src/GestionTickets/GestionTickets.csproj src/GestionTickets/
RUN dotnet restore src/GestionTickets/GestionTickets.csproj

COPY src/GestionTickets/ src/GestionTickets/
RUN dotnet publish src/GestionTickets/GestionTickets.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Etapa 2: imagen final (solo runtime) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    DATA_PROTECTION_PATH=/app/keys

# Carpeta para las claves de Data Protection (se monta como volumen)
RUN mkdir -p /app/keys && chown "$APP_UID" /app/keys

COPY --from=build /app/publish .

# Ejecutar sin privilegios de root
USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "GestionTickets.dll"]
