using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MindHub.Domain.Models
{
    public class Habit
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Frequency { get; set; } = "Diario"; 

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public bool IsPaused { get; set; } = false;

        public int UserId { get; set; }
        
        [JsonIgnore] 
        public UserProfile? User { get; set; }

        // --- OFENSIVA  ---
        public int CurrentStreak { get; set; } = 0;
        public int LongestStreak { get; set; } = 0;
        public DateTime? LastCompletedDate { get; set; }

        // --- RELAÇÕES ---
        [JsonIgnore]
        public List<HabitCheckIn> CheckIns { get; set; } = new();
        
        [JsonIgnore]
        public List<Goal> Goals { get; set; } = new();
    }
}