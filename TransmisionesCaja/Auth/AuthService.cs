using System.Net.Http.Json;

namespace TransmisionesCaja.Auth;

public class UsuarioSesion
{
    public int Id_usuario { get; set; }
    public int? Id_empleado { get; set; }
    public int? Id_sucursal { get; set; }
    public string Nombre_usuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Sucursal { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}

public class AuthService
{
    private readonly HttpClient _http;
    public UsuarioSesion? UsuarioActual { get; private set; }
    public bool EstaAutenticado => UsuarioActual != null;
    public event Action? OnCambioEstado;

    public AuthService(HttpClient http) => _http = http;

    public async Task<bool> LoginAsync(string nombreUsuario, string contrasena)
    {
        try
        {
            Console.WriteLine($"--> Intentando login: {nombreUsuario}");

            var response = await _http.PostAsJsonAsync("api/autenticacion/login", new
            {
                Usuario = nombreUsuario,
                Password = contrasena
            });

            Console.WriteLine($"--> Respuesta HTTP: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"--> Error respuesta: {error}");
                return false;
            }

            var resultado = await response.Content.ReadFromJsonAsync<UsuarioSesion>();
            Console.WriteLine($"--> Usuario recibido: {resultado?.Nombre_usuario} - Rol: {resultado?.Rol}");

            if (resultado == null) return false;

            UsuarioActual = resultado;
            OnCambioEstado?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> EXCEPCION: {ex.Message}");
            return false;
        }
    }

    public void Logout()
    {
        UsuarioActual = null;
        OnCambioEstado?.Invoke();
    }

    public bool EsAdmin()   => UsuarioActual?.Rol == "Admin";
    public bool EsCajero()  => UsuarioActual?.Rol == "Cajero";
    public bool EsTecnico() => UsuarioActual?.Rol == "Tecnico";
    public bool TieneRol(string rol) => UsuarioActual?.Rol == rol;
}
