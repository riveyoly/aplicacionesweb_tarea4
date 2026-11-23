namespace GestionTickets.Models;

/// <summary>Valores permitidos (los mismos que validan los CHECK de la base de datos).</summary>
public static class Catalogos
{
    public static readonly string[] Prioridades = ["Baja", "Media", "Alta", "Crítica"];

    public static readonly string[] Estados = ["Abierto", "En proceso", "Resuelto", "Cerrado"];

    public const string EstadoInicial = "Abierto";
    public const string EstadoCerrado = "Cerrado";

    /// <summary>Un ticket sigue pendiente mientras no esté resuelto ni cerrado.</summary>
    public static bool EstaPendiente(string estado) => estado is "Abierto" or "En proceso";

    /// <summary>Clase de Bootstrap para la etiqueta del estado.</summary>
    public static string ClaseEstado(string estado) => estado switch
    {
        "Abierto" => "text-bg-dark",
        "En proceso" => "text-bg-primary",
        "Resuelto" => "text-bg-success",
        _ => "text-bg-secondary"
    };

    /// <summary>Clase de Bootstrap para la etiqueta de la prioridad.</summary>
    public static string ClasePrioridad(string prioridad) => prioridad switch
    {
        "Baja" => "text-bg-light border",
        "Media" => "text-bg-secondary",
        "Alta" => "text-bg-warning",
        _ => "text-bg-danger"
    };
}
