using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
using IERIC.SumariosIERIC.Infrastructure.Persistence.Quiz;
using Microsoft.EntityFrameworkCore;
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
                throw new ArgumentNullException(
                    nameof(quiz)
                );
            }

            if (quiz.Id == 0)
            {
                QuizSesionEntity nuevaSesion =
                    CrearSesion(quiz);

                _context.QuizSesiones.Add(
                    nuevaSesion
                );

                await _context.SaveChangesAsync();

                quiz.AsignarId(
                    nuevaSesion.Id
                );

                return;
            }

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

            ActualizarSesion(
                sesion,
                quiz
            );

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
            long quizId
        )
        {
            QuizSesionEntity sesion =
                await _context.QuizSesiones
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(
                        x => x.CuilesVinculados
                    )
                    .Include(
                        x => x.Desafios
                    )
                        .ThenInclude(
                            x => x.Opciones
                        )
                    .Include(
                        x => x.Desafios
                    )
                        .ThenInclude(
                            x => x.Respuesta
                        )
                    .SingleOrDefaultAsync(
                        x => x.Id == quizId
                    );

            if (sesion == null)
            {
                return null;
            }

            QuizDesafioEntity desafio =
                sesion.Desafios
                    .Where(
                        x => x.EsActual
                    )
                    .OrderByDescending(
                        x => x.Numero
                    )
                    .FirstOrDefault()
                ??
                sesion.Desafios
                    .OrderByDescending(
                        x => x.Numero
                    )
                    .FirstOrDefault();

            if (desafio == null)
            {
                throw new InvalidOperationException(
                    "La sesión almacenada no contiene desafíos."
                );
            }

            List<QuizOpcionEntity> opcionesPersistidas =
                desafio.Opciones
                    .Where(
                        x => x.CodigoOpcion <= 3
                    )
                    .OrderBy(
                        x => x.CodigoOpcion
                    )
                    .ToList();

            if (
                opcionesPersistidas.Count != 4 ||
                opcionesPersistidas.Any(
                    x => !x.Cuil.HasValue
                )
            )
            {
                throw new InvalidOperationException(
                    "El desafío almacenado no contiene " +
                    "las cuatro opciones de CUIL esperadas."
                );
            }

            Cuit cuitEmpresa =
                new Cuit(
                    sesion.CuitEmpresa
                );

            List<Cuil> cuilesVinculados =
                sesion.CuilesVinculados
                    .Select(
                        x => new Cuil(
                            x.Cuil
                        )
                    )
                    .ToList();

            List<OpcionQuiz> opciones =
                opcionesPersistidas
                    .Select(
                        x => new OpcionQuiz(
                            ConvertirCodigoEnId(
                                x.CodigoOpcion
                            ),
                            new Cuil(
                                x.Cuil.Value
                            ),
                            x.EsVinculado
                        )
                    )
                    .ToList();

            int intentosRealizados =
                sesion.Desafios.Count(
                    x => x.Respuesta != null
                );

            return QuizDominio.Restaurar(
                sesion.Id,
                sesion.UsuarioId,
                cuitEmpresa,
                cuilesVinculados,
                (EscenarioQuiz)desafio.Escenario,
                opciones,
                (EstadoQuiz)sesion.Estado,
                sesion.IntentosTotales,
                intentosRealizados,
                sesion.FechaCreacion,
                sesion.FechaExpiracion,
                sesion.BloqueadoHasta
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
                throw new ArgumentNullException(
                    nameof(quiz)
                );
            }

            List<string> seleccionadas =
                opcionesSeleccionadas?
                    .Where(
                        x => !string.IsNullOrWhiteSpace(x)
                    )
                    .Select(
                        x => x
                            .Trim()
                            .ToLowerInvariant()
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

            List<byte> codigosSeleccionados =
                seleccionadas
                    .Select(
                        ConvertirIdEnCodigo
                    )
                    .Distinct()
                    .ToList();

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                QuizSesionEntity sesion =
                    await _context.QuizSesiones
                        .Include(
                            x => x.Desafios
                        )
                            .ThenInclude(
                                x => x.Opciones
                            )
                        .Include(
                            x => x.Desafios
                        )
                            .ThenInclude(
                                x => x.Respuesta
                            )
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

                if (desafioActual.Respuesta != null)
                {
                    throw new InvalidOperationException(
                        "El desafío actual ya tiene una respuesta."
                    );
                }

                bool contieneOpcionesInvalidas =
                    codigosSeleccionados.Any(
                        codigo =>
                            !desafioActual.Opciones.Any(
                                opcion =>
                                    opcion.CodigoOpcion ==
                                    codigo
                            )
                    );

                if (contieneOpcionesInvalidas)
                {
                    throw new InvalidOperationException(
                        "La respuesta contiene opciones que " +
                        "no pertenecen al desafío actual."
                    );
                }

                QuizRespuestaEntity respuesta =
                    new QuizRespuestaEntity
                    {
                        Id = 0,
                        QuizDesafioId =
                            desafioActual.Id,
                        EsCorrecta =
                            respuestaCorrecta,
                        FechaRespuesta =
                            DateTime.Now
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

                foreach (
                    byte codigo in codigosSeleccionados
                )
                {
                    _context
                        .QuizRespuestasOpciones
                        .Add(
                            new QuizRespuestaOpcionEntity
                            {
                                QuizRespuestaId =
                                    respuesta.Id,
                                QuizDesafioId =
                                    desafioActual.Id,
                                CodigoOpcion =
                                    codigo
                            }
                        );
                }

                if (quiz.Estado == EstadoQuiz.Activo)
                {
                    int siguienteNumero =
                        sesion.Desafios
                            .Max(
                                x => (int)x.Numero
                            ) + 1;

                    QuizDesafioEntity nuevoDesafio =
                        CrearDesafio(
                            quiz,
                            siguienteNumero
                        );

                    nuevoDesafio.QuizSesion =
                        sesion;

                    sesion.Desafios.Add(
                        nuevoDesafio
                    );
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
        }

        public async Task<(
            bool EstaBloqueado,
            DateTime? BloqueadoHasta
        )> ObtenerBloqueoVigenteAsync(
            Cuit cuitEmpresa
        )
        {
            if (cuitEmpresa == null)
            {
                throw new ArgumentNullException(
                    nameof(cuitEmpresa)
                );
            }

            long numeroCuit =
                cuitEmpresa.ToInt64();

            DateTime fechaActual =
                DateTime.Now;

            var bloqueo =
                await _context.QuizSesiones
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.CuitEmpresa == numeroCuit &&
                            x.Estado ==
                                (byte)EstadoQuiz.Bloqueado &&
                            (
                                !x.BloqueadoHasta.HasValue ||
                                x.BloqueadoHasta.Value >
                                    fechaActual
                            )
                    )
                    .OrderByDescending(
                        x => x.FechaCreacion
                    )
                    .Select(
                        x => new
                        {
                            x.BloqueadoHasta
                        }
                    )
                    .FirstOrDefaultAsync();

            if (bloqueo == null)
            {
                return (
                    false,
                    null
                );
            }

            return (
                true,
                bloqueo.BloqueadoHasta
            );
        }

        private static QuizSesionEntity CrearSesion(
            QuizDominio quiz
        )
        {
            QuizSesionEntity sesion =
                new QuizSesionEntity
                {
                    Id = 0,
                    CuitEmpresa =
                        quiz.CuitEmpresa.ToInt64(),
                    UsuarioId =
                        quiz.UsuarioId,
                    Estado =
                        (byte)quiz.Estado,
                    IntentosTotales =
                        checked(
                            (byte)quiz.IntentosTotales
                        ),
                    FechaCreacion =
                        quiz.FechaCreacion,
                    FechaExpiracion =
                        quiz.FechaExpiracion,
                    FechaFinalizacion =
                        null,
                    BloqueadoHasta =
                        quiz.BloqueadoHasta
                };

            foreach (
                Cuil cuil in quiz.CuilesVinculados
            )
            {
                QuizCuilVinculadoEntity vinculado =
                    new QuizCuilVinculadoEntity
                    {
                        QuizSesionId = 0,
                        Cuil = cuil.ToInt64(),
                        QuizSesion = sesion
                    };

                sesion.CuilesVinculados.Add(
                    vinculado
                );
            }

            QuizDesafioEntity desafio =
                CrearDesafio(
                    quiz,
                    1
                );

            desafio.QuizSesion = sesion;

            sesion.Desafios.Add(
                desafio
            );

            return sesion;
        }

        private static QuizDesafioEntity CrearDesafio(
            QuizDominio quiz,
            int numero
        )
        {
            QuizDesafioEntity desafio =
                new QuizDesafioEntity
                {
                    Id = 0,
                    QuizSesionId =
                        quiz.Id,
                    Numero =
                        checked(
                            (byte)numero
                        ),
                    Escenario =
                        checked(
                            (byte)quiz.Escenario
                        ),
                    EsActual = true,
                    FechaCreacion =
                        DateTime.Now
                };

            foreach (
                OpcionQuiz opcion in quiz.Opciones
            )
            {
                byte codigo =
                    ConvertirIdEnCodigo(
                        opcion.Id
                    );

                if (codigo > 3)
                {
                    throw new InvalidOperationException(
                        "Las opciones de CUIL deben utilizar " +
                        "los códigos A, B, C o D."
                    );
                }

                desafio.Opciones.Add(
                    new QuizOpcionEntity
                    {
                        QuizDesafioId = 0,
                        CodigoOpcion = codigo,
                        Cuil =
                            opcion.Cuil.ToInt64(),
                        EsVinculado =
                            opcion.EsVinculado,
                        Desafio =
                            desafio
                    }
                );
            }

            desafio.Opciones.Add(
                new QuizOpcionEntity
                {
                    QuizDesafioId = 0,
                    CodigoOpcion = 4,
                    Cuil = null,
                    EsVinculado =
                        quiz.Escenario ==
                        EscenarioQuiz.NingunaCorrecta,
                    Desafio = desafio
                }
            );

            desafio.Opciones.Add(
                new QuizOpcionEntity
                {
                    QuizDesafioId = 0,
                    CodigoOpcion = 5,
                    Cuil = null,
                    EsVinculado =
                        quiz.Escenario ==
                        EscenarioQuiz.TodasCorrectas,
                    Desafio = desafio
                }
            );

            return desafio;
        }

        private static void ActualizarSesion(
            QuizSesionEntity sesion,
            QuizDominio quiz
        )
        {
            sesion.Estado =
                (byte)quiz.Estado;

            sesion.IntentosTotales =
                checked(
                    (byte)quiz.IntentosTotales
                );

            sesion.FechaExpiracion =
                quiz.FechaExpiracion;

            sesion.BloqueadoHasta =
                quiz.BloqueadoHasta;

            if (
                quiz.Estado != EstadoQuiz.Activo &&
                sesion.FechaFinalizacion == null
            )
            {
                sesion.FechaFinalizacion =
                    DateTime.Now;
            }
        }

        private static byte ConvertirIdEnCodigo(
            string opcionId
        )
        {
            string idNormalizado =
                opcionId?
                    .Trim()
                    .ToLowerInvariant();

            return idNormalizado switch
            {
                "a" => 0,
                "b" => 1,
                "c" => 2,
                "d" => 3,
                "ninguna" => 4,
                "todas" => 5,

                _ => throw new InvalidOperationException(
                    "El identificador de opción no es válido."
                )
            };
        }

        private static string ConvertirCodigoEnId(
            byte codigoOpcion
        )
        {
            return codigoOpcion switch
            {
                0 => "a",
                1 => "b",
                2 => "c",
                3 => "d",
                4 => "ninguna",
                5 => "todas",

                _ => throw new InvalidOperationException(
                    "El código de opción almacenado no es válido."
                )
            };
        }
    }
}