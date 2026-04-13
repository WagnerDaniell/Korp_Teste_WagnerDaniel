using Korp.Faturamento.Api.Middleware;
using Korp.Faturamento.Application.Services;
using Korp.Faturamento.Application.UseCases.NotasFiscais;
using Korp.Faturamento.Domain.Repositories;
using Korp.Faturamento.Infrastructure.Clients;
using Korp.Faturamento.Infrastructure.Context;
using Korp.Faturamento.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FaturamentoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("Korp.Faturamento.Infrastructure");
        npgsqlOptions.EnableRetryOnFailure(3);
        npgsqlOptions.CommandTimeout(60);
    })
);

builder.Services.AddScoped<INotaFiscalRepository, NotaFiscalRepository>();

builder.Services.AddScoped<CreateNotaUseCase>();
builder.Services.AddScoped<ReadNotaUseCase>();
builder.Services.AddScoped<ImprimirNotaUseCase>();

builder.Services.AddHttpClient<IEstoqueService, EstoqueServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:EstoqueApi"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultPolicy");

//app.UseAuthorization();

app.MapControllers();

app.Run();