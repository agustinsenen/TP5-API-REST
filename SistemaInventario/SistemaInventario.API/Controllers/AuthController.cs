using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Auth;
using SistemaInventario.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registro(RegistroDto dto)
        {
            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExiste)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe un usuario con ese email."
                });
            }

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = "Usuario"
            };

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Usuario registrado correctamente.",
                usuarioId = usuario.Id,
                nombre = usuario.Nombre,
                email = usuario.Email,
                rol = usuario.Rol
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginRespuestaDto>> Login(
            LoginDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null)
            {
                return Unauthorized(new
                {
                    mensaje = "Email o contraseña incorrectos."
                });
            }

            var passwordCorrecta = BCrypt.Net.BCrypt.Verify(
                dto.Password,
                usuario.PasswordHash
            );

            if (!passwordCorrecta)
            {
                return Unauthorized(new
                {
                    mensaje = "Email o contraseña incorrectos."
                });
            }

            var token = GenerarToken(usuario);

            return Ok(new LoginRespuestaDto
            {
                Token = token,
                UsuarioId = usuario.Id,
                Nombre = usuario.Nombre,
                Rol = usuario.Rol
            });
        }

        private string GenerarToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    usuario.Nombre
                ),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Email
                ),

                new Claim(
                    ClaimTypes.Role,
                    usuario.Rol
                )
            };

            var clave = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(clave))
            {
                throw new InvalidOperationException(
                    "No se configuró la clave JWT."
                );
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(clave)
            );

            var credenciales = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}