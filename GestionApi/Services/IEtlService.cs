using Microsoft.AspNetCore.Mvc;

namespace GestionApi.Services;

public interface IEtlService
{
    Task<byte[]> ExportarAlumnosAsync(int cursoId);
    Task<List<string>> ImportarAlumnosAsync(byte[] archivo);
}