// ============================================================
// TransmisionesCaja / Services / ApiService.cs
// ============================================================
using System.Net.Http.Json;

namespace TransmisionesCaja.Services;

// ---- DTOs de respuesta ----
public class EmpleadoDto
{
    public int Id_empleado { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public int Id_usuario { get; set; }
    public string Rol { get; set; } = string.Empty;
    public int Id_sucursal { get; set; }
    public bool Activo { get; set; }
    public SucursalInfoDto? Sucursal { get; set; }
    public string NombreSucursal => Sucursal?.Nombre_sucursal ?? "—";
}

public class SucursalInfoDto
{
    public int Id_sucursal { get; set; }
    public string Nombre_sucursal { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activa { get; set; }
    public int Id_municipio { get; set; }
}

public class SucursalDto
{
    public int Id_sucursal { get; set; }
    public string Nombre_sucursal { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activa { get; set; }
    public int Id_municipio { get; set; }
}

public class ServicioDto
{
    public int Id_servicio { get; set; }
    public string Nombre_servicio { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio_base { get; set; }
    public string TipoServicio { get; set; } = string.Empty;
    public int Id_tipo_servicio { get; set; }
    public bool Activo { get; set; }
}

public class UsuarioDto
{
    public int Id_usuario { get; set; }
    public string Nombre_usuario { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class CatalogoItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CatalogosDto
{
    public List<ProvinciaDto> Provincias { get; set; } = new();
    public List<MunicipioDto> Municipios { get; set; } = new();
    public List<SectorDto> Sectores { get; set; } = new();
    public List<TipoServicioDto> TiposServicio { get; set; } = new();
    public List<SucursalDto> Sucursales { get; set; } = new();
}

public class ProvinciaDto { public int Id_provincia { get; set; } public string Nombre_provincia { get; set; } = string.Empty; }
public class MunicipioDto { public int Id_municipio { get; set; } public string Nombre_municipio { get; set; } = string.Empty; public int Id_provincia { get; set; } }
public class SectorDto { public int Id_sector { get; set; } public string Nombre_sector { get; set; } = string.Empty; public int Id_municipio { get; set; } }
public class TipoServicioDto { public int Id_tipo_servicio { get; set; } public string Descripcion { get; set; } = string.Empty; }

// ✅ ProductoDto con objetos anidados en lugar de strings
public class ProductoDto
{
    public int Id_producto { get; set; }
    public string Descripcion_producto { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public decimal Precio_unitario { get; set; }
    public decimal Costo_unitario { get; set; }
    public int Stock_actual { get; set; }
    public bool Activo { get; set; }

    // ✅ Objetos anidados como los devuelve la API
    public CategoriaInfoDto? Categoria { get; set; }
    public TipoTransInfoDto? TipoTransmision { get; set; }

    // ✅ Propiedades auxiliares para mostrar en la tabla
    public string NombreCategoria => Categoria?.Nombre_categoria ?? "—";
    public string NombreTipoTrans => TipoTransmision?.Descripcion ?? "—";
}

public class CategoriaInfoDto
{
    public int Id_categoria { get; set; }
    public string Nombre_categoria { get; set; } = string.Empty;
}

public class TipoTransInfoDto
{
    public int Id_tipo_trans { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

public class CategoriaDto { public int Id_categoria { get; set; } public string Nombre_categoria { get; set; } = string.Empty; }
public class TipoTransDto { public int Id_tipo_trans { get; set; } public string Descripcion { get; set; } = string.Empty; }

// ---- Servicio principal ----
public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http) => _http = http;

    // ==================== EMPLEADOS ====================

    public async Task<List<EmpleadoDto>> GetEmpleadosAsync(int? idSucursal = null)
    {
        var url = idSucursal.HasValue
            ? $"api/empleados?idSucursal={idSucursal}"
            : "api/empleados";
        return await _http.GetFromJsonAsync<List<EmpleadoDto>>(url) ?? new();
    }

    public async Task<EmpleadoDto?> GetEmpleadoAsync(int id)
        => await _http.GetFromJsonAsync<EmpleadoDto>($"api/empleados/{id}");

    public async Task<bool> CrearEmpleadoAsync(object req)
    {
        var res = await _http.PostAsJsonAsync("api/empleados", req);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> ActualizarEmpleadoAsync(int id, object req)
    {
        var res = await _http.PutAsJsonAsync($"api/empleados/{id}", req);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DesactivarEmpleadoAsync(int id, bool reactivar = false)
    {
        var res = await _http.PutAsJsonAsync($"api/empleados/{id}/estado", new { Activo = reactivar });
        return res.IsSuccessStatusCode;
    }

    // ==================== SUCURSALES ====================

    public async Task<List<SucursalDto>> GetSucursalesAsync()
        => await _http.GetFromJsonAsync<List<SucursalDto>>("api/sucursales") ?? new();

    public async Task<bool> CrearSucursalAsync(object req)
    {
        var res = await _http.PostAsJsonAsync("api/sucursales", req);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> ActualizarSucursalAsync(int id, object req)
    {
        var res = await _http.PutAsJsonAsync($"api/sucursales/{id}", req);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> CambiarEstadoSucursalAsync(int id, bool activo)
    {
        // Enviamos el nuevo estado a la API
        var res = await _http.PutAsJsonAsync($"api/sucursales/{id}/estado", new { Activa = activo });
        return res.IsSuccessStatusCode;
    }

    // ==================== SERVICIOS ====================

    public async Task<List<ServicioDto>> GetServiciosAsync()
        => await _http.GetFromJsonAsync<List<ServicioDto>>("api/servicios") ?? new();

    public async Task<bool> CrearServicioAsync(object req)
    {
        var res = await _http.PostAsJsonAsync("api/servicios", req);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> ActualizarServicioAsync(int id, object req)
    {
        var res = await _http.PutAsJsonAsync($"api/servicios/{id}", req);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> CambiarEstadoServicioAsync(int id, bool activo)
    {
        var res = await _http.PutAsJsonAsync($"api/servicios/{id}/estado", new { Activo = activo });
        return res.IsSuccessStatusCode;
    }

    // ==================== USUARIOS ====================

    public async Task<List<UsuarioDto>> GetUsuariosAsync()
        => await _http.GetFromJsonAsync<List<UsuarioDto>>("api/usuarios") ?? new();

    public async Task<List<UsuarioDto>> GetUsuariosDisponiblesAsync()
        => await _http.GetFromJsonAsync<List<UsuarioDto>>("api/usuarios?soloDisponibles=true") ?? new();

    public async Task<bool> CrearUsuarioAsync(object req)
    {
        var res = await _http.PostAsJsonAsync("api/usuarios", req);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> ActualizarUsuarioAsync(int id, object req)
    {
        var res = await _http.PutAsJsonAsync($"api/usuarios/{id}", req);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> EliminarUsuarioAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/usuarios/{id}");
        return res.IsSuccessStatusCode;
    }

    // ==================== CATALOGOS ====================

    public async Task<List<ProvinciaDto>> GetProvinciasAsync()
        => await _http.GetFromJsonAsync<List<ProvinciaDto>>("api/catalogos/provincias") ?? new();

    public async Task<List<MunicipioDto>> GetMunicipiosAsync(int idProvincia)
        => await _http.GetFromJsonAsync<List<MunicipioDto>>($"api/catalogos/municipios/{idProvincia}") ?? new();

    public async Task<CatalogosDto> GetCatalogosAsync()
    {
        var catalogos = new CatalogosDto
        {
            Provincias = await _http.GetFromJsonAsync<List<ProvinciaDto>>("api/catalogos/provincias") ?? new(),
            Municipios = new(),
            Sectores = new(),
            TiposServicio = await _http.GetFromJsonAsync<List<TipoServicioDto>>("api/catalogos/tipos-servicio") ?? new(),
            Sucursales = await GetSucursalesAsync()
        };
        return catalogos;
    }

    // ==================== PRODUCTOS ====================

    public async Task<List<ProductoDto>> GetProductosAsync()
        => await _http.GetFromJsonAsync<List<ProductoDto>>("api/productos") ?? new();

    public async Task<bool> CrearProductoAsync(object req)
    {
        var res = await _http.PostAsJsonAsync("api/productos", req);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> ActualizarPrecioProductoAsync(int id, decimal precio, decimal costo)
    {
        var res = await _http.PatchAsync($"api/productos/{id}/precio?nuevoPrecio={precio}&nuevoCosto={costo}", null);
        return res.IsSuccessStatusCode;
    }

    public async Task<List<CategoriaDto>> GetCategoriasAsync()
        => await _http.GetFromJsonAsync<List<CategoriaDto>>("api/catalogos/categorias") ?? new();

    public async Task<List<TipoTransDto>> GetTiposTransmisionAsync()
        => await _http.GetFromJsonAsync<List<TipoTransDto>>("api/catalogos/tipos-transmision") ?? new();
}