namespace MindHub.Domain.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Usuário Padrão";
        public int CurrentPoints { get; set; } = 0; // RF-030: Inicia com 0
    }
}