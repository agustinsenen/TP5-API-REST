namespace SistemaInventario.API.DTOs.Producto
{
    public class ProductoDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public string? ImagenUrl { get; set; }

        public int CategoriaId { get; set; }

        public string? CategoriaNombre { get; set; }
    }
}