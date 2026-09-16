using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.Models
{
    public class Ingreso
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // Proveedor
        public int ProveedorId { get; set; }

        public Proveedor? Proveedor { get; set; }

        // Usuario que registra el ingreso
        public int UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        public ICollection<DetalleIngreso> Detalles { get; set; } = new List<DetalleIngreso>();
    }
}