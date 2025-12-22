using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MindHub.Domain.Models
{
    public class UserAchievement
    {
        public int Id { get; set; }

        // O Erro acontece porque faltavam estas duas linhas abaixo:
        public int UserId { get; set; }
        [JsonIgnore]
        public UserProfile? User { get; set; }

        public int AchievementId { get; set; }
        [JsonIgnore]
        public Achievement? Achievement { get; set; }

        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    }
}