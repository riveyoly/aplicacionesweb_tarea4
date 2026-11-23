using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Models;

/// <summary>
/// Avances de la atencion de cada ticket; el ultimo define su estado.
/// </summary>
[Table("seguimientos")]
[Index("TecnicoId", Name = "ix_seguimientos_tecnico_id")]
[Index("TicketId", Name = "ix_seguimientos_ticket_id")]
public partial class Seguimiento
{
    [Key]
    [Column("seguimiento_id")]
    public int SeguimientoId { get; set; }

    [Column("ticket_id")]
    public int TicketId { get; set; }

    [Column("tecnico_id")]
    public int TecnicoId { get; set; }

    [Column("fecha")]
    public DateOnly Fecha { get; set; }

    [Column("estado")]
    [StringLength(12)]
    public string Estado { get; set; } = null!;

    [Column("comentario")]
    [StringLength(500)]
    public string Comentario { get; set; } = null!;

    [ForeignKey("TecnicoId")]
    [InverseProperty("Seguimientos")]
    public virtual Tecnico Tecnico { get; set; } = null!;

    [ForeignKey("TicketId")]
    [InverseProperty("Seguimientos")]
    public virtual Ticket Ticket { get; set; } = null!;
}
