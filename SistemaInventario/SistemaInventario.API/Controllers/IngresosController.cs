using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Ingreso;
using SistemaInventario.API.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IngresosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IngresosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ingresos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngresoDto>>> GetIngresos()
        {
            var ingresos = await _context.Ingresos
                .Include(i => i.Proveedor)
                .Include(i => i.Detalles)
                    .ThenInclude(d => d.Producto)
                .Select(i => new IngresoDto
                {
                    Id = i.Id,
                    Fecha = i.Fecha,
                    ProveedorId = i.ProveedorId,
                    ProveedorNombre = i.Proveedor != null
                        ? i.Proveedor.Nombre
                        : null,

                    Detalles = i.Detalles.Select(d => new DetalleIngresoDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto != null
                            ? d.Producto.Nombre
                            : null,
                        Cantidad = d.Cantidad,
                        PrecioCompra = d.PrecioCompra
                    }).ToList()
                })
                .ToListAsync();

            return Ok(ingresos);
        }

        // GET: api/ingresos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IngresoDto>> GetIngreso(int id)
        {
            var ingreso = await _context.Ingresos
                .Include(i => i.Proveedor)
                .Include(i => i.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(i => i.Id == id)
                .Select(i => new IngresoDto
                {
                    Id = i.Id,
                    Fecha = i.Fecha,
                    ProveedorId = i.ProveedorId,
                    ProveedorNombre = i.Proveedor != null
                        ? i.Proveedor.Nombre
                        : null,

                    Detalles = i.Detalles.Select(d => new DetalleIngresoDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto != null
                            ? d.Producto.Nombre
                            : null,
                        Cantidad = d.Cantidad,
                        PrecioCompra = d.PrecioCompra
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (ingreso == null)
            {
                return NotFound(new
                {
                    mensaje = "El ingreso no existe."
                });
            }

            return Ok(ingreso);
        }

        // POST: api/ingresos
        [HttpPost]
        public async Task<ActionResult<IngresoDto>> CrearIngreso(
            CrearIngresoDto dto)
        {
            // 1. Verificar proveedor
            var proveedorExiste = await _context.Proveedores
                .AnyAsync(p => p.Id == dto.ProveedorId);

            if (!proveedorExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El proveedor indicado no existe."
                });
            }

            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usuarioIdClaim == null)
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            var usuarioId = int.Parse(usuarioIdClaim.Value);

            // 2. Crear el ingreso
            var ingreso = new Ingreso
            {
                ProveedorId = dto.ProveedorId,
                UsuarioId = usuarioId,
                Fecha = DateTime.Now
            };

            // 3. Procesar cada detalle
            foreach (var detalleDto in dto.Detalles)
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == detalleDto.ProductoId);

                if (producto == null)
                {
                    return BadRequest(new
                    {
                        mensaje = $"El producto con ID {detalleDto.ProductoId} no existe."
                    });
                }

                var detalle = new DetalleIngreso
                {
                    ProductoId = detalleDto.ProductoId,
                    Cantidad = detalleDto.Cantidad,
                    PrecioCompra = detalleDto.PrecioCompra
                };

                ingreso.Detalles.Add(detalle);

                // Aumentar stock
                producto.Stock += detalleDto.Cantidad;
            }

            _context.Ingresos.Add(ingreso);

            await _context.SaveChangesAsync();

            // 4. Obtener el ingreso creado con sus relaciones
            var resultado = await _context.Ingresos
                .Include(i => i.Proveedor)
                .Include(i => i.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(i => i.Id == ingreso.Id)
                .Select(i => new IngresoDto
                {
                    Id = i.Id,
                    Fecha = i.Fecha,
                    ProveedorId = i.ProveedorId,
                    ProveedorNombre = i.Proveedor != null
                        ? i.Proveedor.Nombre
                        : null,

                    Detalles = i.Detalles.Select(d => new DetalleIngresoDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto != null
                            ? d.Producto.Nombre
                            : null,
                        Cantidad = d.Cantidad,
                        PrecioCompra = d.PrecioCompra
                    }).ToList()
                })
                .FirstAsync();

            return CreatedAtAction(
                nameof(GetIngreso),
                new { id = ingreso.Id },
                resultado);
        }
    }
}