using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Models;

/// <summary>
/// Personal de soporte que atiende los tickets.
/// </summary>
[Table("tecnicos")]
[Index("Correo", Name = "uq_tecnicos_correo", IsUnique = true)]
public partial class Tecnico
{
    [Key]
    [Column("tecnico_id")]
    public int TecnicoId { get; set; }

    [Column("nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column("correo")]
    [StringLength(120)]
    public string Correo { get; set; } = null!;

    [Column("telefono")]
    [StringLength(10)]
    public string? Telefono { get; set; }

    [Column("especialidad")]
    [StringLength(80)]
    public string Especialidad { get; set; } = null!;

    [Column("activo")]
    public bool Activo { get; set; }

    [InverseProperty("Tecnico")]
    public virtual ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();
}
