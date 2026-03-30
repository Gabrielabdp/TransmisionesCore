namespace TransmisionesCore.Entities;
public class Usuario
{
    public int Id_usuario { get; set; }
    public string Nombre_usuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
