using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MindHub.Domain.Models
{
    public class Goal
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty; // Ex: "Projeto Verão"

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Status da Meta "Em Andamento", "Concluída", "Falhou"
        public string Status { get; set; } = "Em Andamento";

        public int UserId { get; set; }
        
        [JsonIgnore]
        public UserProfile? User { get; set; }

        // RELAÇÃO: Uma Meta tem vários Hábitos (RF39)
        public List<Habit> Habits { get; set; } = new();
    }
}