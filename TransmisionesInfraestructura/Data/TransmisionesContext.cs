using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;

namespace TransmisionesInfraestructura.Data;

public class TransmisionesContext : DbContext
{
    public TransmisionesContext(DbContextOptions<TransmisionesContext> options) : base(options) { }

    public DbSet<Provincia> Provincias { get; set; }
    public DbSet<Municipio> Municipios { get; set; }
    public DbSet<Sector> Sectores { get; set; }
    public DbSet<TipoTransmision> TiposTransmision { get; set; }
    public DbSet<CategoriaProducto> CategoriasProductos { get; set; }
    public DbSet<TipoServicio> TiposServicios { get; set; }
    public DbSet<CondicionesPago> CondicionesPagos { get; set; }
    public DbSet<MetodoPago> MetodosPagos { get; set; }
    public DbSet<CanalVenta> CanalesVentas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Sucursal> Sucursales { get; set; }
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<Caja> Cajas { get; set; }
    public DbSet<MovimientoCaja> MovimientosCaja { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Servicio> Servicios { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<Orden> Ordenes { get; set; }
    public DbSet<DetalleOrden> DetallesOrden { get; set; }
    public DbSet<DetalleServicio> DetallesServicios { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<CuentaCobrar> CuentasCobrar { get; set; }
    public DbSet<Cobro> Cobros { get; set; }
    public DbSet<OrdenCompra> OrdenesCompra { get; set; }
    public DbSet<DetalleCompra> DetallesCompras { get; set; }
    public DbSet<CuentaPagar> CuentasPagar { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<Garantia> Garantias { get; set; }
    public DbSet<Devolucion> Devoluciones { get; set; }
    public DbSet<AjusteInventario> AjustesInventario { get; set; }
    public DbSet<Log> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // =====================================================
        // 1. TABLAS Y PKs
        // =====================================================
        modelBuilder.Entity<Provincia>().ToTable("Provincia").HasKey(p => p.Id_provincia);
        modelBuilder.Entity<Municipio>().ToTable("Municipio").HasKey(m => m.Id_municipio);
        modelBuilder.Entity<Sector>().ToTable("Sector").HasKey(s => s.Id_sector);
        modelBuilder.Entity<TipoTransmision>().ToTable("Tipo_Transmision").HasKey(t => t.Id_tipo_trans);
        modelBuilder.Entity<CategoriaProducto>().ToTable("Categoria_Producto").HasKey(c => c.Id_categoria);
        modelBuilder.Entity<TipoServicio>().ToTable("Tipo_Servicio").HasKey(t => t.Id_tipo_servicio);
        modelBuilder.Entity<CondicionesPago>().ToTable("Condiciones_Pago").HasKey(c => c.Id_condicion_pago);
        modelBuilder.Entity<MetodoPago>().ToTable("MetodoPago").HasKey(m => m.Id_metodopago);
        modelBuilder.Entity<CanalVenta>().ToTable("Canal_Venta").HasKey(c => c.Id_canal);
        modelBuilder.Entity<Usuario>().ToTable("Usuario").HasKey(u => u.Id_usuario);
        modelBuilder.Entity<Sucursal>().ToTable("Sucursal").HasKey(s => s.Id_sucursal);
        modelBuilder.Entity<Empleado>().ToTable("Empleado").HasKey(e => e.Id_empleado);
        modelBuilder.Entity<Log>().ToTable("Log").HasKey(l => l.Id_log);
        modelBuilder.Entity<Producto>().ToTable("Producto").HasKey(p => p.Id_producto);
        modelBuilder.Entity<Servicio>().ToTable("Servicio").HasKey(s => s.Id_servicio);
        modelBuilder.Entity<Proveedor>().ToTable("Proveedor").HasKey(p => p.Id_proveedor);
        modelBuilder.Entity<OrdenCompra>().ToTable("Orden_Compra").HasKey(o => o.Id_orden_compra);
        modelBuilder.Entity<Vehiculo>().ToTable("Vehiculo").HasKey(v => v.Matricula);
        modelBuilder.Entity<Orden>().ToTable("Orden").HasKey(o => o.Id_orden);
        modelBuilder.Entity<Factura>().ToTable("Factura").HasKey(f => f.Id_factura);
        modelBuilder.Entity<Cobro>().ToTable("Cobro").HasKey(c => c.Id_cobro);
        modelBuilder.Entity<Pago>().ToTable("Pago").HasKey(p => p.Id_pago);
        modelBuilder.Entity<Garantia>().ToTable("Garantia").HasKey(g => g.Id_garantia);
        modelBuilder.Entity<Devolucion>().ToTable("Devolucion").HasKey(d => d.Id_devolucion);
        modelBuilder.Entity<AjusteInventario>().ToTable("Ajuste_Inventario").HasKey(a => a.Id_ajuste);
        modelBuilder.Entity<CuentaPagar>().ToTable("Cuenta_Pagar").HasKey(c => c.Id_cpagar);
        modelBuilder.Entity<CuentaCobrar>().ToTable("Cuenta_Cobrar").HasKey(c => c.Id_ccobro);
        modelBuilder.Entity<Cliente>().ToTable("Cliente").HasKey(c => c.Id_cliente);

        // =====================================================
        // 2. CLAVES COMPUESTAS
        // =====================================================
        modelBuilder.Entity<DetalleOrden>().ToTable("Detalle_Orden").HasKey(d => new { d.Id_orden, d.Id_producto });
        modelBuilder.Entity<DetalleCompra>().ToTable("Detalle_Compra").HasKey(d => new { d.Id_orden_compra, d.Id_producto });
        modelBuilder.Entity<DetalleServicio>().ToTable("Detalle_Servicio").HasKey(d => new { d.Id_orden, d.Id_servicio });

        // =====================================================
        // 3. RELACIONES GEOGRAFÍA
        // =====================================================
        modelBuilder.Entity<Municipio>()
            .HasOne(m => m.Provincia).WithMany(p => p.Municipios)
            .HasForeignKey(m => m.Id_provincia).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Sector>()
            .HasOne(s => s.Municipio).WithMany(m => m.Sectores)
            .HasForeignKey(s => s.Id_municipio).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Sucursal>()
            .HasOne(s => s.Municipio).WithMany()
            .HasForeignKey(s => s.Id_municipio).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 4. RELACIONES CLIENTE
        // =====================================================
        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Provincia).WithMany()
            .HasForeignKey(c => c.Id_provincia).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Municipio).WithMany()
            .HasForeignKey(c => c.Id_municipio).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Sector).WithMany()
            .HasForeignKey(c => c.Id_sector).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 5. RELACIONES PROVEEDOR
        // =====================================================
        modelBuilder.Entity<Proveedor>()
            .HasOne(p => p.Provincia).WithMany()
            .HasForeignKey(p => p.Id_provincia).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Proveedor>()
            .HasOne(p => p.Municipio).WithMany()
            .HasForeignKey(p => p.Id_municipio).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Proveedor>()
            .HasOne(p => p.Sector).WithMany()
            .HasForeignKey(p => p.Id_sector).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 6. RELACIONES EMPLEADO
        // =====================================================
        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.Usuario).WithMany()
            .HasForeignKey(e => e.Id_usuario).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.Sucursal).WithMany()
            .HasForeignKey(e => e.Id_sucursal).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 7. RELACIONES CAJA
        // =====================================================
        modelBuilder.Entity<Caja>().ToTable("Caja").HasKey(c => c.Id_caja);

        modelBuilder.Entity<Caja>()
            .HasOne(c => c.Sucursal).WithMany()
            .HasForeignKey(c => c.Id_sucursal).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Caja>()
            .HasOne(c => c.UsuarioApertura).WithMany()
            .HasForeignKey(c => c.Id_usuario_apertura).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Caja>()
            .HasOne(c => c.UsuarioCierre).WithMany()
            .HasForeignKey(c => c.Id_usuario_cierre).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 7B. RELACIONES MOVIMIENTO CAJA
        // =====================================================
        modelBuilder.Entity<MovimientoCaja>().ToTable("MovimientoCaja").HasKey(m => m.Id_movimiento);

        modelBuilder.Entity<MovimientoCaja>()
            .HasOne(m => m.Caja).WithMany()
            .HasForeignKey(m => m.Id_caja).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoCaja>()
            .HasOne(m => m.Usuario).WithMany()
            .HasForeignKey(m => m.Id_usuario).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 8. RELACIONES PRODUCTO
        // =====================================================
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria).WithMany()
            .HasForeignKey(p => p.Id_categoria).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.TipoTransmision).WithMany()
            .HasForeignKey(p => p.Id_tipo_trans).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 9. RELACIONES VEHICULO
        // =====================================================
        modelBuilder.Entity<Vehiculo>()
            .HasOne(v => v.Cliente).WithMany()
            .HasForeignKey(v => v.Id_cliente).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vehiculo>()
            .HasOne(v => v.TipoTransmision).WithMany()
            .HasForeignKey(v => v.Id_tipo_trans).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 10. RELACIONES SERVICIO
        // =====================================================
        modelBuilder.Entity<Servicio>()
            .HasOne(s => s.TipoServicio).WithMany()
            .HasForeignKey(s => s.Id_tipo_servicio).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 11. RELACIONES ORDEN
        // =====================================================
        modelBuilder.Entity<Orden>()
            .HasOne(o => o.Cliente).WithMany()
            .HasForeignKey(o => o.Id_cliente).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Orden>()
            .HasOne(o => o.Empleado).WithMany()
            .HasForeignKey(o => o.Id_empleado).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Orden>()
            .HasOne(o => o.Canal).WithMany()
            .HasForeignKey(o => o.Id_canal).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Orden>()
            .HasOne(o => o.Vehiculo).WithMany()
            .HasForeignKey(o => o.Id_vehiculo).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 12. RELACIONES DETALLE ORDEN
        // =====================================================
        modelBuilder.Entity<DetalleOrden>()
            .HasOne(d => d.Orden).WithMany(o => o.DetallesOrden)
            .HasForeignKey(d => d.Id_orden).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleOrden>()
            .HasOne(d => d.Producto).WithMany()
            .HasForeignKey(d => d.Id_producto).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 13. RELACIONES DETALLE SERVICIO
        // =====================================================
        modelBuilder.Entity<DetalleServicio>()
            .HasOne(d => d.Orden).WithMany(o => o.DetallesServicio)
            .HasForeignKey(d => d.Id_orden).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleServicio>()
            .HasOne(d => d.Servicio).WithMany()
            .HasForeignKey(d => d.Id_servicio).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleServicio>()
            .HasOne(d => d.EmpleadoTecnico).WithMany()
            .HasForeignKey(d => d.Id_empleado_tecnico).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 14. RELACIONES FACTURA
        // =====================================================
        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Orden).WithMany()
            .HasForeignKey(f => f.Id_orden).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Cliente).WithMany()
            .HasForeignKey(f => f.Id_cliente).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Empleado).WithMany()
            .HasForeignKey(f => f.Id_empleado).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Caja).WithMany()
            .HasForeignKey(f => f.Id_caja).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 15. RELACIONES CUENTA COBRAR / COBRO
        // =====================================================
        modelBuilder.Entity<CuentaCobrar>()
            .HasOne(c => c.Orden).WithMany()
            .HasForeignKey(c => c.Id_orden).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cobro>()
            .HasOne(c => c.CuentaCobrar).WithMany()
            .HasForeignKey(c => c.Id_ccobro).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cobro>()
            .HasOne(c => c.MetodoPago).WithMany()
            .HasForeignKey(c => c.Id_metodopago).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cobro>()
            .HasOne(c => c.Caja).WithMany()
            .HasForeignKey(c => c.Id_caja).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cobro>()
            .HasOne(c => c.Usuario).WithMany()
            .HasForeignKey(c => c.Id_usuario).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 16. RELACIONES ORDEN COMPRA / DETALLE COMPRA
        // =====================================================
        modelBuilder.Entity<OrdenCompra>()
            .HasOne(o => o.Proveedor).WithMany()
            .HasForeignKey(o => o.Id_proveedor).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdenCompra>()
            .HasOne(o => o.Empleado).WithMany()
            .HasForeignKey(o => o.Id_empleado).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleCompra>()
            .HasOne(d => d.OrdenCompra).WithMany(o => o.Detalles)
            .HasForeignKey(d => d.Id_orden_compra).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleCompra>()
            .HasOne(d => d.Producto).WithMany()
            .HasForeignKey(d => d.Id_producto).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 17. RELACIONES CUENTA PAGAR / PAGO
        // =====================================================
        modelBuilder.Entity<CuentaPagar>()
            .HasOne(c => c.OrdenCompra).WithMany()
            .HasForeignKey(c => c.Id_orden_compra).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pago>()
            .HasOne(p => p.CuentaPagar).WithMany()
            .HasForeignKey(p => p.Id_cpagar).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pago>()
            .HasOne(p => p.MetodoPago).WithMany()
            .HasForeignKey(p => p.Id_metodopago).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pago>()
            .HasOne(p => p.Caja).WithMany()
            .HasForeignKey(p => p.Id_caja).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pago>()
            .HasOne(p => p.Usuario).WithMany()
            .HasForeignKey(p => p.Id_usuario).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 18. RELACIONES GARANTIA / DEVOLUCION
        // =====================================================
        modelBuilder.Entity<Garantia>()
            .HasOne(g => g.Orden).WithMany()
            .HasForeignKey(g => g.Id_orden).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Garantia>()
            .HasOne(g => g.Producto).WithMany()
            .HasForeignKey(g => g.Id_producto).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Garantia>()
            .HasOne(g => g.Cliente).WithMany()
            .HasForeignKey(g => g.Id_cliente).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Devolucion>()
            .HasOne(d => d.Orden).WithMany()
            .HasForeignKey(d => d.Id_orden).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Devolucion>()
            .HasOne(d => d.Producto).WithMany()
            .HasForeignKey(d => d.Id_producto).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Devolucion>()
            .HasOne(d => d.Empleado).WithMany()
            .HasForeignKey(d => d.Id_empleado).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 19. RELACIONES AJUSTE INVENTARIO / LOG
        // =====================================================
        modelBuilder.Entity<AjusteInventario>()
            .HasOne(a => a.Producto).WithMany()
            .HasForeignKey(a => a.Id_producto).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AjusteInventario>()
            .HasOne(a => a.Empleado).WithMany()
            .HasForeignKey(a => a.Id_empleado).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Log>()
            .HasOne(l => l.Usuario).WithMany()
            .HasForeignKey(l => l.Id_usuario).OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 20. DECIMALES
        // =====================================================
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }
    }
}
