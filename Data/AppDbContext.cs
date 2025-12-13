using Microsoft.EntityFrameworkCore;
using MindHub.Domain.Models;

namespace MindHub.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Essa linha diz: "Crie uma tabela chamada Habits baseada na classe Habit"
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitCheckIn> CheckIns { get; set; }
    }
}