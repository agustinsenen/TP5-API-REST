namespace SistemaInventario.API.DTOs.Ingreso
{
    public class IngresoDto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int ProveedorId { get; set; }

        public string? ProveedorNombre { get; set; }

        public List<DetalleIngresoDto> Detalles { get; set; } = new();
    }
}