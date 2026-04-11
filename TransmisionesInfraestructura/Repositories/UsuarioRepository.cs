using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesInfraestructura.Data;

namespace TransmisionesInfraestructura.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly TransmisionesContext _context;
    public UsuarioRepository(TransmisionesContext context) => _context = context;

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
        => await _context.Usuarios.FindAsync(id);

    public async Task<Usuario?> LoginAsync(string email, string contrasena)
        => await _context.Usuarios.FirstOrDefaultAsync(u => u.Nombre_usuario == email && u.Contrasena == contrasena && u.Activo);

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        => await _context.Usuarios.FirstOrDefaultAsync(u => u.Nombre_usuario == email);

    public async Task<Usuario> InsertarAsync(Usuario usuario)
    {
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
