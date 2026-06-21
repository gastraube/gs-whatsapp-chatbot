using gschatbot.api.Configuration;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace gschatbot.api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly JwtOptions _jwt;

    public AuthController(IUsuarioRepository usuarioRepo, IOptions<JwtOptions> jwt)
    {
        _usuarioRepo = usuarioRepo;
        _jwt = jwt.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await _usuarioRepo.BuscarPorEmailAsync(request.Email);
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            return Unauthorized(new { message = "E-mail ou senha inválidos." });

        var token = GerarToken(usuario);
        return Ok(new { token, nome = usuario.Nome, email = usuario.Email, role = usuario.Role });
    }

    private string GerarToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, usuario.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwt.ExpiracaoHoras),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest(string Email, string Senha);
