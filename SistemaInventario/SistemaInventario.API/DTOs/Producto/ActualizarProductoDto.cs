using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.DTOs.Producto
{
    public class ActualizarProductoDto
    {
        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Precio { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoriaId { get; set; }

        public IFormFile? Imagen { get; set; }
    }
}