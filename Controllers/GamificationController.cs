using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindHub.Infrastructure.Data;
using MindHub.Services;

namespace MindHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamificationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public GamificationController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // Retorna o Ranking dos 10 melhores usuários
        [HttpGet("ranking")]
        public async Task<IActionResult> GetRanking()
        {
            var ranking = await _context.UserProfiles
                .OrderByDescending(u => u.CurrentPoints)
                .Take(10)
                .Select(u => new { u.Name, u.CurrentPoints })
                .ToListAsync();

            return Ok(ranking);
        }

        // Retorna as conquistas do usuário logado
        [HttpGet("achievements")]
        public async Task<IActionResult> GetMyAchievements()
        {
            var userId = await GetUserId();
            if (userId == null) return Unauthorized();

            var achievements = await _context.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.UserId == userId)
                .Select(ua => new 
                { 
                    ua.Achievement.Title, 
                    ua.Achievement.Description, 
                    UnlockedAt = ua.UnlockedAt 
                })
                .ToListAsync();

            return Ok(achievements);
        }

        // Função auxiliar para ler o Token
        private async Task<int?> GetUserId()
        {
            if (!Request.Headers.ContainsKey("Authorization")) return null;
            
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return await _authService.GetUserIdByTokenAsync(token);
        }
    }
}