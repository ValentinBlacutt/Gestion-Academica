using DocumentFormat.OpenXml.Math;
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
    public DbSet<Usuario> Usuarios { get; set; }

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

        // Usuario — Username único
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Usuario — Email único
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Usuario — Rol como string
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Rol)
            .HasConversion<string>();

        modelBuilder.Entity<Usuario>().HasData(new Usuario
        {
            Id = 1,
            Username = "admin",
            Email = "zinclas@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Rol = Rol.Admin,
            EstaActivo = true,
            TokenActivacion = null,
            FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}