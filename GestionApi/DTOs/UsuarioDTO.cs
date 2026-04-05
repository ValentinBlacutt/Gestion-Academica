namespace GestionApi.DTOs;

public class UsuarioDTO
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool EstaActivo { get; set; }
}