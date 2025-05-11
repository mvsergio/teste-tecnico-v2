using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using Thunders.TechTest.ApiService.Data;
using Thunders.TechTest.ApiService.Handlers;
using Thunders.TechTest.ApiService.Models;
using Thunders.TechTest.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao contêiner
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<PedagioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

builder.Services.AddOpenTelemetry().WithTracing(builder =>
{
    builder
        .AddSource("PedagioService")
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ApiService"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter();
});

builder.Services.AddRebus(configurer =>
    configurer
        .Transport(t => t.UseRabbitMq("amqp://localhost", "utilizacoes"))
        .Routing(r => r.TypeBased().Map<Utilizacao>("utilizacoes"))
);

builder.Services.AddTransient<PedagioService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiService", Version = "v1" });
});

var app = builder.Build();

// Configurar o pipeline de requisições
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiService v1"));
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Services.StartRebus();

app.Run();