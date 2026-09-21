using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.DTOs.Proveedor
{
    public class CrearProveedorDto
    {
        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [EmailAddress]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(250)]
        public string? Direccion { get; set; }
    }
}