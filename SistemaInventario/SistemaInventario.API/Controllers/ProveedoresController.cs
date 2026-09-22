using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Paginacion;
using SistemaInventario.API.DTOs.Proveedor;
using SistemaInventario.API.Models;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/proveedores
        // Usuario y Administrador
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<ProveedorDto>>> GetProveedores(
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1)
            {
                return BadRequest(new
                {
                    mensaje = "El número de página debe ser mayor o igual a 1."
                });
            }

            if (pageSize < 1 || pageSize > 50)
            {
                return BadRequest(new
                {
                    mensaje = "El tamaño de página debe estar entre 1 y 50."
                });
            }

            var query = _context.Proveedores.AsQueryable();

            var totalRegistros = await query.CountAsync();

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)pageSize
            );

            var proveedores = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProveedorDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Telefono = p.Telefono,
                    Email = p.Email,
                    Direccion = p.Direccion
                })
                .ToListAsync();

            var respuesta = new RespuestaPaginadaDto<ProveedorDto>
            {
                Pagina = page,
                TamanoPagina = pageSize,
                TotalRegistros = totalRegistros,
                TotalPaginas = totalPaginas,
                Datos = proveedores
            };

            return Ok(respuesta);
        }

        // GET: api/proveedores/5
        // Usuario y Administrador
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
        // Solo Administrador
        [HttpPost]
        [Authorize(Roles = "Administrador")]
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
        // Solo Administrador
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
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
        // Solo Administrador
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
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