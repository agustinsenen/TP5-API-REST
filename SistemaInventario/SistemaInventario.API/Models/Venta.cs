using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.Models
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // Cliente
        public int ClienteId { get; set; }

        public Cliente? Cliente { get; set; }

        // Usuario que registra la venta
        public int UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}