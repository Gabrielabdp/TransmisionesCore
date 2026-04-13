using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TransmisionesAPI;
using TransmisionesCore.Interfaces;
using TransmisionesCore.UseCases;
using TransmisionesInfraestructura.Data;
using TransmisionesInfraestructura.Repositories;
using TransmisionesInfraestructura.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ILogService, LogService>();

// Agrega sercicios al contenerdor.
builder.Services.AddControllers().AddJsonOptions(options =>
 {
     // Esto corta el ciclo infinito en la serialización
     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
 });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<TransmisionesContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => {
        // Esto evita que la app se rinda si Azure tarda en responder
        sqlOptions.CommandTimeout(60);
    }));

// Repositories
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IOrdenRepository, OrdenRepository>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<ICajaRepository, CajaRepository>();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IVehiculoRepository, VehiculoRepository>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();


// Use Cases
builder.Services.AddScoped<AutenticacionUseCase>();
builder.Services.AddScoped<OrdenUseCases>();
builder.Services.AddScoped<ClienteUseCases>();
builder.Services.AddScoped<ProductoUseCases>();
builder.Services.AddScoped<CajaUseCases>();
builder.Services.AddScoped<FacturaUseCases>();
builder.Services.AddScoped<VehiculoUseCases>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
// Registrar el Repositorio
builder.Services.AddScoped<ILogRepository, LogRepository>();

// Registrar el Caso de Uso (Este es el que te falta según el error)
builder.Services.AddScoped<AuditoriaUseCases>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Test Endpoint
// Test Endpoint para validar Relaciones del Core
app.MapPruebasDb(); // <--- Solo esta línea limpia

app.Run();


