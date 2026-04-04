using GestionApi.Enums;

namespace GestionApi.Models;

public class Usuario
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public Rol Rol { get; set; }
    public bool EstaActivo { get; set; } = false;
    public string? TokenActivacion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}