namespace TransmisionesWeb.Services;

public class SessionStateService
{
    // Portal cliente
    public int?    ClienteId      { get; set; }
    public string? ClienteNombre  { get; set; }
    public string? ClienteEmail   { get; set; }

    // Terminal de caja
    public int?    EmpleadoId     { get; set; }
    public string? EmpleadoNombre { get; set; }
    public string? EmpleadoRol    { get; set; }
    public int?    CajaId         { get; set; }

    public bool EsClienteAutenticado  => ClienteId.HasValue;
    public bool EsEmpleadoAutenticado => EmpleadoId.HasValue;

    public void CerrarSesionCliente()
    {
        ClienteId     = null;
        ClienteNombre = null;
        ClienteEmail  = null;
    }

    public void CerrarSesionEmpleado()
    {
        EmpleadoId     = null;
        EmpleadoNombre = null;
        EmpleadoRol    = null;
        CajaId         = null;
    }
}
