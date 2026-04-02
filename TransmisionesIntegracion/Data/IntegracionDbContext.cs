using Microsoft.EntityFrameworkCore;
using TransmisionesIntegracion.Models;

namespace TransmisionesIntegracion.Data
{
    public class IntegracionDbContext : DbContext
    {
        public IntegracionDbContext(DbContextOptions<IntegracionDbContext> options) : base(options)
        {
        }

        public DbSet<ProductoCache> ProductosCache { get; set; }
        public DbSet<ClienteCache> ClientesCache { get; set; }
        public DbSet<OrdenCache> OrdenesCache { get; set; }
        public DbSet<TransaccionPendiente> TransaccionesPendientes { get; set; }
        public DbSet<LogTrafico> LogsTrafico { get; set; }
    }
}