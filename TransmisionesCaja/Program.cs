using TransmisionesCaja.Auth;
using TransmisionesCaja.Components;
using TransmisionesCaja.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()        // ✅ faltaba esta línea
    .AddInteractiveServerComponents();

builder.Services.AddAntiforgery();

var apiUrl = builder.Configuration["ApiUrl"] ?? "https://localhost:56678/";

builder.Services.AddHttpClient(nameof(AuthService), client =>
{
    client.BaseAddress = new Uri(apiUrl);
});
builder.Services.AddSingleton<AuthService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var http = factory.CreateClient(nameof(AuthService));
    return new AuthService(http);
});

builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();