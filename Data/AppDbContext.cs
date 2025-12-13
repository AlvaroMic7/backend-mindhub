using Microsoft.EntityFrameworkCore;
using MindHub.Domain.Models;

namespace MindHub.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitCheckIn> CheckIns { get; set; }
        
        // Novas tabelas
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed: Cria automaticamente o Usuário e as Conquistas ao rodar a migration
            modelBuilder.Entity<UserProfile>().HasData(
                new UserProfile { Id = 1, Name = "Eu", CurrentPoints = 0 }
            );

            modelBuilder.Entity<Achievement>().HasData(
                new Achievement { Id = 1, Title = "Mestre da Semana", Description = "Completou 90% dos hábitos em 7 dias", IconName = "badge-week" },
                new Achievement { Id = 2, Title = "Lenda Mensal", Description = "Ofensiva de 30 dias em um hábito", IconName = "trophy-month" }
            );
        }
    }
}