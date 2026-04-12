using TransmisionesCore.Entities;
using TransmisionesCore.Exceptions;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases;

public class ClienteUseCases
{
    private readonly IClienteRepository _repo;
    private readonly IOrdenRepository _ordenRepo;
    public ClienteUseCases(IClienteRepository repo, IOrdenRepository ordenRepo)
    {
        _repo = repo;
        _ordenRepo = ordenRepo;
    }

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

    public async Task<ClienteBusquedaDTO?> BuscarPorDocumento(string documento)
    {
        var docABuscar = documento.Trim();

        var cliente = await _repo.ObtenerPorDocumentoAsync(docABuscar);

        if (cliente == null) return null;

        return new ClienteBusquedaDTO(
            IdCliente: cliente.Id_cliente,
            NombreCompleto: $"{cliente.Nombre_cliente} {cliente.Apellido_cliente}",
            Documento: !string.IsNullOrEmpty(cliente.Cedula_cliente) ? cliente.Cedula_cliente : (cliente.RNC_cliente ?? "N/A"),
            Telefono: cliente.Telefono_cliente ?? "Sin Teléfono",
            Correo: cliente.Correo_cliente
        );
    }

    public async Task<ClienteResumenDTO?> ObtenerResumenAsync(int id)
    {
        // El UseCase simplemente le pide al Repositorio que haga el trabajo sucio
        return await _repo.ObtenerResumenAsync(id);
    }

    public async Task<ResumenFlotaDTO> ObtenerResumenFlotaAsync(int idCliente)
    {
        // 4. CAMBIA _repo por _ordenRepo aquí
        // Asegúrate de que el método se llame ObtenerTodosAsync (en plural) o como esté en tu interfaz
        var ordenes = await _ordenRepo.ObtenerTodosAsync();

        var ordenesCliente = ordenes.Where(o => o.Id_cliente == idCliente).ToList();

        var distribucion = ordenesCliente
            .GroupBy(o => o.Estado_orden)
            .Select(g => new EstadoVehiculoCount(g.Key ?? "Pendiente", g.Count()))
            .ToList();

        return new ResumenFlotaDTO(
            TotalVehiculos: ordenesCliente.Select(o => o.Id_vehiculo).Distinct().Count(),
            DistribucionEstados: distribucion
        );
    }

}


