using MindHub.Domain.Models;      // <--- Antes estava MindHub.Models
using MindHub.Infrastructure.Data; // <--- Antes estava MindHub.Data
using Microsoft.EntityFrameworkCore;
using MindHub.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configura o Banco de Dados 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// 2. Adiciona os Controllers 
builder.Services.AddControllers();

// 3. Adiciona o Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// REGISTRO DOS SERVICES 
builder.Services.AddScoped<GamificationService>();
builder.Services.AddScoped<DashboardService>(); 

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// --- INÍCIO DO DASHBOARD NO TERMINAL ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var dashboardService = services.GetRequiredService<DashboardService>();

        var userId = 1;
        var user = context.UserProfiles.Find(userId);
        
        if (user != null)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n========================================================");
            Console.WriteLine("          🚀 MINDHUB - RELATÓRIO DE STATUS BACKEND       ");
            Console.WriteLine("========================================================");
            Console.ResetColor();

            Console.WriteLine("\n[1] MÓDULOS IMPLEMENTADOS:");
            PrintStatus("Banco de Dados (SQLite)", true);
            PrintStatus("CRUD de Hábitos", true);
            PrintStatus("Registro de Check-ins", true);
            PrintStatus("Gamificação (XP e Medalhas)", true);
            PrintStatus("Dashboard (Gráficos e Stats)", true);
            PrintStatus("Filtro de Pausados (RF052)", true);

           
            var totalHabitos = context.Habits.Count(h => h.UserId == userId);
            var habitosPausados = context.Habits.Count(h => h.UserId == userId && h.IsPaused);
            var totalCheckins = context.CheckIns.Count(c => c.Habit!.UserId == userId);
            
            
            var nivelCalculado = (user.CurrentPoints / 100) + 1;

            Console.WriteLine("\n[2] DADOS ATUAIS (Usuário: " + user.Name + "):");
            Console.WriteLine($"   • Nível Atual: {nivelCalculado} ({user.CurrentPoints} XP)"); 
            Console.WriteLine($"   • Hábitos Ativos: {totalHabitos - habitosPausados}");
            Console.WriteLine($"   • Hábitos Pausados: {habitosPausados}");
            Console.WriteLine($"   • Total de Check-ins: {totalCheckins}");

            Console.WriteLine("\n[3] GRÁFICO DE CONSISTÊNCIA (Últimos 7 dias):");
            var end = DateTime.Now;
            var start = end.AddDays(-6);
            var dashData = await dashboardService.GetDashboardAsync(userId, start, end);

            foreach (var point in dashData.ConsistencyChart)
            {
                string bar = point.Count > 0 ? new string('■', point.Count * 2) : "."; 
                
                Console.Write($"   {point.Date}: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{bar} ({point.Count})");
                Console.ResetColor();
            }

            Console.WriteLine("\n[4] ANÁLISE (IA do Backend):");
            if (dashData.MostCompletedHabit != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   🏆 Melhor Hábito: {dashData.MostCompletedHabit.Title} ({dashData.MostCompletedHabit.CompletionRate}%)");
                Console.ResetColor();
            }
            if (dashData.LeastCompletedHabit != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   ⚠️  Precisa Melhorar: {dashData.LeastCompletedHabit.Title} ({dashData.LeastCompletedHabit.CompletionRate}%)");
                Console.ResetColor();
            }

            Console.WriteLine("\n========================================================\n");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao gerar dashboard: {ex.Message}");
    }
}

void PrintStatus(string nome, bool feito)
{
    Console.Write($"   [{ (feito ? "OK" : "  ") }] {nome}");
    if (feito) 
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(" ✔");
    }
    else 
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(" ✘");
    }
    Console.ResetColor();
}
// --- FIM DO DASHBOARD ---

app.Run();