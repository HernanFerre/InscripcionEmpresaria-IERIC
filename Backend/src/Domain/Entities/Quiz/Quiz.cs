using System;
using System.Collections.Generic;
using System.Linq;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.SeedWork;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Entities
{
    public class Quiz : IAggregateRoot
    {
        private readonly List<Cuil> _cuilesVinculados =
            new List<Cuil>();

        private readonly List<OpcionQuiz> _opciones =
            new List<OpcionQuiz>();

        public long Id { get; private set; }

        public string UsuarioId { get; private set; }

        public Cuit CuitEmpresa { get; private set; }

        public EscenarioQuiz Escenario { get; private set; }

        public EstadoQuiz Estado { get; private set; }

        public int IntentosTotales { get; private set; }

        public int IntentosRealizados { get; private set; }

        public int IntentosRestantes =>
            Math.Max(
                0,
                IntentosTotales - IntentosRealizados
            );

        public bool Validado =>
            Estado == EstadoQuiz.Validado;

        public DateTime FechaCreacion { get; private set; }

        public DateTime FechaExpiracion { get; private set; }

        public DateTime? BloqueadoHasta { get; private set; }

        public IReadOnlyCollection<Cuil> CuilesVinculados =>
            _cuilesVinculados.AsReadOnly();

        public IReadOnlyCollection<OpcionQuiz> Opciones =>
            _opciones.AsReadOnly();

        public bool LimiteExcedido =>
            Estado == EstadoQuiz.Bloqueado ||
            IntentosRestantes <= 0;

        public bool EstaExpirado =>
            Estado == EstadoQuiz.Expirado ||
            DateTime.Now >= FechaExpiracion;

        private Quiz()
        {
        }

        public Quiz(
            Cuit cuitEmpresa,
            string usuarioId,
            IEnumerable<Cuil> cuilesVinculados,
            EscenarioQuiz escenario,
            IEnumerable<OpcionQuiz> opciones,
            int intentosTotales = 3
        )
        {
            if (cuitEmpresa == null)
            {
                throw new SumariosDomainException(
                    "El quiz debe estar asociado a un CUIT"
                );
            }

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                throw new SumariosDomainException(
                    "El quiz debe estar asociado a un usuario"
                );
            }

            string usuarioNormalizado = usuarioId.Trim();

            if (usuarioNormalizado.Length > 200)
            {
                throw new SumariosDomainException(
                    "El identificador del usuario supera " +
                    "la longitud permitida"
                );
            }

            if (intentosTotales <= 0)
            {
                throw new SumariosDomainException(
                    "El quiz debe permitir al menos un intento"
                );
            }

            List<Cuil> cuiles = cuilesVinculados?
                .GroupBy(cuil => cuil.ToInt64())
                .Select(grupo => grupo.First())
                .ToList();

            if (cuiles == null || cuiles.Count == 0)
            {
                throw new SumariosDomainException(
                    "El quiz debe recibir al menos un CUIL vinculado"
                );
            }

            Id = 0;
            UsuarioId = usuarioNormalizado;
            CuitEmpresa = cuitEmpresa;
            Estado = EstadoQuiz.Activo;
            IntentosTotales = intentosTotales;
            IntentosRealizados = 0;
            FechaCreacion = DateTime.Now;
            FechaExpiracion = DateTime.Now.AddMinutes(10);
            BloqueadoHasta = null;

            _cuilesVinculados.AddRange(cuiles);

            CambiarDesafio(
                escenario,
                opciones
            );
        }

        public static Quiz Restaurar(
            long id,
            string usuarioId,
            Cuit cuitEmpresa,
            IEnumerable<Cuil> cuilesVinculados,
            EscenarioQuiz escenario,
            IEnumerable<OpcionQuiz> opciones,
            EstadoQuiz estado,
            int intentosTotales,
            int intentosRealizados,
            DateTime fechaCreacion,
            DateTime fechaExpiracion,
            DateTime? bloqueadoHasta
        )
        {
            if (id <= 0)
            {
                throw new SumariosDomainException(
                    "El identificador del quiz no es válido"
                );
            }

            if (
                intentosRealizados < 0 ||
                intentosRealizados > intentosTotales
            )
            {
                throw new SumariosDomainException(
                    "La cantidad de intentos realizados no es válida"
                );
            }

            if (!Enum.IsDefined(typeof(EstadoQuiz), estado))
            {
                throw new SumariosDomainException(
                    "El estado almacenado del quiz no es válido"
                );
            }

            Quiz quiz = new Quiz(
                cuitEmpresa,
                usuarioId,
                cuilesVinculados,
                escenario,
                opciones,
                intentosTotales
            );

            quiz.Id = id;
            quiz.Estado = estado;
            quiz.IntentosRealizados = intentosRealizados;
            quiz.FechaCreacion = fechaCreacion;
            quiz.FechaExpiracion = fechaExpiracion;
            quiz.BloqueadoHasta = bloqueadoHasta;

            return quiz;
        }

        public void AsignarId(long id)
        {
            if (id <= 0)
            {
                throw new SumariosDomainException(
                    "El identificador del quiz no es válido"
                );
            }

            if (Id != 0)
            {
                throw new SumariosDomainException(
                    "El quiz ya tiene un identificador asignado"
                );
            }

            Id = id;
        }

        public bool ValidarRespuesta(
            IEnumerable<string> opcionesSeleccionadas,
            TimeSpan? duracionBloqueo
        )
        {
            MarcarComoExpirado();

            if (Estado == EstadoQuiz.Expirado)
            {
                throw new SumariosDomainException(
                    "El quiz ha expirado"
                );
            }

            if (Estado == EstadoQuiz.Validado)
            {
                return true;
            }

            if (LimiteExcedido)
            {
                Estado = EstadoQuiz.Bloqueado;

                throw new SumariosDomainException(
                    "El quiz no tiene intentos disponibles"
                );
            }

            if (
                duracionBloqueo.HasValue &&
                duracionBloqueo.Value <= TimeSpan.Zero
            )
            {
                throw new SumariosDomainException(
                    "La duración del bloqueo debe ser mayor a cero"
                );
            }

            List<string> opcionesNormalizadas =
                opcionesSeleccionadas?
                    .Where(
                        opcion =>
                            !string.IsNullOrWhiteSpace(opcion)
                    )
                    .Select(
                        opcion =>
                            opcion
                                .Trim()
                                .ToLowerInvariant()
                    )
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase
                    )
                    .ToList();

            if (
                opcionesNormalizadas == null ||
                opcionesNormalizadas.Count == 0
            )
            {
                throw new SumariosDomainException(
                    "Debe seleccionar al menos una opción"
                );
            }

            HashSet<string> seleccionadas =
                new HashSet<string>(
                    opcionesNormalizadas,
                    StringComparer.OrdinalIgnoreCase
                );

            ValidarOpcionesSeleccionadas(
                seleccionadas
            );

            HashSet<string> correctas =
                ObtenerRespuestasCorrectas();

            bool respuestaCorrecta =
                seleccionadas.SetEquals(correctas);

            IntentosRealizados++;

            if (respuestaCorrecta)
            {
                Estado = EstadoQuiz.Validado;

                return true;
            }

            if (IntentosRestantes <= 0)
            {
                Estado = EstadoQuiz.Bloqueado;

                BloqueadoHasta =
                    duracionBloqueo.HasValue
                        ? DateTime.Now.Add(
                            duracionBloqueo.Value
                        )
                        : null;
            }

            return false;
        }

        public void CambiarDesafio(
            EscenarioQuiz escenario,
            IEnumerable<OpcionQuiz> opciones
        )
        {
            if (Estado == EstadoQuiz.Validado)
            {
                throw new SumariosDomainException(
                    "No se puede modificar un quiz validado"
                );
            }

            if (Estado == EstadoQuiz.Bloqueado)
            {
                throw new SumariosDomainException(
                    "No se puede modificar un quiz sin intentos"
                );
            }

            if (Estado == EstadoQuiz.Expirado)
            {
                throw new SumariosDomainException(
                    "No se puede modificar un quiz expirado"
                );
            }

            List<OpcionQuiz> nuevasOpciones =
                opciones?.ToList();

            if (
                nuevasOpciones == null ||
                nuevasOpciones.Count != 4
            )
            {
                throw new SumariosDomainException(
                    "El quiz debe contener exactamente cuatro opciones"
                );
            }

            bool identificadoresRepetidos =
                nuevasOpciones
                    .GroupBy(
                        opcion => opcion.Id,
                        StringComparer.OrdinalIgnoreCase
                    )
                    .Any(
                        grupo => grupo.Count() > 1
                    );

            if (identificadoresRepetidos)
            {
                throw new SumariosDomainException(
                    "Las opciones del quiz no pueden repetirse"
                );
            }

            ValidarEscenario(
                escenario,
                nuevasOpciones
            );

            Escenario = escenario;

            _opciones.Clear();
            _opciones.AddRange(nuevasOpciones);
        }

        public void MarcarComoExpirado()
        {
            if (
                Estado == EstadoQuiz.Activo &&
                DateTime.Now >= FechaExpiracion
            )
            {
                Estado = EstadoQuiz.Expirado;
            }
        }

        private void ValidarOpcionesSeleccionadas(
            HashSet<string> seleccionadas
        )
        {
            HashSet<string> opcionesDisponibles =
                new HashSet<string>(
                    _opciones.Select(opcion => opcion.Id),
                    StringComparer.OrdinalIgnoreCase
                )
                {
                    "ninguna",
                    "todas"
                };

            if (
                seleccionadas.Any(
                    opcion =>
                        !opcionesDisponibles.Contains(opcion)
                )
            )
            {
                throw new SumariosDomainException(
                    "La respuesta contiene una opción inválida"
                );
            }

            bool incluyeOpcionEspecial =
                seleccionadas.Contains("ninguna") ||
                seleccionadas.Contains("todas");

            if (
                incluyeOpcionEspecial &&
                seleccionadas.Count > 1
            )
            {
                throw new SumariosDomainException(
                    "Las opciones 'Ninguna' y 'Todas' " +
                    "deben seleccionarse individualmente"
                );
            }
        }

        private void ValidarEscenario(
            EscenarioQuiz escenario,
            IEnumerable<OpcionQuiz> opciones
        )
        {
            int cantidadVinculadas =
                opciones.Count(
                    opcion => opcion.EsVinculado
                );

            bool escenarioValido =
                escenario switch
                {
                    EscenarioQuiz.UnaCorrecta =>
                        cantidadVinculadas == 1,

                    EscenarioQuiz.DosCorrectas =>
                        cantidadVinculadas == 2,

                    EscenarioQuiz.TodasCorrectas =>
                        cantidadVinculadas == 4,

                    EscenarioQuiz.NingunaCorrecta =>
                        cantidadVinculadas == 0,

                    _ => false
                };

            if (!escenarioValido)
            {
                throw new SumariosDomainException(
                    "Las opciones no coinciden con el escenario del quiz"
                );
            }
        }

        private HashSet<string> ObtenerRespuestasCorrectas()
        {
            if (
                Escenario ==
                EscenarioQuiz.TodasCorrectas
            )
            {
                return new HashSet<string>(
                    new[] { "todas" },
                    StringComparer.OrdinalIgnoreCase
                );
            }

            if (
                Escenario ==
                EscenarioQuiz.NingunaCorrecta
            )
            {
                return new HashSet<string>(
                    new[] { "ninguna" },
                    StringComparer.OrdinalIgnoreCase
                );
            }

            return new HashSet<string>(
                _opciones
                    .Where(
                        opcion =>
                            opcion.EsVinculado
                    )
                    .Select(
                        opcion =>
                            opcion.Id
                    ),
                StringComparer.OrdinalIgnoreCase
            );
        }
    }
}