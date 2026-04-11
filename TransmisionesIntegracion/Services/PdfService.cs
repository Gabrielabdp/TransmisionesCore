using System;
using System.IO;
using System.Threading.Tasks;
using TransmisionesCore.Entities;

namespace TransmisionesIntegracion.Services;

public class PdfService
{
    public async Task<byte[]> GenerarFacturaPdfAsync(Factura factura, string nombreCliente)
    {
        // En una implementación real, aquí usaríamos QuestPDF o iTextSharp.
        // Por ahora, simulamos la generación del PDF como un byte array.
        // Pero el sistema ya queda listo para recibir el flujo de bytes.

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms);
        
        await writer.WriteLineAsync("===========================================");
        await writer.WriteLineAsync("        TRANSMISIONES MAG - FACTURA        ");
        await writer.WriteLineAsync("===========================================");
        await writer.WriteLineAsync($"Factura Nro: {factura.Numero_factura}");
        await writer.WriteLineAsync($"Fecha: {factura.Fecha_factura:dd/MM/yyyy HH:mm}");
        await writer.WriteLineAsync($"Cliente: {nombreCliente}");
        await writer.WriteLineAsync("-------------------------------------------");
        await writer.WriteLineAsync($"Subtotal: RD$ {factura.SubTotal:N2}");
        await writer.WriteLineAsync($"ITBIS (18%): RD$ {factura.ITBIS:N2}");
        await writer.WriteLineAsync($"TOTAL PAGADO: RD$ {factura.Total:N2}");
        await writer.WriteLineAsync("===========================================");
        await writer.WriteLineAsync("      GRACIAS POR SU PREFERENCIA           ");
        
        await writer.FlushAsync();
        return ms.ToArray();
    }
}
