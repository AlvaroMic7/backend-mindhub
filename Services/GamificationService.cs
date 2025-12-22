using Microsoft.EntityFrameworkCore;
using MindHub.Infrastructure.Data;
using MindHub.Domain.Models;

namespace MindHub.Services
{
    public class GamificationService
    {
        private readonly AppDbContext _context;

        public GamificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task ProcessCheckInAsync(int userId, int habitId)
        {
            // 1. Busca o usuário para dar pontos
            var user = await _context.UserProfiles.FindAsync(userId);
            if (user == null) return;

            // Adiciona 10 XP
            user.CurrentPoints += 10;

            // 2. Verifica conquista de Semana Perfeita
            // Regra: Ter feito pelo menos 7 check-ins nos últimos 7 dias
            var dataLimite = DateTime.UtcNow.AddDays(-7);
            
            // Correção aqui: Usamos 'CheckInDate' (que é o nome certo no seu Model agora)
            var totalCheckInsSemana = await _context.CheckIns
                .CountAsync(c => c.Habit.UserId == userId && c.CheckInDate >= dataLimite);

            if (totalCheckInsSemana >= 7)
            {
                await UnlockAchievement(userId, 1); // ID 1 = Conquista Semanal
            }
            
            // 3. Verifica conquista de Ofensiva (Foguinho)
            // Busca o hábito para ver o Streak atual
            var habit = await _context.Habits.FindAsync(habitId);
            
            // Correção aqui: Usamos 'CurrentStreak' (que adicionamos hoje no Model)
            if (habit != null && habit.CurrentStreak >= 30)
            {
                await UnlockAchievement(userId, 2); // ID 2 = Conquista Mensal
            }

            await _context.SaveChangesAsync();
        }

        private async Task UnlockAchievement(int userId, int achievementId)
        {
            // Verifica se já tem essa conquista para não duplicar
            bool jaTem = await _context.UserAchievements
                .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

            if (!jaTem)
            {
                _context.UserAchievements.Add(new UserAchievement
                {
                    UserId = userId,
                    AchievementId = achievementId,
                    UnlockedAt = DateTime.UtcNow
                });
            }
        }
    }
}