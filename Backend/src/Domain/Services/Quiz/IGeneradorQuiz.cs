using System.Collections.Generic;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Services
{
    public interface IGeneradorQuiz
    {
        Quiz CrearQuiz(
            Cuit cuitEmpresa,
            IEnumerable<Cuil> cuilesVinculados,
            int intentosTotales = 3
        );

        void GenerarNuevoDesafio(Quiz quiz);
    }
}