using TransmisionesCore.Entities;
using TransmisionesCore.Exceptions;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases;

public class ClienteUseCases
{
    private readonly IClienteRepository _repo;
    public ClienteUseCases(IClienteRepository repo) => _repo = repo;

    public async Task<Cliente> RegistrarClienteAsync(CrearClienteRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NombreCliente))
            throw new DomainException("El nombre del cliente es requerido.");

        var cliente = new Cliente
        {
            Id_sector        = req.IdSector,
            Id_municipio     = req.IdMunicipio,
            Id_provincia     = req.IdProvincia,
            Nombre_cliente   = req.NombreCliente.Trim(),
            Apellido_cliente = req.ApellidoCliente.Trim(),
            RNC_cliente      = req.RNC,
            Cedula_cliente   = req.Cedula,
            Telefono_cliente = req.Telefono,
            Correo_cliente   = req.Correo,
            Fecha_registro   = DateTime.UtcNow
        };

        return await _repo.InsertarAsync(cliente);
    }

    public async Task<IEnumerable<Cliente>> BuscarAsync(string? buscar = null)
        => await _repo.ObtenerTodosAsync(buscar);

    public async Task<Cliente> ObtenerPorIdAsync(int id)
        => await _repo.ObtenerPorIdAsync(id)
            ?? throw new EntidadNoEncontradaException("Cliente", id);
}
