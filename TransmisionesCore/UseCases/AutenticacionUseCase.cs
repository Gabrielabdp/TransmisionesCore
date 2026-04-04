using TransmisionesCore.Entities;
using TransmisionesCore.Exceptions;
using TransmisionesCore.Interfaces;


namespace TransmisionesCore.UseCases;

public class AutenticacionUseCase
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IEmpleadoRepository _empleadoRepo;

    public AutenticacionUseCase(IUsuarioRepository usuarioRepo, IEmpleadoRepository empleadoRepo)
    {
        _usuarioRepo = usuarioRepo;
        _empleadoRepo = empleadoRepo;
    }

    public async Task<object?> ValidarAccesoAsync(string nombreUsuario, string contrasena)
    {
        // 1. Validamos las credenciales en la tabla de Usuarios
        var usuario = await _usuarioRepo.LoginAsync(nombreUsuario, contrasena);

        if (usuario == null) return null;

        // 2. Buscamos al empleado vinculado para saber quién es y en qué sucursal está
        var empleados = await _empleadoRepo.ObtenerTodosAsync();
        var empleado = empleados.FirstOrDefault(e => e.Id_usuario == usuario.Id_usuario);

        // 3. Devolvemos un objeto anónimo con lo necesario para la sesión en tu App
        return new
        {
            Id_usuario = usuario.Id_usuario,
            Id_empleado = empleado?.Id_empleado,
            NombreCompleto = empleado?.NombreCompleto ?? usuario.Nombre_usuario,
            Rol = usuario.Rol,
            Id_sucursal = empleado?.Id_sucursal,
            Sucursal = empleado?.Sucursal?.Nombre_sucursal
        };
    }
}