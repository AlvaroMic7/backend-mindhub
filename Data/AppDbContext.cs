using Microsoft.EntityFrameworkCore;
using MindHub.Domain.Models;

namespace MindHub.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tabelas existentes
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitCheckIn> CheckIns { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        
        // Tabelas de Autenticação (que fizemos ontem)
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<PasswordRecoveryCode> PasswordRecoveryCodes { get; set; }

        // --- NOVA TABELA  ---
        public DbSet<Goal> Goals { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Dados iniciais das Conquistas (mantendo o que já tinha)
            modelBuilder.Entity<Achievement>().HasData(
                new Achievement { Id = 1, Title = "Mestre da Semana", Description = "Completou 90% dos hábitos em 7 dias", IconName = "badge-week" },
                new Achievement { Id = 2, Title = "Lenda Mensal", Description = "Ofensiva de 30 dias em um hábito", IconName = "trophy-month" }
            );

            // --- RELAÇÃO MUITOS-PARA-MUITOS ---
            // Uma Meta tem vários Hábitos <-> Um Hábito tem várias Metas
            modelBuilder.Entity<Goal>()
                .HasMany(g => g.Habits)
                .WithMany(h => h.Goals);
        }
    }
}