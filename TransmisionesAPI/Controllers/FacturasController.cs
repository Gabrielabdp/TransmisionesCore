using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.UseCases;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly FacturaUseCases _useCases;
    public FacturasController(FacturaUseCases useCases) => _useCases = useCases;

    [HttpPost]
    public async Task<IActionResult> Generar(int idOrden, int idEmpleado, int idCaja, string tipoFactura = "Consumidor_Final")
        => Ok(await _useCases.GenerarFacturaAsync(idOrden, idEmpleado, idCaja, tipoFactura));
}
