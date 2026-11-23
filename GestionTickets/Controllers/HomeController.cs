using System.Diagnostics;
using GestionTickets.Data;
using GestionTickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Controllers;

public class HomeController(GestionTicketsContext context) : Controller
{
    // GET: /  → "Bienvenida YOLANDA" con el resumen de cada módulo
    public async Task<IActionResult> Index()
    {
        var tickets = await context.Tickets
            .Include(t => t.Categoria)
            .Include(t => t.Seguimientos)
            .ToListAsync();

        ViewData["Categorias"] = await context.Categorias.CountAsync();
        ViewData["Usuarios"] = await context.Usuarios.CountAsync();
        ViewData["Tecnicos"] = await context.Tecnicos.CountAsync();
        ViewData["Seguimientos"] = await context.Seguimientos.CountAsync();
        ViewData["Tickets"] = tickets.Count;
        ViewData["Pendientes"] = tickets.Count(t => Catalogos.EstaPendiente(t.Estado));
        ViewData["Vencidos"] = tickets.Count(t => t.Vencido);
        return View();
    }

    // Páginas de error 404, 400, etc.
    public IActionResult Estado(int id)
    {
        ViewData["Codigo"] = id;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
