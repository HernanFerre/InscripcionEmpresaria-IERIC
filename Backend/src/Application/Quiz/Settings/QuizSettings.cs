using System;

namespace IERIC.SumariosIERIC.Application.Quiz.Settings
{
    public class QuizSettings
    {
        public int? DuracionBloqueo { get; set; }

        public string UnidadDuracionBloqueo
        { get; set; } = "seconds";

        public TimeSpan? ObtenerDuracionBloqueo()
        {
            if (!DuracionBloqueo.HasValue)
            {
                return null;
            }

            if (DuracionBloqueo.Value <= 0)
            {
                throw new InvalidOperationException(
                    "La duración del bloqueo debe ser mayor a cero."
                );
            }

            string unidad =
                UnidadDuracionBloqueo?
                    .Trim()
                    .ToLowerInvariant();

            return unidad switch
            {
                "seconds" =>
                    TimeSpan.FromSeconds(
                        DuracionBloqueo.Value
                    ),

                "minutes" =>
                    TimeSpan.FromMinutes(
                        DuracionBloqueo.Value
                    ),

                "hours" =>
                    TimeSpan.FromHours(
                        DuracionBloqueo.Value
                    ),

                _ => throw new InvalidOperationException(
                    "La unidad de duración del bloqueo " +
                    "debe ser seconds, minutes o hours."
                )
            };
        }
    }
}