using GestionApi.Enums;
using GestionApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GestionApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Curso> Cursos { get; set; }
    public DbSet<Alumno> Alumnos { get; set; }
    public DbSet<Asistencia> Asistencias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Curso — Año + División únicos
        modelBuilder.Entity<Curso>()
            .HasIndex(c => new { c.Anio, c.Division })
            .IsUnique();

        // Alumno — DNI único
        modelBuilder.Entity<Alumno>()
            .HasIndex(a => a.DNI)
            .IsUnique();

        // Alumno — enums como string
        modelBuilder.Entity<Alumno>()
            .Property(a => a.Estado)
            .HasConversion<string>();

        // Asistencia — enums como string
        modelBuilder.Entity<Asistencia>()
            .Property(a => a.Estado)
            .HasConversion<string>();

        // Asistencia — un alumno solo puede tener una asistencia por día
        modelBuilder.Entity<Asistencia>()
            .HasIndex(a => new { a.AlumnoId, a.Fecha })
            .IsUnique();
    }
}