using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Entities.Inscripcion
{
    public class EmpresaIntegrante
    {
        public Empresa EmpresaPrincipal { get; private set; }

        public Empresa Integrante { get; private set; }

        public Cuit CuitEmpresaPrincipal =>
            EmpresaPrincipal.Cuit;

        public Cuit CuitEmpresaIntegrante =>
            Integrante.Cuit;

        private EmpresaIntegrante()
        {
        }

        private EmpresaIntegrante(
            Empresa empresaPrincipal,
            Empresa integrante
        )
        {
            if (empresaPrincipal == null)
            {
                throw new SumariosDomainException(
                    "La relación debe tener una empresa principal."
                );
            }

            if (integrante == null)
            {
                throw new SumariosDomainException(
                    "La relación debe tener una empresa integrante."
                );
            }

            if (
                empresaPrincipal.Cuit.ToInt64() ==
                integrante.Cuit.ToInt64()
            )
            {
                throw new SumariosDomainException(
                    "Una empresa no puede ser integrante de sí misma."
                );
            }

            EmpresaPrincipal = empresaPrincipal;
            Integrante = integrante;
        }

        public static EmpresaIntegrante Vincular(
            Empresa empresaPrincipal,
            Empresa integrante
        )
        {
            return new EmpresaIntegrante(
                empresaPrincipal,
                integrante
            );
        }

        public bool CorrespondeA(
            Cuit cuitEmpresaPrincipal,
            Cuit cuitEmpresaIntegrante
        )
        {
            if (
                cuitEmpresaPrincipal == null ||
                cuitEmpresaIntegrante == null
            )
            {
                return false;
            }

            return
                CuitEmpresaPrincipal.ToInt64() ==
                cuitEmpresaPrincipal.ToInt64() &&
                CuitEmpresaIntegrante.ToInt64() ==
                cuitEmpresaIntegrante.ToInt64();
        }
    }
}