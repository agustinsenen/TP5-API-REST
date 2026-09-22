using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Paginacion;
using SistemaInventario.API.DTOs.Producto;
using SistemaInventario.API.Models;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductosController(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/productos
        // Usuario y Administrador
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<ProductoDto>>> GetProductos(
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

            var query = _context.Productos
                .Include(p => p.Categoria)
                .AsQueryable();

            var totalRegistros = await query.CountAsync();

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)pageSize
            );

            var productos = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    ImagenUrl = p.ImagenUrl,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria != null
                        ? p.Categoria.Nombre
                        : null
                })
                .ToListAsync();

            var respuesta = new RespuestaPaginadaDto<ProductoDto>
            {
                Pagina = page,
                TamanoPagina = pageSize,
                TotalRegistros = totalRegistros,
                TotalPaginas = totalPaginas,
                Datos = productos
            };

            return Ok(respuesta);
        }

        // GET: api/productos/5
        // Usuario y Administrador
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Id == id)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    ImagenUrl = p.ImagenUrl,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria != null
                        ? p.Categoria.Nombre
                        : null
                })
                .FirstOrDefaultAsync();

            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje = "El producto no existe."
                });
            }

            return Ok(producto);
        }

        // POST: api/productos
        // Solo Administrador
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<ProductoDto>> CrearProducto(
            [FromForm] CrearProductoDto dto)
        {
            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId);

            if (!categoriaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La categoría indicada no existe."
                });
            }

            string? imagenUrl = null;

            if (dto.Imagen != null)
            {
                var resultadoImagen = await GuardarImagenAsync(dto.Imagen);

                if (!resultadoImagen.Exito)
                {
                    return BadRequest(new
                    {
                        mensaje = resultadoImagen.Mensaje
                    });
                }

                imagenUrl = resultadoImagen.Ruta;
            }

            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Stock = 0,
                CategoriaId = dto.CategoriaId,
                ImagenUrl = imagenUrl
            };

            _context.Productos.Add(producto);

            await _context.SaveChangesAsync();

            var resultado = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Id == producto.Id)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    ImagenUrl = p.ImagenUrl,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria != null
                        ? p.Categoria.Nombre
                        : null
                })
                .FirstAsync();

            return CreatedAtAction(
                nameof(GetProducto),
                new { id = producto.Id },
                resultado);
        }

        // PUT: api/productos/5
        // Solo Administrador
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarProducto(
            int id,
            [FromForm] ActualizarProductoDto dto)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje = "El producto no existe."
                });
            }

            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId);

            if (!categoriaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La categoría indicada no existe."
                });
            }

            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.Precio = dto.Precio;
            producto.CategoriaId = dto.CategoriaId;

            if (dto.Imagen != null)
            {
                var imagenAnterior = producto.ImagenUrl;

                var resultadoImagen = await GuardarImagenAsync(dto.Imagen);

                if (!resultadoImagen.Exito)
                {
                    return BadRequest(new
                    {
                        mensaje = resultadoImagen.Mensaje
                    });
                }

                producto.ImagenUrl = resultadoImagen.Ruta;

                EliminarImagen(imagenAnterior);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/productos/5
        // Solo Administrador
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje = "El producto no existe."
                });
            }

            var imagenUrl = producto.ImagenUrl;

            _context.Productos.Remove(producto);

            await _context.SaveChangesAsync();

            EliminarImagen(imagenUrl);

            return NoContent();
        }

        // ============================================================
        // MÉTODOS AUXILIARES PARA IMÁGENES
        // ============================================================

        private async Task<(bool Exito, string? Ruta, string? Mensaje)> GuardarImagenAsync(
            IFormFile imagen)
        {
            var extensionesPermitidas = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension = Path.GetExtension(imagen.FileName)
                .ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                return (
                    false,
                    null,
                    "Formato de imagen no permitido. Use JPG, JPEG, PNG o WEBP."
                );
            }

            const long tamanoMaximo = 5 * 1024 * 1024;

            if (imagen.Length > tamanoMaximo)
            {
                return (
                    false,
                    null,
                    "La imagen no puede superar los 5 MB."
                );
            }

            if (imagen.Length == 0)
            {
                return (
                    false,
                    null,
                    "La imagen está vacía."
                );
            }

            var carpetaImagenes = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "productos"
            );

            if (!Directory.Exists(carpetaImagenes))
            {
                Directory.CreateDirectory(carpetaImagenes);
            }

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";

            var rutaArchivo = Path.Combine(
                carpetaImagenes,
                nombreArchivo
            );

            using (var stream = new FileStream(
                rutaArchivo,
                FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            var rutaPublica = $"/uploads/productos/{nombreArchivo}";

            return (true, rutaPublica, null);
        }

        private void EliminarImagen(string? imagenUrl)
        {
            if (string.IsNullOrWhiteSpace(imagenUrl))
            {
                return;
            }

            var nombreArchivo = Path.GetFileName(imagenUrl);

            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                return;
            }

            var carpetaImagenes = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "productos"
            );

            var rutaArchivo = Path.Combine(
                carpetaImagenes,
                nombreArchivo
            );

            if (System.IO.File.Exists(rutaArchivo))
            {
                System.IO.File.Delete(rutaArchivo);
            }
        }
    }
}