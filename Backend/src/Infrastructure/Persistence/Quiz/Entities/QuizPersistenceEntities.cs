using System;
using System.Collections.Generic;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Quiz
{
    public class QuizSesionEntity
    {
        public long Id { get; set; }

        public long CuitEmpresa { get; set; }

        public string UsuarioId { get; set; }

        public byte Estado { get; set; }

        public byte IntentosTotales { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaExpiracion { get; set; }

        public DateTime? FechaFinalizacion { get; set; }

        public DateTime? BloqueadoHasta { get; set; }

        public List<QuizCuilVinculadoEntity> CuilesVinculados
        { get; set; } = new List<QuizCuilVinculadoEntity>();

        public List<QuizDesafioEntity> Desafios
        { get; set; } = new List<QuizDesafioEntity>();
    }

    public class QuizCuilVinculadoEntity
    {
        public long QuizSesionId { get; set; }

        public long Cuil { get; set; }

        public QuizSesionEntity QuizSesion { get; set; }
    }

    public class QuizDesafioEntity
    {
        public long Id { get; set; }

        public long QuizSesionId { get; set; }

        public byte Numero { get; set; }

        public byte Escenario { get; set; }

        public bool EsActual { get; set; }

        public DateTime FechaCreacion { get; set; }

        public QuizSesionEntity QuizSesion { get; set; }

        public List<QuizOpcionEntity> Opciones
        { get; set; } = new List<QuizOpcionEntity>();

        public QuizRespuestaEntity Respuesta { get; set; }
    }

    public class QuizOpcionEntity
    {
        public long QuizDesafioId { get; set; }

        public byte CodigoOpcion { get; set; }

        public long? Cuil { get; set; }

        public bool EsVinculado { get; set; }

        public QuizDesafioEntity Desafio { get; set; }

        public List<QuizRespuestaOpcionEntity> RespuestasSeleccionadas
        { get; set; } = new List<QuizRespuestaOpcionEntity>();
    }

    public class QuizRespuestaEntity
    {
        public long Id { get; set; }

        public long QuizDesafioId { get; set; }

        public bool EsCorrecta { get; set; }

        public DateTime FechaRespuesta { get; set; }

        public QuizDesafioEntity Desafio { get; set; }

        public List<QuizRespuestaOpcionEntity> OpcionesSeleccionadas
        { get; set; } = new List<QuizRespuestaOpcionEntity>();
    }

    public class QuizRespuestaOpcionEntity
    {
        public long QuizRespuestaId { get; set; }

        public long QuizDesafioId { get; set; }

        public byte CodigoOpcion { get; set; }

        public QuizRespuestaEntity Respuesta { get; set; }

        public QuizOpcionEntity Opcion { get; set; }
    }
}