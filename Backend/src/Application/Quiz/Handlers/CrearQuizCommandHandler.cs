using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using IERIC.SumariosIERIC.Application.Quiz.Mappers;
using IERIC.SumariosIERIC.Application.Quiz.Models;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.Services;
using IERIC.SumariosIERIC.Domain.ValueObjects;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
using QuizDominio =
    IERIC.SumariosIERIC.Domain.Entities.Quiz;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class CrearQuizCommandHandler
        : IRequestHandler<CrearQuizCommand, QuizResponse>
    {
        private const int LimiteCuiles = 10;

        private readonly IGeneradorQuiz _generadorQuiz;
        private readonly IQuizRepository _quizRepository;
        private readonly IProveedorCuilesEmpresa
            _proveedorCuilesEmpresa;
        private readonly IProveedorEstadoEmpresa
            _proveedorEstadoEmpresa;

        public CrearQuizCommandHandler(
            IGeneradorQuiz generadorQuiz,
            IQuizRepository quizRepository,
            IProveedorCuilesEmpresa proveedorCuilesEmpresa,
            IProveedorEstadoEmpresa proveedorEstadoEmpresa
        )
        {
            _generadorQuiz =
                generadorQuiz ??
                throw new ArgumentNullException(
                    nameof(generadorQuiz)
                );

            _quizRepository =
                quizRepository ??
                throw new ArgumentNullException(
                    nameof(quizRepository)
                );

            _proveedorCuilesEmpresa =
                proveedorCuilesEmpresa ??
                throw new ArgumentNullException(
                    nameof(proveedorCuilesEmpresa)
                );

            _proveedorEstadoEmpresa =
                proveedorEstadoEmpresa ??
                throw new ArgumentNullException(
                    nameof(proveedorEstadoEmpresa)
                );
        }

        public async Task<QuizResponse> Handle(
            CrearQuizCommand command,
            CancellationToken cancellationToken
        )
        {
            Cuit cuitEmpresa =
                CrearCuit(command.Cuit);

            EstadoInscripcionEmpresa estadoEmpresa =
                await _proveedorEstadoEmpresa
                    .ObtenerPorCuitAsync(
                        cuitEmpresa,
                        cancellationToken
                    );

            if (!estadoEmpresa.PuedeIniciarInscripcion)
            {
                string mensaje =
                    string.IsNullOrWhiteSpace(
                        estadoEmpresa.Mensaje
                    )
                        ? "El estado de la empresa no permite iniciar la inscripción."
                        : estadoEmpresa.Mensaje;

                throw new SumariosDomainException(
                    mensaje
                );
            }

            IReadOnlyCollection<Cuil> resultadoCuiles =
                await _proveedorCuilesEmpresa
                    .ObtenerPorCuitAsync(
                        cuitEmpresa,
                        LimiteCuiles,
                        cancellationToken
                    );

            if (
                resultadoCuiles == null ||
                resultadoCuiles.Count == 0
            )
            {
                throw new SumariosDomainException(
                    "No se encontraron CUILes vinculados al CUIT informado."
                );
            }

            List<Cuil> cuilesVinculados =
                resultadoCuiles.ToList();

            QuizDominio quiz =
                _generadorQuiz.CrearQuiz(
                    cuitEmpresa,
                    cuilesVinculados
                );

            await _quizRepository.GuardarAsync(quiz);

            return QuizResponseMapper.DesdeDominio(quiz);
        }

        private Cuit CrearCuit(
            string valor
        )
        {
            string numeroNormalizado =
                NormalizarNumero(valor);

            if (
                !long.TryParse(
                    numeroNormalizado,
                    out long numero
                )
            )
            {
                throw new SumariosDomainException(
                    "El CUIT de la empresa no es válido."
                );
            }

            return new Cuit(numero);
        }

        private string NormalizarNumero(
            string valor
        )
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            return new string(
                valor
                    .Where(char.IsDigit)
                    .ToArray()
            );
        }
    }
}