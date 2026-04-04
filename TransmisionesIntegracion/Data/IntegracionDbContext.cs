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
        public DbSet<ServicioCache> ServiciosCache { get; set; }
        public DbSet<CondicionPagoCache> CondicionesPagoCache { get; set; }
        public DbSet<ProvinciaCache> ProvinciasCache { get; set; }
        public DbSet<MunicipioCache> MunicipiosCache { get; set; }
        public DbSet<EmpleadoCache> EmpleadosCache { get; set; }
        public DbSet<TransaccionPendiente> TransaccionesPendientes { get; set; }
        public DbSet<LogTrafico> LogsTrafico { get; set; }
        public DbSet<VehiculoCache> VehiculosCache { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<VehiculoCache>().HasKey(v => v.Matricula);
        }
    }
}