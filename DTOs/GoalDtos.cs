using System.ComponentModel.DataAnnotations;

namespace MindHub.Application.DTOs
{
    // O que o usuário manda para criar a meta
    public class CreateGoalDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Lista dos IDs dos hábitos que fazem parte dessa meta (RF39)
        public List<int> HabitIds { get; set; } = new();
    }

    public class GoalDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Em Andamento", "Concluída"
        public DateTime EndDate { get; set; }
        
        // Quantos hábitos estão vinculados
        public int HabitCount { get; set; } 
        
        // Progresso simples (Quantos % concluído - lógica futura)
        public double ProgressPercentage { get; set; } 
    }
}