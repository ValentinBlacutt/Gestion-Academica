using System.Net.Http.Json;

namespace GestionApiClient.Clients;

public class EtlClient
{
    private readonly HttpClient _httpClient;

    public EtlClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]?> ExportarAlumnosAsync(int cursoId)
    {
        var response = await _httpClient.GetAsync($"api/etl/exportar/alumnos/{cursoId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<List<string>?> ImportarAlumnosAsync(byte[] archivo)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(archivo);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "archivo", "alumnos.xlsx");

        var response = await _httpClient.PostAsync("api/etl/importar/alumnos", content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<string>>();
    }
}