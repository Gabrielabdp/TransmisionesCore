using System;

namespace TransmisionesCore.Entities;

public class MovimientoCaja
{
    public int Id_movimiento { get; set; }
    public int Id_caja { get; set; }
    public int Id_usuario { get; set; }
    public decimal Monto { get; set; }
    public string Tipo { get; set; } = "Entrada"; // "Entrada" o "Salida"
    public string Motivo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    // Relaciones
    public Caja Caja { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
