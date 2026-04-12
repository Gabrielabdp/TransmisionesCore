using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TransmisionesCore.Services;

public class EmailService
{
    private readonly string _host = "smtp.gmail.com";
    private readonly int _port = 587;
    private readonly string _user = "anprogram1@gmail.com"; // Reemplazar con credenciales reales
    private readonly string _pass = "csnzhojitpgnsiqc"; // Reemplazar con credenciales reales

    public async Task EnviarFacturaRealAsync(string emailDestino, string numeroFactura, byte[] pdfAdjunto)
    {
        try
        {
            using var client = new SmtpClient(_host, _port);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(_user, _pass);

            var message = new MailMessage(_user, emailDestino)
            {
                Subject = $"Factura Transmisiones MAG - {numeroFactura}",
                Body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; overflow: hidden;'>
                    <div style='background-color: #f8c102; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0; color: #000;'>TRANSMISIONES MAG</h1>
                    </div>
                    <div style='padding: 30px;'>
                        <h2 style='color: #333;'>¡Gracias por su compra!</h2>
                        <p style='color: #666; font-size: 16px; line-height: 1.5;'>
                            Hola, adjunto encontrará su factura oficial correspondiente a la transacción <strong>{numeroFactura}</strong>. 
                        </p>
                        <div style='background-color: #f9f9f9; padding: 20px; border-radius: 5px; margin-top: 20px;'>
                            <p style='margin: 5px 0;'><strong>Nro. Factura:</strong> {numeroFactura}</p>
                            <p style='margin: 5px 0;'><strong>Estado:</strong> Pagada</p>
                        </div>
                        <p style='color: #666; font-size: 14px; margin-top: 30px; text-align: center;'>
                            Si tiene alguna duda sobre su factura, por favor contáctenos respondiendo a este correo.
                        </p>
                    </div>
                    <div style='background-color: #000; color: #fff; padding: 15px; text-align: center; font-size: 12px;'>
                        &copy; {DateTime.Now.Year} Transmisiones MAG, SRL. Todos los derechos reservados.
                    </div>
                </div>",
                IsBodyHtml = true
            };

            // Adjuntamos el PDF de la factura
            using var ms = new System.IO.MemoryStream(pdfAdjunto);
            message.Attachments.Add(new Attachment(ms, $"Factura_{numeroFactura}.pdf", "application/pdf"));

            await client.SendMailAsync(message);
            Console.WriteLine($"[EMAIL] Factura {numeroFactura} enviada a {emailDestino}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL ERROR] {ex.Message}");
            throw;
        }
    }
}
