using System;
using System.IO;
using System.Threading.Tasks;
using TransmisionesCore.Entities;

namespace TransmisionesCore.Services;

public class PdfService
{
    public async Task<byte[]> GenerarFacturaPdfAsync(Factura factura, string nombreCliente)
    {
        using var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms))
        {
            await writer.WriteLineAsync("***************************************************");
            await writer.WriteLineAsync("              TRANSMISIONES MAG, SRL               ");
            await writer.WriteLineAsync("      Expertos en Transmisiones Automáticas        ");
            await writer.WriteLineAsync("       Santo Domingo, República Dominicana         ");
            await writer.WriteLineAsync("***************************************************");
            await writer.WriteLineAsync("");
            await writer.WriteLineAsync($" FACTURA OFICIAL:  {factura.Numero_factura}");
            await writer.WriteLineAsync($" FECHA DE EMISIÓN: {factura.Fecha_factura:dd/MM/yyyy HH:mm:ss}");
            await writer.WriteLineAsync($" CLIENTE:          {nombreCliente}");
            await writer.WriteLineAsync($" RNC/CÉDULA:       Consumidor Final");
            await writer.WriteLineAsync("");
            await writer.WriteLineAsync("---------------------------------------------------");
            await writer.WriteLineAsync(" DESCRIPCIÓN                  CANT.       TOTAL    ");
            await writer.WriteLineAsync("---------------------------------------------------");
            
            // Aquí en un futuro se iterarían los items de la orden real
            await writer.WriteLineAsync($" Servicio/Producto Genérico    1.00     RD$ {factura.SubTotal:N2}");
            
            await writer.WriteLineAsync("---------------------------------------------------");
            await writer.WriteLineAsync($" SUB-TOTAL:                       RD$ {factura.SubTotal:N2}");
            await writer.WriteLineAsync($" ITBIS (18%):                     RD$ {factura.ITBIS:N2}");
            await writer.WriteLineAsync("");
            await writer.WriteLineAsync($" TOTAL A PAGAR:                   RD$ {factura.Total:N2}");
            await writer.WriteLineAsync("---------------------------------------------------");
            await writer.WriteLineAsync("");
            await writer.WriteLineAsync(" MÉTODO DE PAGO: Efectivo / Tarjeta");
            await writer.WriteLineAsync("");
            await writer.WriteLineAsync("***************************************************");
            await writer.WriteLineAsync("      ¡GRACIAS POR CONFIAR EN NOSOTROS!            ");
            await writer.WriteLineAsync("    Garantía de 6 meses en toda reparación.        ");
            await writer.WriteLineAsync("***************************************************");
            await writer.FlushAsync();
        }
        return ms.ToArray();
    }
}
