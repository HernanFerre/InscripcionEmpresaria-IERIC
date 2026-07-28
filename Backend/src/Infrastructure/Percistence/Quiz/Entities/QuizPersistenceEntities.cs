using System;
using System.Collections.Generic;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Quiz
{
    public class QuizSesionEntity
    {
        public Guid Id { get; set; }

        public string CuitEmpresa { get; set; }

        public int Estado { get; set; }

        public int IntentosTotales { get; set; }

        public int IntentosRestantes { get; set; }

        public DateTime FechaCreacionUtc { get; set; }

        public DateTime FechaExpiracionUtc { get; set; }

        public DateTime? FechaFinalizacionUtc { get; set; }

        public List<QuizCuilVinculadoEntity>
            CuilesVinculados
        { get; set; } =
                new List<QuizCuilVinculadoEntity>();

        public List<QuizDesafioEntity> Desafios { get; set; } =
            new List<QuizDesafioEntity>();
    }

    public class QuizCuilVinculadoEntity
    {
        public Guid QuizId { get; set; }

        public string Cuil { get; set; }

        public QuizSesionEntity Quiz { get; set; }
    }

    public class QuizDesafioEntity
    {
        public Guid Id { get; set; }

        public Guid QuizId { get; set; }

        public int Numero { get; set; }

        public int Escenario { get; set; }

        public bool EsActual { get; set; }

        public DateTime FechaCreacionUtc { get; set; }

        public QuizSesionEntity Quiz { get; set; }

        public List<QuizOpcionEntity> Opciones { get; set; } =
            new List<QuizOpcionEntity>();

        public List<QuizRespuestaEntity> Respuestas { get; set; } =
            new List<QuizRespuestaEntity>();
    }

    public class QuizOpcionEntity
    {
        public Guid QuizDesafioId { get; set; }

        public string Id { get; set; }

        public string Cuil { get; set; }

        public bool EsVinculado { get; set; }

        public int Orden { get; set; }

        public QuizDesafioEntity Desafio { get; set; }
    }

    public class QuizRespuestaEntity
    {
        public Guid Id { get; set; }

        public Guid QuizDesafioId { get; set; }

        public string OpcionesSeleccionadas { get; set; }

        public bool EsCorrecta { get; set; }

        public DateTime FechaRespuestaUtc { get; set; }

        public QuizDesafioEntity Desafio { get; set; }
    }
}