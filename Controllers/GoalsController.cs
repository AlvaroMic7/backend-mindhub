using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindHub.Infrastructure.Data;
using MindHub.Domain.Models;
using MindHub.Application.DTOs;
using MindHub.Services;

namespace MindHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoalsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public GoalsController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGoal([FromBody] CreateGoalDto dto)
        {
            var userId = await GetUserId();
            if (userId == null) return Unauthorized();

            var goal = new Goal
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                UserId = userId.Value,
                Status = "Em Andamento"
            };

            if (dto.HabitIds.Any())
            {
                var habits = await _context.Habits
                    .Where(h => dto.HabitIds.Contains(h.Id) && h.UserId == userId)
                    .ToListAsync();
                goal.Habits = habits;
            }

            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Meta criada com sucesso!", goalId = goal.Id });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyGoals()
        {
            var userId = await GetUserId();
            if (userId == null) return Unauthorized();

            // Traz as metas e inclui os Hábitos E os CheckIns desses hábitos
            var goals = await _context.Goals
                .Include(g => g.Habits)
                .ThenInclude(h => h.CheckIns) 
                .Where(g => g.UserId == userId)
                .ToListAsync();

            var result = goals.Select(g => new GoalDto
            {
                Id = g.Id,
                Title = g.Title,
                Status = g.Status,
                EndDate = g.EndDate,
                HabitCount = g.Habits.Count,
                // Chama a função auxiliar abaixo para calcular a %
                ProgressPercentage = CalculateProgress(g) 
            }).ToList();

            return Ok(result);
        }

        // Função Matemática do Progresso (RF040)
        private double CalculateProgress(Goal goal)
        {
            if (!goal.Habits.Any()) return 0;

            int totalCheckinsEsperados = 0;
            int totalCheckinsFeitos = 0;
            var hoje = DateTime.UtcNow.Date;

            // Definimos o fim do cálculo como "Hoje" (se a meta ainda está rolando) 
            // ou a DataFim (se a meta já acabou), para não calcular futuro.
            var dataFimCalculo = goal.EndDate < hoje ? goal.EndDate : hoje;
            
            // Se a meta nem começou ainda
            if (goal.StartDate > hoje) return 0;

            foreach (var habit in goal.Habits)
            {
                // Conta quantos check-ins foram feitos DENTRO do período da meta
                int feitos = habit.CheckIns.Count(c => c.CheckInDate.Date >= goal.StartDate && c.CheckInDate.Date <= goal.EndDate);
                totalCheckinsFeitos += feitos;

                // Calcula quantos DEVERIAM ter sido feitos até hoje
                for (var dia = goal.StartDate; dia <= dataFimCalculo; dia = dia.AddDays(1))
                {
                    if (HabitLogic.IsScheduledForDate(habit.Frequency, dia))
                    {
                        totalCheckinsEsperados++;
                    }
                }
            }

            if (totalCheckinsEsperados == 0) return 0;

            double percent = (double)totalCheckinsFeitos / totalCheckinsEsperados * 100;
            return Math.Round(percent > 100 ? 100 : percent, 1); // Trava em 100%
        }

        private async Task<int?> GetUserId()
        {
            if (!Request.Headers.ContainsKey("Authorization")) return null;
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return await _authService.GetUserIdByTokenAsync(token);
        }
    }
}