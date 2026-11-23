using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Models;

/// <summary>
/// Solicitudes de soporte abiertas por los usuarios.
/// </summary>
[Table("tickets")]
[Index("CategoriaId", Name = "ix_tickets_categoria_id")]
[Index("UsuarioId", Name = "ix_tickets_usuario_id")]
public partial class Ticket
{
    [Key]
    [Column("ticket_id")]
    public int TicketId { get; set; }

    [Column("asunto")]
    [StringLength(150)]
    public string Asunto { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(1000)]
    public string Descripcion { get; set; } = null!;

    [Column("fecha_apertura")]
    public DateOnly FechaApertura { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("categoria_id")]
    public int CategoriaId { get; set; }

    [ForeignKey("CategoriaId")]
    [InverseProperty("Tickets")]
    public virtual Categoria Categoria { get; set; } = null!;

    [InverseProperty("Ticket")]
    public virtual ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();

    [ForeignKey("UsuarioId")]
    [InverseProperty("Tickets")]
    public virtual Usuario Usuario { get; set; } = null!;
}
