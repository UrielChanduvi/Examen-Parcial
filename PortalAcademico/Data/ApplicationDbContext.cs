using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Curso>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Curso_Creditos_Positive", "\"Creditos\" > 0");
                table.HasCheckConstraint("CK_Curso_CupoMaximo_Positive", "\"CupoMaximo\" > 0");
                table.HasCheckConstraint("CK_Curso_Horario", "\"HorarioInicio\" < \"HorarioFin\"");
            });

            entity.Property(c => c.Codigo)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(c => c.Creditos)
                .IsRequired();

            entity.Property(c => c.CupoMaximo)
                .IsRequired();

            entity.Property(c => c.HorarioInicio)
                .HasConversion(
                    value => value.ToString("HH:mm"),
                    value => TimeOnly.Parse(value));

            entity.Property(c => c.HorarioFin)
                .HasConversion(
                    value => value.ToString("HH:mm"),
                    value => TimeOnly.Parse(value));

            entity.HasIndex(c => c.Codigo)
                .IsUnique();
        });

        builder.Entity<Matricula>(entity =>
        {
            entity.Property(m => m.UsuarioId)
                .IsRequired();

            entity.Property(m => m.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(m => m.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasIndex(m => new { m.CursoId, m.UsuarioId })
                .IsUnique();

            entity.HasOne(m => m.Curso)
                .WithMany(c => c.Matriculas)
                .HasForeignKey(m => m.CursoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Usuario)
                .WithMany(u => u.Matriculas)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
