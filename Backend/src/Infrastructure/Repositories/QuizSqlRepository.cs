using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
using IERIC.SumariosIERIC.Infrastructure.Persistence.Quiz;
using QuizDominio =
    IERIC.SumariosIERIC.Domain.Entities.Quiz;

namespace IERIC.SumariosIERIC.Infrastructure.Repositories
{
    public class QuizSqlRepository
        : IQuizRepository
    {
        private readonly SumariosContext _context;

        public QuizSqlRepository(
            SumariosContext context
        )
        {
            _context =
                context ??
                throw new ArgumentNullException(
                    nameof(context)
                );
        }

        public async Task GuardarAsync(
            QuizDominio quiz
        )
        {
            if (quiz == null)
            {
                throw new ArgumentNullException(nameof(quiz));
            }

            QuizSesionEntity sesion =
                await _context.QuizSesiones
                    .Include(x => x.Desafios)
                    .SingleOrDefaultAsync(
                        x => x.Id == quiz.Id
                    );

            if (sesion == null)
            {
                sesion = CrearSesion(quiz);

                _context.QuizSesiones.Add(sesion);

                await _context.SaveChangesAsync();

                return;
            }

            ActualizarSesion(sesion, quiz);

            if (quiz.Estado != EstadoQuiz.Activo)
            {
                QuizDesafioEntity desafioActual =
                    sesion.Desafios
                        .SingleOrDefault(
                            x => x.EsActual
                        );

                if (desafioActual != null)
                {
                    desafioActual.EsActual = false;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<QuizDominio> ObtenerPorIdAsync(
            Guid quizId
        )
        {
            QuizSesionEntity sesion =
                await _context.QuizSesiones
                    .AsNoTracking()
                    .Include(x => x.CuilesVinculados)
                    .Include(x => x.Desafios)
                        .ThenInclude(x => x.Opciones)
                    .SingleOrDefaultAsync(
                        x => x.Id == quizId
                    );

            if (sesion == null)
            {
                return null;
            }

            QuizDesafioEntity desafio =
                sesion.Desafios
                    .Where(x => x.EsActual)
                    .OrderByDescending(x => x.Numero)
                    .FirstOrDefault()
                ??
                sesion.Desafios
                    .OrderByDescending(x => x.Numero)
                    .FirstOrDefault();

            if (desafio == null)
            {
                throw new InvalidOperationException(
                    "La sesión almacenada no contiene desafíos."
                );
            }

            Cuit cuitEmpresa = new Cuit(
                long.Parse(sesion.CuitEmpresa)
            );

            List<Cuil> cuilesVinculados =
                sesion.CuilesVinculados
                    .Select(
                        x => new Cuil(
                            long.Parse(x.Cuil)
                        )
                    )
                    .ToList();

            List<OpcionQuiz> opciones =
                desafio.Opciones
                    .OrderBy(x => x.Orden)
                    .Select(
                        x => new OpcionQuiz(
                            x.Id,
                            new Cuil(
                                long.Parse(x.Cuil)
                            ),
                            x.EsVinculado
                        )
                    )
                    .ToList();

            return QuizDominio.Restaurar(
                sesion.Id,
                cuitEmpresa,
                cuilesVinculados,
                (EscenarioQuiz)desafio.Escenario,
                opciones,
                (EstadoQuiz)sesion.Estado,
                sesion.IntentosTotales,
                sesion.IntentosRestantes,
                sesion.FechaCreacionUtc,
                sesion.FechaExpiracionUtc
            );
        }

        public async Task GuardarValidacionAsync(
            QuizDominio quiz,
            IEnumerable<string> opcionesSeleccionadas,
            bool respuestaCorrecta
        )
        {
            if (quiz == null)
            {
                throw new ArgumentNullException(nameof(quiz));
            }

            List<string> seleccionadas =
                opcionesSeleccionadas?
                    .Where(
                        x => !string.IsNullOrWhiteSpace(x)
                    )
                    .Select(
                        x => x.Trim().ToLowerInvariant()
                    )
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase
                    )
                    .ToList();

            if (
                seleccionadas == null ||
                seleccionadas.Count == 0
            )
            {
                throw new ArgumentException(
                    "Debe existir al menos una opción seleccionada.",
                    nameof(opcionesSeleccionadas)
                );
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                QuizSesionEntity sesion =
                    await _context.QuizSesiones
                        .Include(x => x.Desafios)
                        .SingleOrDefaultAsync(
                            x => x.Id == quiz.Id
                        );

                if (sesion == null)
                {
                    throw new InvalidOperationException(
                        "No se encontró la sesión del quiz."
                    );
                }

                QuizDesafioEntity desafioActual =
                    sesion.Desafios
                        .SingleOrDefault(
                            x => x.EsActual
                        );

                if (desafioActual == null)
                {
                    throw new InvalidOperationException(
                        "No se encontró el desafío actual del quiz."
                    );
                }

                QuizRespuestaEntity respuesta =
                    new QuizRespuestaEntity
                    {
                        Id = Guid.NewGuid(),
                        QuizDesafioId =
                            desafioActual.Id,
                        OpcionesSeleccionadas =
                            JsonSerializer.Serialize(
                                seleccionadas
                            ),
                        EsCorrecta =
                            respuestaCorrecta,
                        FechaRespuestaUtc =
                            DateTime.UtcNow
                    };

                _context.QuizRespuestas.Add(
                    respuesta
                );

                desafioActual.EsActual = false;

                ActualizarSesion(
                    sesion,
                    quiz
                );

                await _context.SaveChangesAsync();

                if (quiz.Estado == EstadoQuiz.Activo)
                {
                    int siguienteNumero =
                        sesion.Desafios
                            .Max(x => x.Numero) + 1;

                    QuizDesafioEntity nuevoDesafio =
                        CrearDesafio(
                            quiz,
                            siguienteNumero
                        );

                    sesion.Desafios.Add(
                        nuevoDesafio
                    );

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
        }

        private static QuizSesionEntity CrearSesion(
            QuizDominio quiz
        )
        {
            QuizSesionEntity sesion =
                new QuizSesionEntity
                {
                    Id = quiz.Id,
                    CuitEmpresa =
                        quiz.CuitEmpresa.ToString(),
                    Estado = (int)quiz.Estado,
                    IntentosTotales =
                        quiz.IntentosTotales,
                    IntentosRestantes =
                        quiz.IntentosRestantes,
                    FechaCreacionUtc =
                        quiz.FechaAlta,
                    FechaExpiracionUtc =
                        quiz.FechaExpiracionUtc,
                    FechaFinalizacionUtc = null
                };

            foreach (
                Cuil cuil in quiz.CuilesVinculados
            )
            {
                sesion.CuilesVinculados.Add(
                    new QuizCuilVinculadoEntity
                    {
                        QuizId = quiz.Id,
                        Cuil = cuil.ToString()
                    }
                );
            }

            sesion.Desafios.Add(
                CrearDesafio(
                    quiz,
                    1
                )
            );

            return sesion;
        }

        private static QuizDesafioEntity CrearDesafio(
            QuizDominio quiz,
            int numero
        )
        {
            Guid desafioId = Guid.NewGuid();

            QuizDesafioEntity desafio =
                new QuizDesafioEntity
                {
                    Id = desafioId,
                    QuizId = quiz.Id,
                    Numero = numero,
                    Escenario =
                        (int)quiz.Escenario,
                    EsActual = true,
                    FechaCreacionUtc =
                        DateTime.UtcNow
                };

            int orden = 1;

            foreach (
                OpcionQuiz opcion in quiz.Opciones
            )
            {
                desafio.Opciones.Add(
                    new QuizOpcionEntity
                    {
                        QuizDesafioId =
                            desafioId,
                        Id = opcion.Id,
                        Cuil =
                            opcion.Cuil.ToString(),
                        EsVinculado =
                            opcion.EsVinculado,
                        Orden = orden
                    }
                );

                orden++;
            }

            return desafio;
        }

        private static void ActualizarSesion(
            QuizSesionEntity sesion,
            QuizDominio quiz
        )
        {
            sesion.Estado =
                (int)quiz.Estado;

            sesion.IntentosTotales =
                quiz.IntentosTotales;

            sesion.IntentosRestantes =
                quiz.IntentosRestantes;

            sesion.FechaExpiracionUtc =
                quiz.FechaExpiracionUtc;

            if (
                quiz.Estado != EstadoQuiz.Activo &&
                sesion.FechaFinalizacionUtc == null
            )
            {
                sesion.FechaFinalizacionUtc =
                    DateTime.UtcNow;
            }
        }
    }
}