using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaInventario.API.Models
{
    public class DetalleVenta
    {
        [Key]
        public int Id { get; set; }

        public int VentaId { get; set; }

        public Venta? Venta { get; set; }

        public int ProductoId { get; set; }

        public Producto? Producto { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }
    }
}