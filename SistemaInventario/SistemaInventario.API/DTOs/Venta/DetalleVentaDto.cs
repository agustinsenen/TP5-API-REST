namespace SistemaInventario.API.DTOs.Venta
{
    public class DetalleVentaDto
    {
        public int Id { get; set; }

        public int ProductoId { get; set; }

        public string? ProductoNombre { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioVenta { get; set; }
    }
}