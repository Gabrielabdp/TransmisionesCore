using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases
{
    public class AuditoriaUseCases
    {
        private readonly ILogRepository _logRepo;

        public AuditoriaUseCases(ILogRepository logRepo)
        {
            _logRepo = logRepo;
        }

        public async Task<IEnumerable<AuditoriaPrecioDTO>> ObtenerLogPreciosAsync()
        {
            return await _logRepo.ObtenerLogPreciosAsync();
        }
    }
}
