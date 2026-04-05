using GestionApi.DTOs;

namespace GestionApi.Services;

public interface IAuthService
{
    Task<string?> LoginAsync(string username, string password);
    Task<string?> InvitarUsuarioAsync(string email, string rol);
    Task<bool> ActivarCuentaAsync(string token, string username, string password);
    Task<bool> DesactivarUsuarioAsync(int usuarioId);
    Task<List<UsuarioDTO>> GetAllUsuariosAsync();
    Task<bool> ReactivarUsuarioAsync(int usuarioId);
}