using GestionTickets.Data;
using GestionTickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Controllers;

// Generado con scaffolding:
//   dotnet aspnet-codegenerator controller -name UsuariosController -m Usuario -dc GestionTicketsContext
// Adaptado: mensajes en español, cédula y correo sin repetir, protección al eliminar.
public class UsuariosController(GestionTicketsContext context) : ControladorBase(context)
{
    // GET: Usuarios
    public async Task<IActionResult> Index()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Tickets)
            .OrderBy(u => u.Nombre)
            .ToListAsync();
        return View(usuarios);
    }

    // GET: Usuarios/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var usuario = await _context.Usuarios
            .Include(u => u.Tickets).ThenInclude(t => t.Seguimientos)
            .FirstOrDefaultAsync(m => m.UsuarioId == id);
        if (usuario == null) return NotFound();

        return View(usuario);
    }

    // GET: Usuarios/Create
    public IActionResult Create()
    {
        return View(new Usuario());
    }

    // POST: Usuarios/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Cedula,Nombre,Correo,Telefono,Departamento")] Usuario usuario)
    {
        await ValidarAsync(usuario);
        if (ModelState.IsValid)
        {
            _context.Add(usuario);
            if (await GuardarAsync())
            {
                Exito($"Usuario «{usuario.Nombre}» registrado.");
                return RedirectToAction(nameof(Index));
            }
        }
        return View(usuario);
    }

    // GET: Usuarios/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        return View(usuario);
    }

    // POST: Usuarios/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("UsuarioId,Cedula,Nombre,Correo,Telefono,Departamento")] Usuario usuario)
    {
        if (id != usuario.UsuarioId) return NotFound();
        if (!await _context.Usuarios.AnyAsync(u => u.UsuarioId == id)) return NotFound();

        await ValidarAsync(usuario);
        if (ModelState.IsValid)
        {
            _context.Update(usuario);
            if (await GuardarAsync())
            {
                Exito($"Usuario «{usuario.Nombre}» actualizado.");
                return RedirectToAction(nameof(Index));
            }
        }
        return View(usuario);
    }

    // GET: Usuarios/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var usuario = await _context.Usuarios
            .Include(u => u.Tickets)
            .FirstOrDefaultAsync(m => m.UsuarioId == id);
        if (usuario == null) return NotFound();

        return View(usuario);
    }

    // POST: Usuarios/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Tickets)
            .FirstOrDefaultAsync(u => u.UsuarioId == id);
        if (usuario == null) return RedirectToAction(nameof(Index));

        // RN-09: no se elimina un usuario que tiene tickets
        if (usuario.Tickets.Count > 0)
        {
            Error($"No se puede eliminar a «{usuario.Nombre}»: tiene {usuario.Tickets.Count} ticket(s) registrado(s).");
            return RedirectToAction(nameof(Index));
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
        Exito($"Usuario «{usuario.Nombre}» eliminado.");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>RN-01 y RN-02: la cédula y el correo no se repiten.</summary>
    private async Task ValidarAsync(Usuario usuario)
    {
        usuario.Nombre = usuario.Nombre?.Trim()!;
        usuario.Cedula = usuario.Cedula?.Trim()!;
        usuario.Correo = usuario.Correo?.Trim().ToLowerInvariant()!;

        if (!string.IsNullOrEmpty(usuario.Cedula) &&
            await _context.Usuarios.AnyAsync(u => u.UsuarioId != usuario.UsuarioId && u.Cedula == usuario.Cedula))
        {
            ModelState.AddModelError(nameof(Usuario.Cedula), "Ya existe un usuario con esa cédula.");
        }

        if (!string.IsNullOrEmpty(usuario.Correo) &&
            await _context.Usuarios.AnyAsync(u => u.UsuarioId != usuario.UsuarioId && u.Correo == usuario.Correo))
        {
            ModelState.AddModelError(nameof(Usuario.Correo), "Ya existe un usuario con ese correo.");
        }
    }
}
