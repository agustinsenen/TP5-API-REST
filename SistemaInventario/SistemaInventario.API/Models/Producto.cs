using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaInventario.API.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        public int Stock { get; set; }

        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        // Relación con Categoria
        public int CategoriaId { get; set; }

        public Categoria? Categoria { get; set; }

        public ICollection<DetalleIngreso> DetallesIngreso { get; set; } = new List<DetalleIngreso>();

        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}