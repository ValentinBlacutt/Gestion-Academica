using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionApi.Data;
using GestionApi.Enums;
using GestionApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GestionApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Username == username && u.EstaActivo);

        if (usuario == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash)) return null;

        return GenerarToken(usuario);
    }

    public async Task<string?> InvitarUsuarioAsync(string email, string rol)
    {
        var existe = await _context.Usuarios.AnyAsync(u => u.Email == email);
        if (existe) return null;
        if (!Enum.TryParse<Rol>(rol, ignoreCase: true, out var rolEnum)) return null;

        var tokenActivacion = Guid.NewGuid().ToString();

        var usuario = new Usuario
        {
            Email = email,
            Rol = rolEnum,
            EstaActivo = false,
            TokenActivacion = tokenActivacion,
            FechaCreacion = DateTime.UtcNow
        };
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return tokenActivacion;
    }

    public async Task<bool> ActivarCuentaAsync(string token, string username, string password)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.TokenActivacion == token && !u.EstaActivo);

        if (usuario == null) return false;

        var usernameExiste = await _context.Usuarios
            .AnyAsync(u => u.Username == username);
        if (usernameExiste) return false;

        usuario.Username = username;
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        usuario.EstaActivo = true;
        usuario.TokenActivacion = null;

        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerarToken(Usuario usuario)
    {
        var secretKey = _configuration["Jwt:SecretKey"]!;
        var issuer = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;
        var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"]!);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Username!),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}