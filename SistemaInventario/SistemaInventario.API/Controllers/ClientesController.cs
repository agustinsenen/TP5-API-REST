using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Paginacion;
using SistemaInventario.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/clientes
        // Usuario y Administrador
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<Cliente>>> GetClientes(
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

            var query = _context.Clientes.AsQueryable();

            var totalRegistros = await query.CountAsync();

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)pageSize
            );

            var clientes = await query
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var respuesta = new RespuestaPaginadaDto<Cliente>
            {
                Pagina = page,
                TamanoPagina = pageSize,
                TotalRegistros = totalRegistros,
                TotalPaginas = totalPaginas,
                Datos = clientes
            };

            return Ok(respuesta);
        }

        // GET: api/clientes/5
        // Usuario y Administrador
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound(new
                {
                    mensaje = "El cliente no existe."
                });
            }

            return Ok(cliente);
        }

        // POST: api/clientes
        // Solo Administrador
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Cliente>> CrearCliente(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCliente),
                new { id = cliente.Id },
                cliente
            );
        }

        // PUT: api/clientes/5
        // Solo Administrador
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarCliente(
            int id,
            Cliente cliente)
        {
            if (id != cliente.Id)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la URL no coincide con el ID del cliente."
                });
            }

            var clienteExiste = await _context.Clientes
                .AnyAsync(c => c.Id == id);

            if (!clienteExiste)
            {
                return NotFound(new
                {
                    mensaje = "El cliente no existe."
                });
            }

            _context.Entry(cliente).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/clientes/5
        // Solo Administrador
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Ventas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound(new
                {
                    mensaje = "El cliente no existe."
                });
            }

            if (cliente.Ventas.Any())
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar el cliente porque tiene ventas asociadas."
                });
            }

            _context.Clientes.Remove(cliente);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}