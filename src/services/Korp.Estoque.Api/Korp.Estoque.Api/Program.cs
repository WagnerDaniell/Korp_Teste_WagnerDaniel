using Korp.Estoque.Api.Middleware;
using Korp.Estoque.Application.UseCases.Produtos;
using Korp.Estoque.Domain.Repositories;
using Korp.Estoque.Infrastructure.Context;
using Korp.Estoque.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptionsAction => { 
        npgsqlOptionsAction.MigrationsAssembly("Korp.Estoque.Infrastructure");
        npgsqlOptionsAction.EnableRetryOnFailure(3);
        npgsqlOptionsAction.CommandTimeout(60);


    }) //A DefaultConnection está nas vriáveis de ambiente do docker-compose

);

builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<ITransactionExecutor, EfTransactionExecutor>();

builder.Services.AddScoped<CreateProdutoUseCase>();
builder.Services.AddScoped<ReadProdutoUseCase>();
builder.Services.AddScoped<UpdateProdutoUseCase>();
builder.Services.AddScoped<DeleteProdutoUseCase>();
builder.Services.AddScoped<BaixaEstoqueUseCase>();
builder.Services.AddScoped<ValidarProdutosUseCase>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.AllowAnyOrigin() // Em prod, IP do Angular
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Estoque API", Version = "v1" });

    // Adicionar configuração para o header de autenticação interna
    c.AddSecurityDefinition("InternalSecret", new OpenApiSecurityScheme
    {
        Name = "X-Internal-Secret",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Secret para comunicação interna entre serviços",
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "InternalSecret"
                }
            },
            Array.Empty<string>()
        }
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

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();