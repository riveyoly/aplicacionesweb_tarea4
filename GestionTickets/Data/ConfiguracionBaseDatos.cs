using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GestionTickets.Data;

public static class ConfiguracionBaseDatos
{
    /// <summary>
    /// Arma la cadena de conexión con las variables del archivo .env
    /// (o variables de entorno). La contraseña nunca se escribe en el código.
    /// Si existe ConnectionStrings:GestionTickets, se usa esa.
    /// </summary>
    public static string CadenaConexion(IConfiguration config)
    {
        var explicita = config.GetConnectionString("GestionTickets");
        if (!string.IsNullOrWhiteSpace(explicita)) return explicita;

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = config["DB_HOST"] ?? "localhost",
            Port = int.TryParse(config["DB_PORT"], out var puerto) ? puerto : 5434,
            Database = config["POSTGRES_DB"] ?? "gestion_tickets",
            Username = config["POSTGRES_USER"] ?? "postgres",
            Password = config["POSTGRES_PASSWORD"]
        };
        return builder.ConnectionString;
    }

    /// <summary>
    /// Espera a que PostgreSQL responda antes de atender peticiones.
    /// Las tablas las crean los scripts de la carpeta Database (enfoque Database First).
    /// </summary>
    public static async Task EsperarBaseDatosAsync(this WebApplication app, int intentos = 10)
    {
        for (var intento = 1; intento <= intentos; intento++)
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<GestionTicketsContext>();
            if (await db.Database.CanConnectAsync())
            {
                app.Logger.LogInformation("Conexión con PostgreSQL establecida.");
                return;
            }

            app.Logger.LogWarning("PostgreSQL aún no responde. Reintento {Intento}/{Total} en 3 s...", intento, intentos);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }

        app.Logger.LogError("No se pudo conectar con PostgreSQL. Revise el archivo .env y que la base exista.");
    }
}
