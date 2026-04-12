using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransmisionesCore.Entities; 
using TransmisionesCore.UseCases;
using TransmisionesInfraestructura.Data;
using TransmisionesCore.Interfaces;

namespace TransmisionesInfraestructura.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly TransmisionesContext _context;

        public LogRepository(TransmisionesContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuditoriaPrecioDTO>> ObtenerLogPreciosAsync()
        {
            return await _context.Logs
                .Include(l => l.Usuario)
                .Where(l => l.Tabla_afectada == "Producto" && l.Detalle.Contains("Precio"))
                .OrderByDescending(l => l.Fecha)
                .Select(l => new AuditoriaPrecioDTO
                {
                    Fecha = l.Fecha,
                    Hora = l.Hora,
                    Usuario = l.Usuario.Nombre_usuario,
                    Accion = l.Accion,
                    Detalle = l.Detalle
                })
                .ToListAsync();
        }

        public async Task<bool> RegistrarLogAsync(Log log)
        {
            _context.Logs.Add(log);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}