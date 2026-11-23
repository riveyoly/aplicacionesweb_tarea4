using GestionTickets.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GestionTickets.Controllers;

/// <summary>
/// Base común de los controladores generados por scaffolding:
/// mensajes de confirmación y guardado con errores de PostgreSQL en español.
/// </summary>
public abstract class ControladorBase(GestionTicketsContext context) : Controller
{
    protected readonly GestionTicketsContext _context = context;

    /// <summary>Fecha de hoy en la zona horaria del servidor (America/Guayaquil en Docker).</summary>
    protected static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Now);

    protected void Exito(string mensaje) => TempData["Exito"] = mensaje;
    protected void Error(string mensaje) => TempData["Error"] = mensaje;

    /// <summary>
    /// Guarda los cambios. Si PostgreSQL rechaza el dato (duplicado, relación o CHECK),
    /// agrega el error al formulario en lugar de mostrar una pantalla de error.
    /// </summary>
    protected async Task<bool> GuardarAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            ModelState.AddModelError(string.Empty, pg.SqlState switch
            {
                PostgresErrorCodes.UniqueViolation => "Ya existe un registro con esos datos.",
                PostgresErrorCodes.ForeignKeyViolation => "El registro está relacionado con otros datos.",
                PostgresErrorCodes.CheckViolation => "Algún dato no cumple las reglas de la base de datos.",
                _ => "No se pudo guardar en la base de datos."
            });
            _context.ChangeTracker.Clear();
            return false;
        }
    }
}
