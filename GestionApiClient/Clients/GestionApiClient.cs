using GestionApiClient.Clients;

namespace GestionApiClient;

public class GestionApiClient
{
    private readonly HttpClient _httpClient;

    public CursosClient Cursos { get; }
    public AlumnosClient Alumnos { get; }
    public AsistenciasClient Asistencias { get; }
    public EtlClient Etl { get; }
    public AuthClient Auth { get; }

    public GestionApiClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        Cursos = new CursosClient(_httpClient);
        Alumnos = new AlumnosClient(_httpClient);
        Asistencias = new AsistenciasClient(_httpClient);
        Etl = new EtlClient(_httpClient);
        Auth = new AuthClient(_httpClient);
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}