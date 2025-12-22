using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindHub.Infrastructure.Data;
using MindHub.Domain.Models;
using MindHub.Services;

namespace MindHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckInsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly GamificationService _gamificationService;

        public CheckInsController(AppDbContext context, AuthService authService, GamificationService gamificationService)
        {
            _context = context;
            _authService = authService;
            _gamificationService = gamificationService;
        }

        [HttpPost("{habitId}")]
        public async Task<IActionResult> CheckIn(int habitId)
        {
            var userId = await GetUserId();
            if (userId == null) return Unauthorized();

            var habit = await _context.Habits
                .Include(h => h.CheckIns)
                .FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId);

            if (habit == null) return NotFound("Hábito não encontrado.");

            var hoje = DateTime.UtcNow.Date;

            // 1. Validação de Frequência (RF012)
            // Impede check-in se hoje não for dia (Opcional: remova se quiser permitir check-in extra)
            if (!HabitLogic.IsScheduledForDate(habit.Frequency, hoje))
            {
               return BadRequest($"Este hábito ({habit.Frequency}) não está agendado para hoje ({hoje.DayOfWeek}).");
            }

            // 2. Verifica duplicidade
            bool jaFez = habit.CheckIns.Any(c => c.CheckInDate.Date == hoje);
            if (jaFez) return BadRequest("Check-in já realizado hoje.");

            // 3. Cria o check-in
            var checkIn = new HabitCheckIn { HabitId = habit.Id, CheckInDate = DateTime.UtcNow };
            _context.CheckIns.Add(checkIn);
            
            // 4. Lógica de Ofensiva INTELIGENTE (RF26-RF29)
            if (habit.LastCompletedDate == null)
            {
                // Primeiro check-in da vida
                habit.CurrentStreak = 1;
            }
            else
            {
                var ultimo = habit.LastCompletedDate.Value.Date;
                
                // Pergunta ao cérebro se quebrou
                bool quebrou = HabitLogic.DidBreakStreak(habit.Frequency, ultimo, hoje);

                if (quebrou)
                {
                    habit.CurrentStreak = 1; // Recomeça do 1
                }
                else
                {
                    habit.CurrentStreak++; // Continua, mesmo se passou dias (desde que fossem folgas)
                }
            }

            // Atualiza recorde
            if (habit.CurrentStreak > habit.LongestStreak) habit.LongestStreak = habit.CurrentStreak;

            habit.LastCompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _gamificationService.ProcessCheckInAsync(userId.Value, habitId);

            return Ok(new { message = "Check-in realizado!", streak = habit.CurrentStreak });
        }

        private async Task<int?> GetUserId()
        {
            if (!Request.Headers.ContainsKey("Authorization")) return null;
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return await _authService.GetUserIdByTokenAsync(token);
        }
    }
}