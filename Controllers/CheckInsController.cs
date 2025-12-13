using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindHub.Application.DTOs;
using MindHub.Domain.Models;
using MindHub.Infrastructure.Data;

namespace MindHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckInsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CheckInsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/CheckIns?date=2025-12-13
        // Gera a lista do dia (RF-018 automático)
        [HttpGet]
        public async Task<IActionResult> GetDailyList([FromQuery] DateOnly date)
        {
            var dayOfWeek = date.DayOfWeek;

            // 1. Busca todos os hábitos
            var allHabits = await _context.Habits.ToListAsync();

            // 2. Filtra quais hábitos devem aparecer HOJE (Regra RF-012)
            var habitsForToday = allHabits.Where(h => 
                h.Frequency == FrequencyType.Daily || // Diário: Sempre aparece
                h.Frequency == FrequencyType.TimesPerWeek || // X Vezes: Sempre aparece como opção
                (h.Frequency == FrequencyType.SpecificDays && h.SpecificDays != null && h.SpecificDays.Contains(dayOfWeek)) // Dias Específicos: Só se for o dia certo
            ).ToList();

            // 3. Busca quais já foram concluídos nesta data
            var checkIns = await _context.CheckIns
                .Where(c => c.Date == date)
                .Select(c => c.HabitId)
                .ToListAsync();

            // 4. Monta o DTO combinando as duas informações
            var result = habitsForToday.Select(h => new DailyHabitDto
            {
                HabitId = h.Id,
                Title = h.Title,
                IsCompleted = checkIns.Contains(h.Id) // True se achou no banco, False se não
            });

            return Ok(result);
        }

        // POST: api/CheckIns/toggle
        // Serve tanto para Marcar quanto Desmarcar (RF-018 e RF-019)
        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleCheckIn([FromBody] ToggleCheckInDto dto)
        {
            // Validação RF-021: Não permitir marcar em dias futuros
            var today = DateOnly.FromDateTime(DateTime.UtcNow); 
            // Obs: Em produção ideal, converteríamos o UTC para o fuso horário do usuário.
            // Para o MVP, usaremos a data do servidor ou aceitaremos a data enviada se não for absurda.
            
            if (dto.Date > today)
            {
                return BadRequest("Não é permitido concluir hábitos em datas futuras.");
            }

            // Verifica se o hábito existe
            var habit = await _context.Habits.FindAsync(dto.HabitId);
            if (habit == null) return NotFound("Hábito não encontrado.");

            // Verifica se JÁ existe o check-in (para decidir se marca ou desmarca)
            var existingCheckIn = await _context.CheckIns
                .FirstOrDefaultAsync(c => c.HabitId == dto.HabitId && c.Date == dto.Date);

            if (existingCheckIn != null)
            {
                // RF-019: Se já existe, remove (Desfazer)
                _context.CheckIns.Remove(existingCheckIn);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Hábito desmarcado.", isCompleted = false });
            }
            else
            {
                // RF-018: Se não existe, cria (Concluir)
                var newCheckIn = new HabitCheckIn
                {
                    HabitId = dto.HabitId,
                    Date = dto.Date,
                    CompletedAt = DateTime.UtcNow
                };

                _context.CheckIns.Add(newCheckIn);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Hábito concluído!", isCompleted = true });
            }
        }
    }
}