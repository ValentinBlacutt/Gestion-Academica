using System.Net.Http.Json;
using GestionApiClient.DTOs;

namespace GestionApiClient.Clients;

public class AsistenciasClient
{
    private readonly HttpClient _httpClient;

    public AsistenciasClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AsistenciaDTO>> GetByCursoHoyAsync(int cursoId)
    {
        return await _httpClient.GetFromJsonAsync<List<AsistenciaDTO>>($"api/asistencias/curso/{cursoId}/hoy") ?? new List<AsistenciaDTO>();
    }

    public async Task<Dictionary<string, int>> GetResumenAlumnoAsync(int alumnoId)
    {
        return await _httpClient.GetFromJsonAsync<Dictionary<string, int>>($"api/asistencias/alumno/{alumnoId}/resumen") ?? new Dictionary<string, int>();
    }

    public async Task<bool> BulkCreateAsync(List<AsistenciaDTO> dtos)
    {
        var response = await _httpClient.PostAsJsonAsync("api/asistencias/bulk", dtos);
        return response.IsSuccessStatusCode;
    }

    public async Task<AsistenciaDTO?> UpdateAsync(int id, AsistenciaDTO dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/asistencias/{id}", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AsistenciaDTO>();
    }
}