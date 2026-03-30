using GestionApiClient.Clients;

namespace GestionApiClient;

public class GestionApiClient
{
    public CursosClient Cursos { get; }
    public AlumnosClient Alumnos { get; }
    public AsistenciasClient Asistencias { get; }

    public EtlClient Etl { get; }

    public GestionApiClient(string baseUrl)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        Cursos = new CursosClient(httpClient);
        Alumnos = new AlumnosClient(httpClient);
        Asistencias = new AsistenciasClient(httpClient);
        Etl = new EtlClient(httpClient);
    }
}