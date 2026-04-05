using GestionApiClient.DTOs;
using System.Net.Http.Json;

namespace GestionApiClient.Clients;

public class AuthClient
{
    private readonly HttpClient _httpClient;

    public AuthClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginRequestDTO
        {
            Username = username,
            Password = password
        });

        if (!response.IsSuccessStatusCode) return null;
        var resultado = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return resultado?.Token;
    }

    public async Task<string?> InvitarUsuarioAsync(string email, string rol)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/invitar", new InvitarUsuarioDTO
        {
            Email = email,
            Rol = rol
        });
        if (!response.IsSuccessStatusCode) return null;
        var resultado = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return resultado?.Token;
    }

    public async Task<bool> ActivarCuentaAsync(string token, string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/activar", new ActivarCuentaDTO
        {
            Token = token,
            Username = username,
            Password = password
        });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DesactivarUsuarioAsync(int id)
    {
        var response = await _httpClient.PatchAsync($"api/auth/usuarios/{id}/desactivar", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<UsuarioDTO>> GetAllUsuariosAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<UsuarioDTO>>("api/auth/usuarios") ?? new List<UsuarioDTO>();
    }

    public async Task<bool> ReactivarUsuarioAsync(int id)
    {
        var response = await _httpClient.PatchAsync($"api/auth/usuarios/{id}/reactivar", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CambiarPasswordAsync(string passwordActual, string passwordNueva)
    {
        var response = await _httpClient.PatchAsJsonAsync("api/auth/cambiar-password", new CambiarPasswordDTO
        {
            PasswordActual = passwordActual,
            PasswordNueva = passwordNueva
        });
        return response.IsSuccessStatusCode;
    }

    public async Task<string?> ResetearPasswordAsync(int id)
    {
        var response = await _httpClient.PatchAsync($"api/auth/usuarios/{id}/resetear-password", null);
        if (!response.IsSuccessStatusCode) return null;
        var resultado = await response.Content.ReadFromJsonAsync<PasswordTemporalResponse>();
        return resultado?.PasswordTemporal;
    }

}

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
}

public class PasswordTemporalResponse
{
    public string PasswordTemporal { get; set; } = string.Empty;
}