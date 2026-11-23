using System;
using System.Collections.Generic;
using GestionTickets.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Data;

public partial class GestionTicketsContext : DbContext
{
    public GestionTicketsContext(DbContextOptions<GestionTicketsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Seguimiento> Seguimientos { get; set; }

    public virtual DbSet<Tecnico> Tecnicos { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("pk_categorias");

            entity.ToTable("categorias", tb => tb.HasComment("Tipos de problema que se atienden y su tiempo de respuesta."));
        });

        modelBuilder.Entity<Seguimiento>(entity =>
        {
            entity.HasKey(e => e.SeguimientoId).HasName("pk_seguimientos");

            entity.ToTable("seguimientos", tb => tb.HasComment("Avances de la atencion de cada ticket; el ultimo define su estado."));

            entity.HasOne(d => d.Tecnico).WithMany(p => p.Seguimientos)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_seguimientos_tecnicos");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Seguimientos).HasConstraintName("fk_seguimientos_tickets");
        });

        modelBuilder.Entity<Tecnico>(entity =>
        {
            entity.HasKey(e => e.TecnicoId).HasName("pk_tecnicos");

            entity.ToTable("tecnicos", tb => tb.HasComment("Personal de soporte que atiende los tickets."));
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("pk_tickets");

            entity.ToTable("tickets", tb => tb.HasComment("Solicitudes de soporte abiertas por los usuarios."));

            entity.HasOne(d => d.Categoria).WithMany(p => p.Tickets)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_tickets_categorias");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Tickets)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_tickets_usuarios");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("pk_usuarios");

            entity.ToTable("usuarios", tb => tb.HasComment("Personas de la empresa que reportan tickets."));
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
