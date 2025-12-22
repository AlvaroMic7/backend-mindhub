using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MindHub.Domain.Models
{
    public class Achievement
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty; // Ex: "trophy", "medal"

        [JsonIgnore]
        public List<UserAchievement> UserAchievements { get; set; } = new();
    }
}