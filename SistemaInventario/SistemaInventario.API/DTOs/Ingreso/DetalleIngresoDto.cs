namespace SistemaInventario.API.DTOs.Ingreso
{
    public class DetalleIngresoDto
    {
        public int Id { get; set; }

        public int ProductoId { get; set; }

        public string? ProductoNombre { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioCompra { get; set; }
    }
}