namespace TransmisionesCore.Entities;
public class Log
{
    public int Id_log { get; set; }
    public int Id_usuario { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string? Tabla_afectada { get; set; }
    public string? Detalle { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public Usuario Usuario { get; set; } = null!;

    public TimeSpan Hora { get; set; }
}
