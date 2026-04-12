using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.UseCases;

namespace TransmisionesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditoriaController : ControllerBase
    {
        private readonly AuditoriaUseCases _useCases;

        public AuditoriaController(AuditoriaUseCases useCases)
        {
            _useCases = useCases;
        }

        [HttpGet("precios")]
        public async Task<ActionResult<IEnumerable<AuditoriaPrecioDTO>>> GetPrecios()
        {
            try
            {
                var logs = await _useCases.ObtenerLogPreciosAsync();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener auditoría", error = ex.Message });
            }
        }
    }
}
