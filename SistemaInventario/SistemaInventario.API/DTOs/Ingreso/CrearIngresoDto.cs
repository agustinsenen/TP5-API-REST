using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.DTOs.Ingreso
{
    public class CrearIngresoDto
    {
        [Required]
        public int ProveedorId { get; set; }

        [Required]
        [MinLength(1)]
        public List<CrearDetalleIngresoDto> Detalles { get; set; } = new();
    }

    public class CrearDetalleIngresoDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PrecioCompra { get; set; }
    }
}