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
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
using QuizDominio =
    IERIC.SumariosIERIC.Domain.Entities.Quiz;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class CrearQuizCommandHandler
        : IRequestHandler<CrearQuizCommand, QuizResponse>
    {
        private readonly IGeneradorQuiz _generadorQuiz;
        private readonly IQuizRepository _quizRepository;

        public CrearQuizCommandHandler(
            IGeneradorQuiz generadorQuiz,
            IQuizRepository quizRepository
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
        }

        public async Task<QuizResponse> Handle(
            CrearQuizCommand command,
            CancellationToken cancellationToken
        )
        {
            Cuit cuitEmpresa =
                CrearCuit(command.Cuit);

            List<Cuil> cuilesVinculados =
                CrearCuiles(command.Cuiles);

            QuizDominio quiz =
                _generadorQuiz.CrearQuiz(
                    cuitEmpresa,
                    cuilesVinculados
                );

            await _quizRepository.GuardarAsync(quiz);

            return QuizResponseMapper.DesdeDominio(quiz);
        }

        private Cuit CrearCuit(string valor)
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
                    "El CUIT de la empresa no es válido"
                );
            }

            return new Cuit(numero);
        }

        private List<Cuil> CrearCuiles(
            IEnumerable<string> valores
        )
        {
            if (valores == null)
            {
                throw new SumariosDomainException(
                    "La lista de CUIL no puede ser nula"
                );
            }

            List<Cuil> cuiles = new List<Cuil>();

            foreach (string valor in valores)
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
                        $"El CUIL '{valor}' no es válido"
                    );
                }

                cuiles.Add(new Cuil(numero));
            }

            return cuiles;
        }

        private string NormalizarNumero(string valor)
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