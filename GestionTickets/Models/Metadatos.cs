using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace GestionTickets.Models;

// Las clases de Models (Categoria, Usuario, ...) las genera el scaffolding
// (dotnet ef dbcontext scaffold). Para no perder los cambios al regenerarlas,
// las etiquetas y validaciones se agregan aquí con clases parciales + [ModelMetadataType].

// ---------------- Categoría ----------------
[ModelMetadataType(typeof(CategoriaMetadatos))]
public partial class Categoria
{
    /// <summary>Texto para las listas desplegables.</summary>
    [NotMapped] public string Texto => $"{Nombre} ({Prioridad})";
}

public class CategoriaMetadatos
{
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "Ingrese el nombre.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 60 caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
    public string? Descripcion { get; set; }

    [Display(Name = "Prioridad")]
    [Required(ErrorMessage = "Seleccione la prioridad.")]
    [RegularExpression("^(Baja|Media|Alta|Crítica)$", ErrorMessage = "Prioridad no válida.")]
    public string Prioridad { get; set; } = null!;

    [Display(Name = "Tiempo de respuesta (horas)")]
    [Required(ErrorMessage = "Ingrese el tiempo de respuesta.")]
    [Range(1, 720, ErrorMessage = "El tiempo de respuesta debe estar entre 1 y 720 horas.")]
    public int TiempoRespuestaHoras { get; set; }

    [Display(Name = "Activa")]
    public bool Activa { get; set; }
}

// ---------------- Usuario ----------------
[ModelMetadataType(typeof(UsuarioMetadatos))]
public partial class Usuario
{
    [NotMapped] public string Texto => $"{Cedula} - {Nombre}";
}

public class UsuarioMetadatos
{
    [Display(Name = "Cédula")]
    [Required(ErrorMessage = "Ingrese la cédula.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "La cédula debe tener 10 dígitos.")]
    public string Cedula { get; set; } = null!;

    [Display(Name = "Nombre completo")]
    [Required(ErrorMessage = "Ingrese el nombre.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Correo")]
    [Required(ErrorMessage = "Ingrese el correo.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(120, ErrorMessage = "Máximo 120 caracteres.")]
    public string Correo { get; set; } = null!;

    [Display(Name = "Teléfono")]
    [RegularExpression(@"^\d{7,10}$", ErrorMessage = "El teléfono debe tener de 7 a 10 dígitos.")]
    public string? Telefono { get; set; }

    [Display(Name = "Departamento")]
    [Required(ErrorMessage = "Ingrese el departamento.")]
    [StringLength(60, ErrorMessage = "Máximo 60 caracteres.")]
    public string Departamento { get; set; } = null!;
}

// ---------------- Técnico ----------------
[ModelMetadataType(typeof(TecnicoMetadatos))]
public partial class Tecnico
{
}

public class TecnicoMetadatos
{
    [Display(Name = "Nombre completo")]
    [Required(ErrorMessage = "Ingrese el nombre.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Correo")]
    [Required(ErrorMessage = "Ingrese el correo.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(120, ErrorMessage = "Máximo 120 caracteres.")]
    public string Correo { get; set; } = null!;

    [Display(Name = "Teléfono")]
    [RegularExpression(@"^\d{7,10}$", ErrorMessage = "El teléfono debe tener de 7 a 10 dígitos.")]
    public string? Telefono { get; set; }

    [Display(Name = "Especialidad")]
    [Required(ErrorMessage = "Ingrese la especialidad.")]
    [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
    public string Especialidad { get; set; } = null!;

    [Display(Name = "Activo")]
    public bool Activo { get; set; }
}

// ---------------- Ticket ----------------
[ModelMetadataType(typeof(TicketMetadatos))]
public partial class Ticket
{
    // Datos calculados (no son columnas), como el "saldo" del ejemplo MiBanco.
    // Requieren cargar Seguimientos (con su Técnico) y la Categoría.

    [NotMapped]
    public Seguimiento? UltimoSeguimiento => Seguimientos
        .OrderBy(s => s.Fecha).ThenBy(s => s.SeguimientoId).LastOrDefault();

    [NotMapped, Display(Name = "Estado")]
    public string Estado => UltimoSeguimiento?.Estado ?? Catalogos.EstadoInicial;

    [NotMapped, Display(Name = "Técnico asignado")]
    public string TecnicoAsignado => UltimoSeguimiento?.Tecnico?.Nombre ?? "Sin asignar";

    [NotMapped, Display(Name = "Fecha límite")]
    public DateTime FechaLimite => FechaApertura.ToDateTime(TimeOnly.MinValue)
        .AddHours(Categoria?.TiempoRespuestaHoras ?? 0);

    [NotMapped]
    public bool Vencido => Catalogos.EstaPendiente(Estado) && DateTime.Now > FechaLimite;

    [NotMapped] public string Texto => $"#{TicketId} - {Asunto}";
}

public class TicketMetadatos
{
    [Display(Name = "N.º")]
    public int TicketId { get; set; }

    [Display(Name = "Asunto")]
    [Required(ErrorMessage = "Ingrese el asunto.")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "El asunto debe tener entre 5 y 150 caracteres.")]
    public string Asunto { get; set; } = null!;

    [Display(Name = "Descripción")]
    [Required(ErrorMessage = "Describa el problema.")]
    [StringLength(1000, ErrorMessage = "Máximo 1000 caracteres.")]
    [DataType(DataType.MultilineText)]
    public string Descripcion { get; set; } = null!;

    [Display(Name = "Fecha de apertura")]
    [Required(ErrorMessage = "Ingrese la fecha de apertura.")]
    public DateOnly FechaApertura { get; set; }

    [Display(Name = "Usuario")]
    [Required(ErrorMessage = "Seleccione el usuario.")]
    public int UsuarioId { get; set; }

    [Display(Name = "Categoría")]
    [Required(ErrorMessage = "Seleccione la categoría.")]
    public int CategoriaId { get; set; }

    [Display(Name = "Usuario")]
    public Usuario Usuario { get; set; } = null!;

    [Display(Name = "Categoría")]
    public Categoria Categoria { get; set; } = null!;
}

// ---------------- Seguimiento ----------------
[ModelMetadataType(typeof(SeguimientoMetadatos))]
public partial class Seguimiento
{
}

public class SeguimientoMetadatos
{
    [Display(Name = "Ticket")]
    [Required(ErrorMessage = "Seleccione el ticket.")]
    public int TicketId { get; set; }

    [Display(Name = "Técnico")]
    [Required(ErrorMessage = "Seleccione el técnico.")]
    public int TecnicoId { get; set; }

    [Display(Name = "Fecha")]
    [Required(ErrorMessage = "Ingrese la fecha.")]
    public DateOnly Fecha { get; set; }

    [Display(Name = "Estado")]
    [Required(ErrorMessage = "Seleccione el estado.")]
    [RegularExpression("^(Abierto|En proceso|Resuelto|Cerrado)$", ErrorMessage = "Estado no válido.")]
    public string Estado { get; set; } = null!;

    [Display(Name = "Comentario")]
    [Required(ErrorMessage = "Escriba el comentario.")]
    [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    [DataType(DataType.MultilineText)]
    public string Comentario { get; set; } = null!;

    [Display(Name = "Ticket")]
    public Ticket Ticket { get; set; } = null!;

    [Display(Name = "Técnico")]
    public Tecnico Tecnico { get; set; } = null!;
}
