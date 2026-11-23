using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Models;

/// <summary>
/// Tipos de problema que se atienden y su tiempo de respuesta.
/// </summary>
[Table("categorias")]
[Index("Nombre", Name = "uq_categorias_nombre", IsUnique = true)]
public partial class Categoria
{
    [Key]
    [Column("categoria_id")]
    public int CategoriaId { get; set; }

    [Column("nombre")]
    [StringLength(60)]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(250)]
    public string? Descripcion { get; set; }

    [Column("prioridad")]
    [StringLength(10)]
    public string Prioridad { get; set; } = null!;

    [Column("tiempo_respuesta_horas")]
    public int TiempoRespuestaHoras { get; set; }

    [Column("activa")]
    public bool Activa { get; set; }

    [InverseProperty("Categoria")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
