using System;
using System.Threading;

namespace IERIC.SumariosIERIC.Infrastructure.Services.DatosMaestrosApi
{
    public sealed class EstadoTokenDatosMaestros
    {
        private readonly object _bloqueo = new object();

        private string _token = string.Empty;

        private DateTimeOffset _vigenteHastaUtc =
            DateTimeOffset.MinValue;

        public SemaphoreSlim Renovacion { get; } =
            new SemaphoreSlim(1, 1);

        public bool IntentarObtenerTokenVigente(
            out string token
        )
        {
            lock (_bloqueo)
            {
                if (
                    !string.IsNullOrWhiteSpace(_token) &&
                    DateTimeOffset.UtcNow < _vigenteHastaUtc
                )
                {
                    token = _token;
                    return true;
                }

                token = string.Empty;
                return false;
            }
        }

        public void Guardar(
            string token,
            DateTimeOffset vigenteHastaUtc
        )
        {
            lock (_bloqueo)
            {
                _token = token;
                _vigenteHastaUtc = vigenteHastaUtc;
            }
        }

        public void Invalidar()
        {
            lock (_bloqueo)
            {
                _token = string.Empty;
                _vigenteHastaUtc = DateTimeOffset.MinValue;
            }
        }
    }
}