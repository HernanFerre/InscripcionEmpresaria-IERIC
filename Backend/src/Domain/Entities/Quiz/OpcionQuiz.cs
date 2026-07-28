using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Entities
{
    public class OpcionQuiz
    {
        public string Id { get; private set; }

        public Cuil Cuil { get; private set; }

        public bool EsVinculado { get; private set; }

        private OpcionQuiz()
        {
        }

        public OpcionQuiz(
            string id,
            Cuil cuil,
            bool esVinculado
        )
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new SumariosDomainException(
                    "La opción del quiz debe tener un identificador"
                );
            }

            if (cuil == null)
            {
                throw new SumariosDomainException(
                    "La opción del quiz debe contener un CUIL"
                );
            }

            Id = id;
            Cuil = cuil;
            EsVinculado = esVinculado;
        }

        public string ObtenerCuilEnmascarado()
        {
            string numero = Cuil.ToString();

            return
                $"{numero.Substring(0, 2)}-" +
                $"xxxxx{numero.Substring(7, 3)}-" +
                $"{numero.Substring(10, 1)}";
        }
    }
}