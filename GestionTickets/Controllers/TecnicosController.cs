using GestionTickets.Data;
using GestionTickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Controllers;

// Generado con scaffolding:
//   dotnet aspnet-codegenerator controller -name TecnicosController -m Tecnico -dc GestionTicketsContext
// Adaptado: mensajes en español, correo sin repetir y protección al eliminar.
public class TecnicosController(GestionTicketsContext context) : ControladorBase(context)
{
    // GET: Tecnicos
    public async Task<IActionResult> Index()
    {
        var tecnicos = await _context.Tecnicos
            .Include(t => t.Seguimientos)
            .OrderByDescending(t => t.Activo).ThenBy(t => t.Nombre)
            .ToListAsync();
        return View(tecnicos);
    }

    // GET: Tecnicos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var tecnico = await _context.Tecnicos
            .Include(t => t.Seguimientos).ThenInclude(s => s.Ticket)
            .FirstOrDefaultAsync(m => m.TecnicoId == id);
        if (tecnico == null) return NotFound();

        return View(tecnico);
    }

    // GET: Tecnicos/Create
    public IActionResult Create()
    {
        return View(new Tecnico { Activo = true });
    }

    // POST: Tecnicos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Correo,Telefono,Especialidad,Activo")] Tecnico tecnico)
    {
        await ValidarAsync(tecnico);
        if (ModelState.IsValid)
        {
            _context.Add(tecnico);
            if (await GuardarAsync())
            {
                Exito($"Técnico «{tecnico.Nombre}» registrado.");
                return RedirectToAction(nameof(Index));
            }
        }
        return View(tecnico);
    }

    // GET: Tecnicos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var tecnico = await _context.Tecnicos.FindAsync(id);
        if (tecnico == null) return NotFound();

        return View(tecnico);
    }

    // POST: Tecnicos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("TecnicoId,Nombre,Correo,Telefono,Especialidad,Activo")] Tecnico tecnico)
    {
        if (id != tecnico.TecnicoId) return NotFound();
        if (!await _context.Tecnicos.AnyAsync(t => t.TecnicoId == id)) return NotFound();

        await ValidarAsync(tecnico);
        if (ModelState.IsValid)
        {
            _context.Update(tecnico);
            if (await GuardarAsync())
            {
                Exito($"Técnico «{tecnico.Nombre}» actualizado.");
                return RedirectToAction(nameof(Index));
            }
        }
        return View(tecnico);
    }

    // GET: Tecnicos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var tecnico = await _context.Tecnicos
            .Include(t => t.Seguimientos)
            .FirstOrDefaultAsync(m => m.TecnicoId == id);
        if (tecnico == null) return NotFound();

        return View(tecnico);
    }

    // POST: Tecnicos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tecnico = await _context.Tecnicos
            .Include(t => t.Seguimientos)
            .FirstOrDefaultAsync(t => t.TecnicoId == id);
        if (tecnico == null) return RedirectToAction(nameof(Index));

        // RN-09: no se elimina un técnico con seguimientos (se puede desactivar)
        if (tecnico.Seguimientos.Count > 0)
        {
            Error($"No se puede eliminar a «{tecnico.Nombre}»: registró {tecnico.Seguimientos.Count} seguimiento(s). Puede marcarlo como inactivo.");
            return RedirectToAction(nameof(Index));
        }

        _context.Tecnicos.Remove(tecnico);
        await _context.SaveChangesAsync();
        Exito($"Técnico «{tecnico.Nombre}» eliminado.");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>RN-02: el correo del técnico no se repite.</summary>
    private async Task ValidarAsync(Tecnico tecnico)
    {
        tecnico.Nombre = tecnico.Nombre?.Trim()!;
        tecnico.Correo = tecnico.Correo?.Trim().ToLowerInvariant()!;

        if (!string.IsNullOrEmpty(tecnico.Correo) &&
            await _context.Tecnicos.AnyAsync(t => t.TecnicoId != tecnico.TecnicoId && t.Correo == tecnico.Correo))
        {
            ModelState.AddModelError(nameof(Tecnico.Correo), "Ya existe un técnico con ese correo.");
        }
    }
}
