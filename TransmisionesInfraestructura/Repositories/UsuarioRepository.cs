using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesInfraestructura.Data;

namespace TransmisionesInfraestructura.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly TransmisionesContext _context;
    public UsuarioRepository(TransmisionesContext context) => _context = context;

    public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        => await _context.Usuarios.ToListAsync();

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
        => await _context.Usuarios.FindAsync(id);

    public async Task<Usuario?> LoginAsync(string nombreUsuario, string contrasena)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u =>
            u.Nombre_usuario == nombreUsuario && u.Activo);

        if (usuario == null) return null;

        return BCrypt.Net.BCrypt.Verify(contrasena, usuario.Contrasena)
            ? usuario
            : null;
    }

    public async Task<Usuario> InsertarAsync(Usuario usuario)
    {
        usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }
}