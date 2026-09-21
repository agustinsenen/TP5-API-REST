using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Proveedor;
using SistemaInventario.API.Models;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/proveedores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetProveedores()
        {
            var proveedores = await _context.Proveedores
                .Select(p => new ProveedorDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Telefono = p.Telefono,
                    Email = p.Email,
                    Direccion = p.Direccion
                })
                .ToListAsync();

            return Ok(proveedores);
        }

        // GET: api/proveedores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProveedorDto>> GetProveedor(int id)
        {
            var proveedor = await _context.Proveedores
                .Where(p => p.Id == id)
                .Select(p => new ProveedorDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Telefono = p.Telefono,
                    Email = p.Email,
                    Direccion = p.Direccion
                })
                .FirstOrDefaultAsync();

            if (proveedor == null)
            {
                return NotFound(new
                {
                    mensaje = "El proveedor no existe."
                });
            }

            return Ok(proveedor);
        }

        // POST: api/proveedores
        [HttpPost]
        public async Task<ActionResult<ProveedorDto>> CrearProveedor(
            CrearProveedorDto dto)
        {
            var proveedor = new Proveedor
            {
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion
            };

            _context.Proveedores.Add(proveedor);

            await _context.SaveChangesAsync();

            var resultado = new ProveedorDto
            {
                Id = proveedor.Id,
                Nombre = proveedor.Nombre,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                Direccion = proveedor.Direccion
            };

            return CreatedAtAction(
                nameof(GetProveedor),
                new { id = proveedor.Id },
                resultado);
        }

        // PUT: api/proveedores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarProveedor(
            int id,
            ActualizarProveedorDto dto)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
            {
                return NotFound(new
                {
                    mensaje = "El proveedor no existe."
                });
            }

            proveedor.Nombre = dto.Nombre;
            proveedor.Telefono = dto.Telefono;
            proveedor.Email = dto.Email;
            proveedor.Direccion = dto.Direccion;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/proveedores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
            {
                return NotFound(new
                {
                    mensaje = "El proveedor no existe."
                });
            }

            var tieneIngresos = await _context.Ingresos
                .AnyAsync(i => i.ProveedorId == id);

            if (tieneIngresos)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar el proveedor porque tiene ingresos asociados."
                });
            }

            _context.Proveedores.Remove(proveedor);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}