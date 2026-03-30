namespace TransmisionesCore.Entities;
public class Caja
{
    public int Id_caja { get; set; }
    public int Id_sucursal { get; set; }
    public int? Id_usuario_apertura { get; set; }
    public int? Id_usuario_cierre { get; set; }
    public string Codigo_caja { get; set; } = string.Empty;
    public string Estado { get; set; } = "Cerrada";
    public decimal Saldo_inicial { get; set; }
    public decimal Saldo_final { get; set; }
    public string? Tipo_movimiento { get; set; }
    public DateTime? Ultima_apertura { get; set; }
    public DateTime? Ultimo_cierre { get; set; }
    public bool Activa { get; set; } = true;
    public Sucursal Sucursal { get; set; } = null!;
    public Usuario? UsuarioApertura { get; set; }
    public Usuario? UsuarioCierre { get; set; }
}
