using Microsoft.EntityFrameworkCore;
using MindHub.Infrastructure.Data;
using MindHub.Domain.Models;

namespace MindHub.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardAsync(int userId, DateTime start, DateTime end)
        {
            var habits = await _context.Habits
                .Include(h => h.CheckIns)
                .Where(h => h.UserId == userId)
                .ToListAsync();

            var data = new DashboardDto();

            // Gráfico de Consistência
            // Agrupa check-ins por data
            var checkInsNoPeriodo = habits
                .SelectMany(h => h.CheckIns)
                .Where(c => c.CheckInDate >= start && c.CheckInDate <= end)
                .GroupBy(c => c.CheckInDate.Date)
                .Select(g => new ConsistencyPoint { Date = g.Key.ToString("dd/MM"), Count = g.Count() })
                .ToList();
            
            data.ConsistencyChart = checkInsNoPeriodo;

            // Hábito Mais Concluído
            var habitsOrdered = habits.OrderByDescending(h => h.CheckIns.Count).ToList();
            if (habitsOrdered.Any())
            {
                var best = habitsOrdered.First();
                data.MostCompletedHabit = new HabitStat { Title = best.Title, Count = best.CheckIns.Count };
            }

            return data;
        }
    }

    // Classes DTO auxiliares para o Dashboard
    public class DashboardDto
    {
        public List<ConsistencyPoint> ConsistencyChart { get; set; } = new();
        public HabitStat? MostCompletedHabit { get; set; }
    }

    public class ConsistencyPoint
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class HabitStat
    {
        public string Title { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}