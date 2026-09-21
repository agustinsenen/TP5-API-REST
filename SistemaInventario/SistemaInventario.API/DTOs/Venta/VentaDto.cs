namespace SistemaInventario.API.DTOs.Venta
{
    public class VentaDto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int ClienteId { get; set; }

        public string? ClienteNombre { get; set; }

        public List<DetalleVentaDto> Detalles { get; set; } = new();
    }
}