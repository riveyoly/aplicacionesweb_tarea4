using GestionTickets.Data;
using GestionTickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Controllers;

// Generado con scaffolding:
//   dotnet aspnet-codegenerator controller -name CategoriasController -m Categoria -dc GestionTicketsContext
// Adaptado: mensajes en español, nombre sin repetir y protección al eliminar.
public class CategoriasController(GestionTicketsContext context) : ControladorBase(context)
{
    // GET: Categorias
    public async Task<IActionResult> Index()
    {
        var categorias = await _context.Categorias
            .Include(c => c.Tickets)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
        return View(categorias);
    }

    // GET: Categorias/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var categoria = await _context.Categorias
            .Include(c => c.Tickets)
            .FirstOrDefaultAsync(m => m.CategoriaId == id);
        if (categoria == null) return NotFound();

        return View(categoria);
    }

    // GET: Categorias/Create
    public IActionResult Create()
    {
        CargarListas("Media");
        return View(new Categoria { Activa = true, Prioridad = "Media", TiempoRespuestaHoras = 24 });
    }

    // POST: Categorias/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Prioridad,TiempoRespuestaHoras,Activa")] Categoria categoria)
    {
        await ValidarAsync(categoria);
        if (ModelState.IsValid)
        {
            _context.Add(categoria);
            if (await GuardarAsync())
            {
                Exito($"Categoría «{categoria.Nombre}» creada.");
                return RedirectToAction(nameof(Index));
            }
        }
        CargarListas(categoria.Prioridad);
        return View(categoria);
    }

    // GET: Categorias/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();

        CargarListas(categoria.Prioridad);
        return View(categoria);
    }

    // POST: Categorias/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CategoriaId,Nombre,Descripcion,Prioridad,TiempoRespuestaHoras,Activa")] Categoria categoria)
    {
        if (id != categoria.CategoriaId) return NotFound();
        if (!await _context.Categorias.AnyAsync(c => c.CategoriaId == id)) return NotFound();

        await ValidarAsync(categoria);
        if (ModelState.IsValid)
        {
            _context.Update(categoria);
            if (await GuardarAsync())
            {
                Exito($"Categoría «{categoria.Nombre}» actualizada.");
                return RedirectToAction(nameof(Index));
            }
        }
        CargarListas(categoria.Prioridad);
        return View(categoria);
    }

    // GET: Categorias/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var categoria = await _context.Categorias
            .Include(c => c.Tickets)
            .FirstOrDefaultAsync(m => m.CategoriaId == id);
        if (categoria == null) return NotFound();

        return View(categoria);
    }

    // POST: Categorias/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Tickets)
            .FirstOrDefaultAsync(c => c.CategoriaId == id);
        if (categoria == null) return RedirectToAction(nameof(Index));

        // RN-09: no se elimina una categoría con tickets (la base también lo impide)
        if (categoria.Tickets.Count > 0)
        {
            Error($"No se puede eliminar «{categoria.Nombre}»: tiene {categoria.Tickets.Count} ticket(s). Puede marcarla como inactiva.");
            return RedirectToAction(nameof(Index));
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        Exito($"Categoría «{categoria.Nombre}» eliminada.");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>RN-03: el nombre de la categoría no se repite.</summary>
    private async Task ValidarAsync(Categoria categoria)
    {
        var nombre = categoria.Nombre?.Trim();
        if (string.IsNullOrEmpty(nombre)) return;
        categoria.Nombre = nombre;

        var repetido = await _context.Categorias.AnyAsync(c =>
            c.CategoriaId != categoria.CategoriaId && c.Nombre.ToLower() == nombre.ToLower());
        if (repetido)
        {
            ModelState.AddModelError(nameof(Categoria.Nombre), "Ya existe una categoría con ese nombre.");
        }
    }

    private void CargarListas(string? prioridad) =>
        ViewData["Prioridad"] = new SelectList(Catalogos.Prioridades, prioridad);
}
