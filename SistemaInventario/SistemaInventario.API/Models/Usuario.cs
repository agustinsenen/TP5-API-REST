using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Rol { get; set; } = "Usuario";

        public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();

        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}