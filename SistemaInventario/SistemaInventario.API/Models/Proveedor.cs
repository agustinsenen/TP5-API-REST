using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.Models
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [MaxLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(250)]
        public string? Direccion { get; set; }

        public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
    }
}