using GestionTickets.Data;
using GestionTickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Controllers;

// Generado con scaffolding:
//   dotnet aspnet-codegenerator controller -name TicketsController -m Ticket -dc GestionTicketsContext
// Adaptado: listas con texto legible, estado calculado, filtro por estado y reglas de fechas.
public class TicketsController(GestionTicketsContext context) : ControladorBase(context)
{
    /// <summary>Tickets con todo lo necesario para calcular estado, técnico y fecha límite.</summary>
    private IQueryable<Ticket> TicketsCompletos() => _context.Tickets
        .Include(t => t.Categoria)
        .Include(t => t.Usuario)
        .Include(t => t.Seguimientos).ThenInclude(s => s.Tecnico);

    // GET: Tickets?estado=Pendientes
    public async Task<IActionResult> Index(string? estado)
    {
        var tickets = await TicketsCompletos()
            .OrderByDescending(t => t.FechaApertura).ThenByDescending(t => t.TicketId)
            .ToListAsync();

        if (estado == "Pendientes")
            tickets = tickets.Where(t => Catalogos.EstaPendiente(t.Estado)).ToList();
        else if (!string.IsNullOrEmpty(estado))
            tickets = tickets.Where(t => t.Estado == estado).ToList();

        ViewData["Estado"] = estado;
        return View(tickets);
    }

    // GET: Tickets/Details/5  (página con el historial de seguimientos)
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var ticket = await TicketsCompletos().FirstOrDefaultAsync(m => m.TicketId == id);
        if (ticket == null) return NotFound();

        return View(ticket);
    }

    // GET: Tickets/Create
    public async Task<IActionResult> Create()
    {
        var ticket = new Ticket { FechaApertura = Hoy };
        await CargarListasAsync(ticket);
        return View(ticket);
    }

    // POST: Tickets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Asunto,Descripcion,FechaApertura,UsuarioId,CategoriaId")] Ticket ticket)
    {
        await ValidarAsync(ticket, original: null);
        if (ModelState.IsValid)
        {
            _context.Add(ticket);
            if (await GuardarAsync())
            {
                Exito($"Ticket #{ticket.TicketId} «{ticket.Asunto}» registrado.");
                return RedirectToAction(nameof(Index));
            }
        }
        await CargarListasAsync(ticket);
        return View(ticket);
    }

    // GET: Tickets/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        await CargarListasAsync(ticket);
        return View(ticket);
    }

    // POST: Tickets/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("TicketId,Asunto,Descripcion,FechaApertura,UsuarioId,CategoriaId")] Ticket ticket)
    {
        if (id != ticket.TicketId) return NotFound();

        var original = await _context.Tickets.AsNoTracking()
            .Include(t => t.Seguimientos)
            .FirstOrDefaultAsync(t => t.TicketId == id);
        if (original == null) return NotFound();

        await ValidarAsync(ticket, original);
        if (ModelState.IsValid)
        {
            _context.Update(ticket);
            if (await GuardarAsync())
            {
                Exito($"Ticket #{ticket.TicketId} actualizado.");
                return RedirectToAction(nameof(Index));
            }
        }
        await CargarListasAsync(ticket);
        return View(ticket);
    }

    // GET: Tickets/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var ticket = await TicketsCompletos().FirstOrDefaultAsync(m => m.TicketId == id);
        if (ticket == null) return NotFound();

        return View(ticket);
    }

    // POST: Tickets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Seguimientos)
            .FirstOrDefaultAsync(t => t.TicketId == id);
        if (ticket != null)
        {
            // RN-10: la base elimina en cascada los seguimientos del ticket
            var seguimientos = ticket.Seguimientos.Count;
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            Exito(seguimientos > 0
                ? $"Ticket #{id} eliminado junto con {seguimientos} seguimiento(s)."
                : $"Ticket #{id} eliminado.");
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>RN-05 y coherencia de fechas con los seguimientos.</summary>
    private async Task ValidarAsync(Ticket ticket, Ticket? original)
    {
        ticket.Asunto = ticket.Asunto?.Trim()!;

        if (ticket.FechaApertura > Hoy)
            ModelState.AddModelError(nameof(Ticket.FechaApertura), "La fecha de apertura no puede ser futura.");

        var primerSeguimiento = original?.Seguimientos.Min(s => (DateOnly?)s.Fecha);
        if (primerSeguimiento is { } primera && ticket.FechaApertura > primera)
            ModelState.AddModelError(nameof(Ticket.FechaApertura),
                $"La fecha de apertura no puede ser posterior al primer seguimiento ({primera:dd/MM/yyyy}).");

        if (!await _context.Usuarios.AnyAsync(u => u.UsuarioId == ticket.UsuarioId))
            ModelState.AddModelError(nameof(Ticket.UsuarioId), "Seleccione un usuario válido.");

        var categoria = await _context.Categorias.FindAsync(ticket.CategoriaId);
        if (categoria == null)
            ModelState.AddModelError(nameof(Ticket.CategoriaId), "Seleccione una categoría válida.");
        else if (!categoria.Activa && original?.CategoriaId != ticket.CategoriaId)
            ModelState.AddModelError(nameof(Ticket.CategoriaId), $"La categoría «{categoria.Nombre}» está inactiva.");
    }

    /// <summary>Listas desplegables: "cédula - nombre" y solo categorías activas (más la actual).</summary>
    private async Task CargarListasAsync(Ticket ticket)
    {
        var usuarios = await _context.Usuarios.OrderBy(u => u.Nombre).ToListAsync();
        var categorias = await _context.Categorias
            .Where(c => c.Activa || c.CategoriaId == ticket.CategoriaId)
            .OrderBy(c => c.Nombre).ToListAsync();

        ViewData["UsuarioId"] = new SelectList(usuarios, nameof(Usuario.UsuarioId), nameof(Usuario.Texto), ticket.UsuarioId);
        ViewData["CategoriaId"] = new SelectList(categorias, nameof(Categoria.CategoriaId), nameof(Categoria.Texto), ticket.CategoriaId);
    }
}
