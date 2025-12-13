using MindHub.Domain.Models;
using MindHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configura o Banco de Dados (TEM QUE SER ANTES DO BUILD)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// 2. Adiciona os Controllers
builder.Services.AddControllers();

// 3. Adiciona o Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- TUDO ABAIXO DAQUI É CONFIGURAÇÃO DE COMO O APP RODA ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();