using System.ComponentModel.DataAnnotations;

namespace MindHub.Application.DTOs
{
    public class CreateHabitDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public string Frequency { get; set; } = "Diario"; 
    }

    public class UpdateHabitDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public string Frequency { get; set; } = "Diario";
        public bool IsPaused { get; set; }
    }

    public class HabitDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public bool IsPaused { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
    }
}