namespace TransmisionesIntegracion.Models
{
    public class EmpleadoCache
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public bool Activo { get; set; }

        // Datos de autenticación (Se extraen de la relación Usuario en el CORE)
        public int? IdUsuario { get; set; }
        public string UsuarioAcceso { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}