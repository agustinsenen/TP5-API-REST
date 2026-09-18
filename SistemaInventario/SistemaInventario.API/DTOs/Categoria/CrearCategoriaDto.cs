using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.DTOs.Categoria
{
    public class CrearCategoriaDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Descripcion { get; set; }
    }
}