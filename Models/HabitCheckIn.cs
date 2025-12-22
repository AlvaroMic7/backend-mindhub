using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MindHub.Domain.Models
{
    public class HabitCheckIn
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        
        [JsonIgnore]
        public Habit? Habit { get; set; }

        public DateTime CheckInDate { get; set; } = DateTime.UtcNow; // O erro pedia isso
    }
}