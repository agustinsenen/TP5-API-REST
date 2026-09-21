using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.Models;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return await _context.Clientes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound(new { mensaje = "El cliente no existe." });
            }

            return Ok(cliente);
        }

        [HttpPost]
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

        [HttpPut("{id}")]
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
                return NotFound(new { mensaje = "El cliente no existe." });
            }

            _context.Entry(cliente).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Ventas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound(new { mensaje = "El cliente no existe." });
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