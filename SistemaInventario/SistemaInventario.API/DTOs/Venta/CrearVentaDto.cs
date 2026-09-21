using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.DTOs.Venta
{
    public class CrearVentaDto
    {
        [Required]
        public int ClienteId { get; set; }

        [Required]
        [MinLength(1)]
        public List<CrearDetalleVentaDto> Detalles { get; set; } = new();
    }

    public class CrearDetalleVentaDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PrecioVenta { get; set; }
    }
}