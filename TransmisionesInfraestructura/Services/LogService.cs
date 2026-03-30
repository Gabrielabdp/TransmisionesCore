using TransmisionesCore.Entities; // <--- Verifica que aquí esté tu entidad 'Log'
using TransmisionesCore.Interfaces;
using TransmisionesInfraestructura.Data;

namespace TransmisionesInfraestructura.Services
{
    public class LogService : ILogService
    {
        private readonly TransmisionesContext _context;

        public LogService(TransmisionesContext context)
        {
            _context = context;
        }

        public async Task RegistrarLogAsync(string accion, string tabla, string detalle, int? idUsuario = null)
        {
            var ahora = DateTime.Now;

            // Creamos el objeto con tus columnas reales
            var entradaLog = new Log
            {
                Id_usuario = idUsuario ?? 1,      // Columna: Id_usuario
                Accion = accion,                  // Columna: Accion
                Tabla_afectada = tabla,           // Columna: Tabla_afectada
                Detalle = detalle,                // Columna: Detalle
                Fecha = ahora,                    // Columna: Fecha (Tipo Date)
                Hora = ahora.TimeOfDay            // Columna: Hora (Tipo Time)
            };

            _context.Logs.Add(entradaLog);
            await _context.SaveChangesAsync();
        }
    }
}