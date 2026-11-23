using GestionTickets.Data;
using GestionTickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Controllers;

// Generado con scaffolding:
//   dotnet aspnet-codegenerator controller -name SeguimientosController -m Seguimiento -dc GestionTicketsContext
// Adaptado: técnicos activos, fechas válidas y tickets cerrados sin nuevos seguimientos.
public class SeguimientosController(GestionTicketsContext context) : ControladorBase(context)
{
    // GET: Seguimientos
    public async Task<IActionResult> Index()
    {
        var seguimientos = await _context.Seguimientos
            .Include(s => s.Tecnico)
            .Include(s => s.Ticket)
            .OrderByDescending(s => s.Fecha).ThenByDescending(s => s.SeguimientoId)
            .ToListAsync();
        return View(seguimientos);
    }

    // GET: Seguimientos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var seguimiento = await _context.Seguimientos
            .Include(s => s.Tecnico)
            .Include(s => s.Ticket).ThenInclude(t => t.Usuario)
            .FirstOrDefaultAsync(m => m.SeguimientoId == id);
        if (seguimiento == null) return NotFound();

        return View(seguimiento);
    }

    // GET: Seguimientos/Create?ticketId=3&volver=ticket
    public async Task<IActionResult> Create(int? ticketId, string? volver)
    {
        var seguimiento = new Seguimiento { TicketId = ticketId ?? 0, Fecha = Hoy, Estado = "En proceso" };
        ViewData["Volver"] = volver;
        await CargarListasAsync(seguimiento, original: null);
        return View(seguimiento);
    }

    // POST: Seguimientos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TicketId,TecnicoId,Fecha,Estado,Comentario")] Seguimiento seguimiento, string? volver)
    {
        await ValidarAsync(seguimiento, original: null);
        if (ModelState.IsValid)
        {
            _context.Add(seguimiento);
            if (await GuardarAsync())
            {
                Exito($"Seguimiento registrado en el ticket #{seguimiento.TicketId} ({seguimiento.Estado}).");
                return volver == "ticket"
                    ? RedirectToAction("Details", "Tickets", new { id = seguimiento.TicketId })
                    : RedirectToAction(nameof(Index));
            }
        }
        ViewData["Volver"] = volver;
        await CargarListasAsync(seguimiento, original: null);
        return View(seguimiento);
    }

    // GET: Seguimientos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var seguimiento = await _context.Seguimientos.FindAsync(id);
        if (seguimiento == null) return NotFound();

        await CargarListasAsync(seguimiento, seguimiento);
        return View(seguimiento);
    }

    // POST: Seguimientos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("SeguimientoId,TicketId,TecnicoId,Fecha,Estado,Comentario")] Seguimiento seguimiento)
    {
        if (id != seguimiento.SeguimientoId) return NotFound();

        var original = await _context.Seguimientos.AsNoTracking().FirstOrDefaultAsync(s => s.SeguimientoId == id);
        if (original == null) return NotFound();

        await ValidarAsync(seguimiento, original);
        if (ModelState.IsValid)
        {
            _context.Update(seguimiento);
            if (await GuardarAsync())
            {
                Exito($"Seguimiento del ticket #{seguimiento.TicketId} actualizado.");
                return RedirectToAction(nameof(Index));
            }
        }
        await CargarListasAsync(seguimiento, original);
        return View(seguimiento);
    }

    // GET: Seguimientos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var seguimiento = await _context.Seguimientos
            .Include(s => s.Tecnico)
            .Include(s => s.Ticket)
            .FirstOrDefaultAsync(m => m.SeguimientoId == id);
        if (seguimiento == null) return NotFound();

        return View(seguimiento);
    }

    // POST: Seguimientos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var seguimiento = await _context.Seguimientos.FindAsync(id);
        if (seguimiento != null)
        {
            _context.Seguimientos.Remove(seguimiento);
            await _context.SaveChangesAsync();
            Exito($"Seguimiento eliminado del ticket #{seguimiento.TicketId}.");
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>RN-06, RN-07 y RN-08.</summary>
    private async Task ValidarAsync(Seguimiento seguimiento, Seguimiento? original)
    {
        seguimiento.Comentario = seguimiento.Comentario?.Trim()!;

        var ticket = await _context.Tickets
            .Include(t => t.Seguimientos)
            .FirstOrDefaultAsync(t => t.TicketId == seguimiento.TicketId);
        if (ticket == null)
        {
            ModelState.AddModelError(nameof(Seguimiento.TicketId), "Seleccione un ticket válido.");
        }
        else
        {
            if (seguimiento.Fecha < ticket.FechaApertura)
                ModelState.AddModelError(nameof(Seguimiento.Fecha),
                    $"La fecha no puede ser anterior a la apertura del ticket ({ticket.FechaApertura:dd/MM/yyyy}).");

            // RN-08: un ticket cerrado no admite nuevos seguimientos
            if (original == null && ticket.Estado == Catalogos.EstadoCerrado)
                ModelState.AddModelError(nameof(Seguimiento.TicketId),
                    $"El ticket #{ticket.TicketId} está cerrado; no admite más seguimientos.");
        }

        if (seguimiento.Fecha > Hoy)
            ModelState.AddModelError(nameof(Seguimiento.Fecha), "La fecha no puede ser futura.");

        // RN-06: solo técnicos activos (al editar se respeta el técnico que ya tenía)
        var tecnico = await _context.Tecnicos.FindAsync(seguimiento.TecnicoId);
        if (tecnico == null)
            ModelState.AddModelError(nameof(Seguimiento.TecnicoId), "Seleccione un técnico válido.");
        else if (!tecnico.Activo && original?.TecnicoId != seguimiento.TecnicoId)
            ModelState.AddModelError(nameof(Seguimiento.TecnicoId), $"El técnico «{tecnico.Nombre}» está inactivo.");
    }

    /// <summary>Al crear solo se ofrecen tickets no cerrados y técnicos activos.</summary>
    private async Task CargarListasAsync(Seguimiento seguimiento, Seguimiento? original)
    {
        var tickets = await _context.Tickets
            .Include(t => t.Seguimientos)
            .OrderByDescending(t => t.TicketId)
            .ToListAsync();
        if (original == null)
            tickets = tickets.Where(t => t.Estado != Catalogos.EstadoCerrado || t.TicketId == seguimiento.TicketId).ToList();

        var tecnicos = await _context.Tecnicos
            .Where(t => t.Activo || t.TecnicoId == seguimiento.TecnicoId)
            .OrderBy(t => t.Nombre).ToListAsync();

        ViewData["TicketId"] = new SelectList(tickets, nameof(Ticket.TicketId), nameof(Ticket.Texto), seguimiento.TicketId);
        ViewData["TecnicoId"] = new SelectList(tecnicos, nameof(Tecnico.TecnicoId), nameof(Tecnico.Nombre), seguimiento.TecnicoId);
        ViewData["Estado"] = new SelectList(Catalogos.Estados, seguimiento.Estado);
    }
}
