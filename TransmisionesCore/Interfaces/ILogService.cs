using System.Threading.Tasks;

namespace TransmisionesCore.Interfaces
{
    public interface ILogService
    {
       
        Task RegistrarLogAsync(string accion, string tabla, string detalle, int? idUsuario = null);
    }
}