
using Microsoft.EntityFrameworkCore;
using TransmisionesIntegracion.Data;
using TransmisionesIntegracion.Services;
using TransmisionesIntegracion.Middlewares;
namespace TransmisionesIntegracion
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHttpClient();

            builder.Services.AddDbContext<IntegracionDbContext>(opciones =>
                opciones.UseSqlite("Data Source=integracion_local.db"));

            builder.Services.AddHostedService<SincronizadorBackgroundService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseMiddleware<AuditoriaMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
