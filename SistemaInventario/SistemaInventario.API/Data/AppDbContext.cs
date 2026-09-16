using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Models;

namespace SistemaInventario.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Producto> Productos { get; set; }

        public DbSet<Proveedor> Proveedores { get; set; }

        public DbSet<Cliente> Clientes { get; set; }

        public DbSet<Ingreso> Ingresos { get; set; }

        public DbSet<DetalleIngreso> DetallesIngreso { get; set; }

        public DbSet<Venta> Ventas { get; set; }

        public DbSet<DetalleVenta> DetallesVenta { get; set; }
    }
}