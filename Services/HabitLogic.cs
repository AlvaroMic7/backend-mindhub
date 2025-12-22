namespace MindHub.Services
{
    public static class HabitLogic
    {
        // Verifica se o hábito deve ser feito em uma data específica
        public static bool IsScheduledForDate(string frequency, DateTime date)
        {
            // Normaliza para minúsculo
            var freq = frequency.ToLower().Trim();

            if (freq == "diario" || freq == "diário") return true;

            // Suporte para dias específicos (Ex: "seg,qua,sex" ou "mon,wed,fri")
            // O DayOfWeek do C# retorna: Sunday=0, Monday=1...
            int diaSemana = (int)date.DayOfWeek; 
            
            // Mapeamento simples (0=Dom, 1=Seg, etc.)
            string[] diasPt = { "dom", "seg", "ter", "qua", "qui", "sex", "sab" };
            string hojeSigla = diasPt[diaSemana];

            // Se a string de frequência contém a sigla do dia (ex: "seg,qua")
            return freq.Contains(hojeSigla);
        }

        // Calcula se a Ofensiva quebrou (Verifica buracos entre o último check-in e ontem)
        public static bool DidBreakStreak(string frequency, DateTime lastCheckIn, DateTime today)
        {
            var ontem = today.AddDays(-1);
            
            // Se o último check-in foi ontem ou hoje, não quebrou
            if (lastCheckIn.Date >= ontem.Date) return false;

            // Se foi antes de ontem, precisamos ver se teve algum dia agendado nesse intervalo
            // Loop do dia seguinte ao último check-in até ontem
            for (var dia = lastCheckIn.AddDays(1); dia <= ontem; dia = dia.AddDays(1))
            {
                // Se o hábito estava agendado para esse dia "buraco" e não foi feito...
                if (IsScheduledForDate(frequency, dia))
                {
                    return true; // Quebrou a ofensiva!
                }
            }

            // Se passou pelo loop e nenhum dia era agendado, a ofensiva se mantém (férias/folga)
            return false;
        }
    }
}