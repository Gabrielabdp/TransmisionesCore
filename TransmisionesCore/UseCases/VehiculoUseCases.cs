using System.ComponentModel.DataAnnotations;
using TransmisionesCore.Entities;
using TransmisionesCore.Exceptions;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases;

public class VehiculoUseCases
{
    private readonly IVehiculoRepository _repo;
    private readonly IOrdenRepository _ordenRepo;

    public VehiculoUseCases(IVehiculoRepository repo, IOrdenRepository ordenRepo)
    {
        _repo = repo;
        _ordenRepo = ordenRepo;
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

    public async Task<HistorialVehiculoDTO?> ObtenerHistorial(string matricula)
    {
        return await _repo.ObtenerHistorialPorMatriculaAsync(matricula);
    }
        public async Task<GarantiaEstadoDTO> VerificarGarantiaAsync(int idVehiculo)
    {
       
        var ordenes = await _ordenRepo.ObtenerTodosAsync();
        var ultimaOrden = ordenes
            .Where(o => o.Id_vehiculo == idVehiculo.ToString() && o.Estado_orden == "Facturada")
            .OrderByDescending(o => o.Fecha_orden)
            .FirstOrDefault();

        if (ultimaOrden == null)
        {
            return new GarantiaEstadoDTO(false, null, "No se encontraron reparaciones previas registradas.", null);
        }

        DateTime fechaVencimiento = ultimaOrden.Fecha_orden.AddMonths(6);
        bool esValida = DateTime.Now <= fechaVencimiento;

        return new GarantiaEstadoDTO(
            TieneGarantiaActiva: esValida,
            FechaVencimiento: fechaVencimiento,
            Mensaje: esValida ? "El vehículo cuenta con garantía vigente." : "La garantía ha expirado.",
            FolioOrdenOriginal: $"ORD-{ultimaOrden.Id_orden}"
        );
    }

}