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
    public class HabitsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public HabitsController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyHabits()
        {
            var userId = await GetUserId();
            if (userId == null) return Unauthorized();

            var habits = await _context.Habits
                .Where(h => h.UserId == userId)
                .Select(h => new HabitDto
                {
                    Id = h.Id,
                    Title = h.Title,
                    Description = h.Description,
                    Frequency = h.Frequency,
                    IsPaused = h.IsPaused,
                    CurrentStreak = h.CurrentStreak,
                    LongestStreak = h.LongestStreak
                })
                .ToListAsync();

            return Ok(habits);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHabit([FromBody] CreateHabitDto dto)
        {
            var userId = await GetUserId();
            if (userId == null) return Unauthorized();

            var habit = new Habit
            {
                Title = dto.Title,
                Description = dto.Description,
                Frequency = dto.Frequency,
                UserId = userId.Value,
                StartDate = DateTime.UtcNow,
                CurrentStreak = 0,
                LongestStreak = 0
            };

            _context.Habits.Add(habit);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Hábito criado com sucesso!" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHabit(int id, [FromBody] UpdateHabitDto dto)
        {
            var userId = await GetUserId();
            if (userId == null) return Unauthorized();

            var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);
            if (habit == null) return NotFound("Hábito não encontrado.");

            habit.Title = dto.Title;
            habit.Description = dto.Description;
            habit.Frequency = dto.Frequency;
            habit.IsPaused = dto.IsPaused;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Hábito atualizado!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHabit(int id)
        {
            var userId = await GetUserId();
            if (userId == null) return Unauthorized();

            var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);
            if (habit == null) return NotFound();

            _context.Habits.Remove(habit);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Hábito excluído." });
        }

        private async Task<int?> GetUserId()
        {
            if (!Request.Headers.ContainsKey("Authorization")) return null;
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return await _authService.GetUserIdByTokenAsync(token);
        }
    }
}