using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Venta;
using SistemaInventario.API.Models;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentas()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Select(v => new VentaDto
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    ClienteId = v.ClienteId,
                    ClienteNombre = v.Cliente != null
                        ? v.Cliente.Nombre + " " + v.Cliente.Apellido
                        : null,

                    Detalles = v.Detalles.Select(d => new DetalleVentaDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto != null
                            ? d.Producto.Nombre
                            : null,
                        Cantidad = d.Cantidad,
                        PrecioVenta = d.PrecioVenta
                    }).ToList()
                })
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VentaDto>> GetVenta(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(v => v.Id == id)
                .Select(v => new VentaDto
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    ClienteId = v.ClienteId,
                    ClienteNombre = v.Cliente != null
                        ? v.Cliente.Nombre + " " + v.Cliente.Apellido
                        : null,

                    Detalles = v.Detalles.Select(d => new DetalleVentaDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto != null
                            ? d.Producto.Nombre
                            : null,
                        Cantidad = d.Cantidad,
                        PrecioVenta = d.PrecioVenta
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (venta == null)
            {
                return NotFound(new
                {
                    mensaje = "La venta no existe."
                });
            }

            return Ok(venta);
        }

        [HttpPost]
        public async Task<ActionResult<VentaDto>> CrearVenta(
            CrearVentaDto dto)
        {
            // Verificar que el cliente exista
            var clienteExiste = await _context.Clientes
                .AnyAsync(c => c.Id == dto.ClienteId);

            if (!clienteExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El cliente indicado no existe."
                });
            }

            // Crear la venta
            var venta = new Venta
            {
                ClienteId = dto.ClienteId,

                // Temporal hasta implementar JWT
                UsuarioId = 1,

                Fecha = DateTime.Now
            };

            foreach (var detalleDto in dto.Detalles)
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p =>
                        p.Id == detalleDto.ProductoId);

                // Verificar que exista el producto
                if (producto == null)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            $"El producto con ID {detalleDto.ProductoId} no existe."
                    });
                }

                // Verificar stock
                if (producto.Stock < detalleDto.Cantidad)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            $"Stock insuficiente para el producto '{producto.Nombre}'. " +
                            $"Stock disponible: {producto.Stock}. " +
                            $"Cantidad solicitada: {detalleDto.Cantidad}."
                    });
                }

                var detalle = new DetalleVenta
                {
                    ProductoId = detalleDto.ProductoId,
                    Cantidad = detalleDto.Cantidad,
                    PrecioVenta = detalleDto.PrecioVenta
                };

                venta.Detalles.Add(detalle);

                // Reducir stock
                producto.Stock -= detalleDto.Cantidad;
            }

            _context.Ventas.Add(venta);

            await _context.SaveChangesAsync();

            // Obtener la venta creada con sus relaciones
            var resultado = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(v => v.Id == venta.Id)
                .Select(v => new VentaDto
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    ClienteId = v.ClienteId,
                    ClienteNombre = v.Cliente != null
                        ? v.Cliente.Nombre + " " + v.Cliente.Apellido
                        : null,

                    Detalles = v.Detalles.Select(d => new DetalleVentaDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto != null
                            ? d.Producto.Nombre
                            : null,
                        Cantidad = d.Cantidad,
                        PrecioVenta = d.PrecioVenta
                    }).ToList()
                })
                .FirstAsync();

            return CreatedAtAction(
                nameof(GetVenta),
                new { id = venta.Id },
                resultado
            );
        }
    }
}