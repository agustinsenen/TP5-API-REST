using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Categoria;
using SistemaInventario.API.DTOs.Paginacion;
using SistemaInventario.API.Models;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/categorias
        // Usuario y Administrador
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<CategoriaDto>>> GetCategorias(
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

            var query = _context.Categorias.AsQueryable();

            var totalRegistros = await query.CountAsync();

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)pageSize
            );

            var categorias = await query
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoriaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion
                })
                .ToListAsync();

            var respuesta = new RespuestaPaginadaDto<CategoriaDto>
            {
                Pagina = page,
                TamanoPagina = pageSize,
                TotalRegistros = totalRegistros,
                TotalPaginas = totalPaginas,
                Datos = categorias
            };

            return Ok(respuesta);
        }

        // GET: api/categorias/5
        // Usuario y Administrador
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Where(c => c.Id == id)
                .Select(c => new CategoriaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion
                })
                .FirstOrDefaultAsync();

            if (categoria == null)
            {
                return NotFound(new
                {
                    mensaje = "La categoría no existe."
                });
            }

            return Ok(categoria);
        }

        // POST: api/categorias
        // Solo Administrador
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<CategoriaDto>> CrearCategoria(
            CrearCategoriaDto dto)
        {
            var categoria = new Categoria
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion
            };

            _context.Categorias.Add(categoria);

            await _context.SaveChangesAsync();

            var resultado = new CategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion
            };

            return CreatedAtAction(
                nameof(GetCategoria),
                new { id = categoria.Id },
                resultado);
        }

        // PUT: api/categorias/5
        // Solo Administrador
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarCategoria(
            int id,
            ActualizarCategoriaDto dto)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound(new
                {
                    mensaje = "La categoría no existe."
                });
            }

            categoria.Nombre = dto.Nombre;
            categoria.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/categorias/5
        // Solo Administrador
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound(new
                {
                    mensaje = "La categoría no existe."
                });
            }

            var tieneProductos = await _context.Productos
                .AnyAsync(p => p.CategoriaId == id);

            if (tieneProductos)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar la categoría porque tiene productos asociados."
                });
            }

            _context.Categorias.Remove(categoria);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}