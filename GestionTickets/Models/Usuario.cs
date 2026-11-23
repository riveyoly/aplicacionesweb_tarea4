using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Models;

/// <summary>
/// Personas de la empresa que reportan tickets.
/// </summary>
[Table("usuarios")]
[Index("Cedula", Name = "uq_usuarios_cedula", IsUnique = true)]
[Index("Correo", Name = "uq_usuarios_correo", IsUnique = true)]
public partial class Usuario
{
    [Key]
    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("cedula")]
    [StringLength(10)]
    public string Cedula { get; set; } = null!;

    [Column("nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column("correo")]
    [StringLength(120)]
    public string Correo { get; set; } = null!;

    [Column("telefono")]
    [StringLength(10)]
    public string? Telefono { get; set; }

    [Column("departamento")]
    [StringLength(60)]
    public string Departamento { get; set; } = null!;

    [InverseProperty("Usuario")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
