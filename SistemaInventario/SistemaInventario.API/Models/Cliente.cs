using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [MaxLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(250)]
        public string? Direccion { get; set; }

        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}