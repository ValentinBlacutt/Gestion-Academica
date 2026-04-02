using System.Net.Http.Json;
using GestionApiClient.DTOs;

namespace GestionApiClient.Clients;

public class CursosClient
{
    private readonly HttpClient _httpClient;

    public CursosClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CursoDTO>> GetAllAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<CursoDTO>>("api/cursos") ?? new List<CursoDTO>();
    }

    public async Task<CursoDTO?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/cursos/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CursoDTO>();
    }

    public async Task<CursoDTO?> CreateAsync(CursoDTO dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/cursos", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CursoDTO>();
    }

    public async Task<CursoDTO?> UpdateAsync(int id, CursoDTO dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/cursos/{id}", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CursoDTO>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/cursos/{id}");
        return response.IsSuccessStatusCode;
    }
}