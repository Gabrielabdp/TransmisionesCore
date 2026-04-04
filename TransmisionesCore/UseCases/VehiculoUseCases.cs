using System.ComponentModel.DataAnnotations;
using TransmisionesCore.Entities;
using TransmisionesCore.Exceptions;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases;

public class VehiculoUseCases
{
    private readonly IVehiculoRepository _repo;

    public VehiculoUseCases(IVehiculoRepository repo)
    {
        _repo = repo;
    }

    public async Task<Vehiculo> RegistrarVehiculoAsync(Vehiculo vehiculo)
    {
        //aca valdimos si la placa existe o no
        return await _repo.InsertarAsync(vehiculo);
    }

    public async Task<IEnumerable<Vehiculo>> ObtenerPorClienteAsync(int idCliente)
    {
        return await _repo.ObtenerPorClienteAsync(idCliente);
    }

    public async Task<Vehiculo?> ObtenerPorMatriculaAsync(string matricula)
    {

        return await _repo.ObtenerPorMatriculaAsync(matricula);
    }
}