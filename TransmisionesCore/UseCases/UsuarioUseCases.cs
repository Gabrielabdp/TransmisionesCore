using System;
using System.Threading.Tasks;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases;

public class UsuarioUseCases
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IClienteRepository _clienteRepo;

    public UsuarioUseCases(IUsuarioRepository usuarioRepo, IClienteRepository clienteRepo)
    {
        _usuarioRepo = usuarioRepo;
        _clienteRepo = clienteRepo;
    }

    public async Task<Usuario?> LoginAsync(string email, string contrasena)
    {
        return await _usuarioRepo.LoginAsync(email, contrasena);
    }

    public async Task<Usuario> RegistrarClienteAsync(string nombre, string apellido, string email, string contrasena)
    {
        // 1. Validar que el correo sea único
        var existente = await _usuarioRepo.ObtenerPorEmailAsync(email);
        if (existente != null)
        {
            throw new Exception("El correo electrónico ya está registrado.");
        }

        // 2. Crear el Usuario en la base de datos (usamos Nombre_usuario para guardar el email)
        var nuevoUsuario = new Usuario
        {
            Nombre_usuario = email, 
            Contrasena = contrasena,
            Rol = "Cliente",
            Activo = true
        };

        var usuarioGuardado = await _usuarioRepo.InsertarAsync(nuevoUsuario);

        // 3. Crear el Perfil de Cliente vinculado (asociado por correo)
        var nuevoCliente = new Cliente
        {
            Nombre_cliente = nombre,
            Apellido_cliente = apellido,
            Correo_cliente = email,
            Id_provincia = 1, // Por defecto (Santo Domingo)
            Id_municipio = 1,
            Id_sector = 1
        };

        await _clienteRepo.InsertarAsync(nuevoCliente);

        return usuarioGuardado;
    }
}
