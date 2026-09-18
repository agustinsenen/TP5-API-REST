using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.DTOs.Categoria
{
    public class ActualizarCategoriaDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Descripcion { get; set; }
    }
}