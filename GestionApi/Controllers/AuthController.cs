using GestionApi.DTOs;
using GestionApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDTO dto)
    {
        var token = await _authService.LoginAsync(dto.Username, dto.Password);
        if (token == null) return Unauthorized("Credenciales inválidas.");
        return Ok(new { token });
    }

    [HttpPost("invitar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Invitar(InvitarUsuarioDTO dto)
    {
        var token = await _authService.InvitarUsuarioAsync(dto.Email, dto.Rol);
        if (token == null) return BadRequest("El email ya existe o el rol es inválido.");
        return Ok(new { token });
    }

    [HttpPost("activar")]
    public async Task<IActionResult> Activar(ActivarCuentaDTO dto)
    {
        var resultado = await _authService.ActivarCuentaAsync(dto.Token, dto.Username, dto.Password);
        if (!resultado) return BadRequest("Token inválido o username ya existe.");
        return Ok("Cuenta activada correctamente.");
    }
}