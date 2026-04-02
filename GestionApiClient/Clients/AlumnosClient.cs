using System.Net.Http.Json;
using GestionApiClient.DTOs;

namespace GestionApiClient.Clients;

public class AlumnosClient
{
    private readonly HttpClient _httpClient;

    public AlumnosClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AlumnoDTO>> GetActivosByCursoAsync(int cursoId)
    {
        return await _httpClient.GetFromJsonAsync<List<AlumnoDTO>>($"api/cursos/{cursoId}/alumnos") ?? new List<AlumnoDTO>();
    }

    public async Task<List<AlumnoDTO>> GetExAlumnosAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<AlumnoDTO>>("api/alumnos/exalumnos") ?? new List<AlumnoDTO>();
    }

    public async Task<List<AlumnoDTO>> GetEgresadosAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<AlumnoDTO>>("api/alumnos/egresados") ?? new List<AlumnoDTO>();
    }

    public async Task<AlumnoDTO?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/alumnos/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AlumnoDTO>();
    }

    public async Task<AlumnoDTO?> CreateAsync(AlumnoDTO dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/alumnos", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AlumnoDTO>();
    }

    public async Task<AlumnoDTO?> UpdateAsync(int id, AlumnoDTO dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/alumnos/{id}", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AlumnoDTO>();
    }

    public async Task<bool> BajaAsync(int id)
    {
        var response = await _httpClient.PatchAsync($"api/alumnos/{id}/baja", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EgresarAsync(int id)
    {
        var response = await _httpClient.PatchAsync($"api/alumnos/{id}/egresar", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> MoverCursoAsync(int alumnoId, int cursoId)
    {
        var response = await _httpClient.PatchAsync($"api/alumnos/{alumnoId}/mover/{cursoId}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<AlumnoDTO>> GetTodosAsync(int pagina = 1, int tamanioPagina = 10)
    {
        return await _httpClient.GetFromJsonAsync<List<AlumnoDTO>>($"api/alumnos?pagina={pagina}&tamanioPagina={tamanioPagina}") ?? new List<AlumnoDTO>();
    }
}