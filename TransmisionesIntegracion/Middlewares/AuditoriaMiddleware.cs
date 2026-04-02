using TransmisionesIntegracion.Data;
using TransmisionesIntegracion.Models;

namespace TransmisionesIntegracion.Middlewares
{
    public class AuditoriaMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditoriaMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Este método intercepta TODO lo que entra a tu API
        public async Task InvokeAsync(HttpContext context, IntegracionDbContext dbContext)
        {
            // 1. Interceptar la Petición (Lo que entra)
            context.Request.EnableBuffering(); // Nos permite leer el archivo sin borrarlo
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0; // Rebobinamos para que el Controller lo pueda leer después

            // Preparamos una trampa para atrapar la respuesta (Lo que sale)
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // 2. Dejar que la petición pase a tus Controllers (Cajas, Productos, etc.)
            await _next(context);

            // 3. Interceptar la Respuesta (Lo que tu Controller devolvió)
            context.Response.Body.Position = 0;
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Position = 0;

            // Devolvemos la respuesta al cauce normal para que llegue a la Caja/Web
            await responseBodyStream.CopyToAsync(originalBodyStream);

            // 4. GUARDAR LA AUDITORÍA
            // Ignoramos las llamadas al Swagger para no llenar la base de datos de basura
            var requestPath = context.Request.Path.Value ?? string.Empty;
            if (!requestPath.Contains("swagger", StringComparison.OrdinalIgnoreCase))
            {
                var log = new LogTrafico
                {
                    FechaHora = DateTime.Now,
                    MetodoHttp = context.Request.Method,
                    Endpoint = context.Request.Path,
                    PeticionBody = requestBody,
                    StatusCode = context.Response.StatusCode,
                    // Si la respuesta es gigantesca (como el catálogo entero), guardamos solo un pedazo para no saturar SQLite
                    RespuestaBody = responseBody.Length > 1000 ? responseBody.Substring(0, 1000) + "...[Truncado]" : responseBody,
                    OrigenIP = context.Connection.RemoteIpAddress?.ToString() ?? "Desconocido"
                };

                dbContext.LogsTrafico.Add(log);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}