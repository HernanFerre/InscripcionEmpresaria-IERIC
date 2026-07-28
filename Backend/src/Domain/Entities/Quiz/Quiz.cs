using System;
using System.Collections.Generic;
using System.Linq;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.SeedWork;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Entities
{
    public class Quiz : Entity, IAggregateRoot
    {
        private readonly List<Cuil> _cuilesVinculados =
            new List<Cuil>();

        private readonly List<OpcionQuiz> _opciones =
            new List<OpcionQuiz>();

        public Cuit CuitEmpresa { get; private set; }

        public EscenarioQuiz Escenario { get; private set; }

        public int IntentosTotales { get; private set; }

        public int IntentosRestantes { get; private set; }

        public bool Validado { get; private set; }

        public DateTime FechaExpiracionUtc { get; private set; }

        public IReadOnlyCollection<Cuil> CuilesVinculados =>
            _cuilesVinculados.AsReadOnly();

        public IReadOnlyCollection<OpcionQuiz> Opciones =>
            _opciones.AsReadOnly();

        public bool LimiteExcedido =>
            IntentosRestantes <= 0;

        public bool EstaExpirado =>
            DateTime.UtcNow >= FechaExpiracionUtc;

        private Quiz()
        {
        }

        public Quiz(
            Cuit cuitEmpresa,
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

            Id = Guid.NewGuid();
            Activo = true;
            FechaAlta = DateTime.UtcNow;

            CuitEmpresa = cuitEmpresa;
            IntentosTotales = intentosTotales;
            IntentosRestantes = intentosTotales;
            Validado = false;
            FechaExpiracionUtc = DateTime.UtcNow.AddMinutes(10);

            _cuilesVinculados.AddRange(cuiles);

            CambiarDesafio(escenario, opciones);
        }

        public bool ValidarRespuesta(
            IEnumerable<string> opcionesSeleccionadas
        )
        {
            if (EstaExpirado)
            {
                throw new SumariosDomainException(
                    "El quiz ha expirado"
                );
            }

            if (LimiteExcedido)
            {
                throw new SumariosDomainException(
                    "El quiz no tiene intentos disponibles"
                );
            }

            if (Validado)
            {
                return true;
            }

            if (
                opcionesSeleccionadas == null ||
                !opcionesSeleccionadas.Any()
            )
            {
                throw new SumariosDomainException(
                    "Debe seleccionar al menos una opción"
                );
            }

            HashSet<string> seleccionadas =
                new HashSet<string>(
                    opcionesSeleccionadas,
                    StringComparer.OrdinalIgnoreCase
                );

            HashSet<string> correctas =
                ObtenerRespuestasCorrectas();

            bool respuestaCorrecta =
                seleccionadas.SetEquals(correctas);

            if (respuestaCorrecta)
            {
                Validado = true;
                return true;
            }

            IntentosRestantes--;

            return false;
        }

        public void CambiarDesafio(
            EscenarioQuiz escenario,
            IEnumerable<OpcionQuiz> opciones
        )
        {
            if (Validado)
            {
                throw new SumariosDomainException(
                    "No se puede modificar un quiz validado"
                );
            }

            if (LimiteExcedido)
            {
                throw new SumariosDomainException(
                    "No se puede modificar un quiz sin intentos"
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
                    .Any(grupo => grupo.Count() > 1);

            if (identificadoresRepetidos)
            {
                throw new SumariosDomainException(
                    "Las opciones del quiz no pueden repetirse"
                );
            }

            ValidarEscenario(escenario, nuevasOpciones);

            Escenario = escenario;

            _opciones.Clear();
            _opciones.AddRange(nuevasOpciones);
        }

        private void ValidarEscenario(
            EscenarioQuiz escenario,
            IEnumerable<OpcionQuiz> opciones
        )
        {
            int cantidadVinculadas =
                opciones.Count(opcion => opcion.EsVinculado);

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
            if (Escenario == EscenarioQuiz.TodasCorrectas)
            {
                return new HashSet<string>(
                    new[] { "todas" },
                    StringComparer.OrdinalIgnoreCase
                );
            }

            if (Escenario == EscenarioQuiz.NingunaCorrecta)
            {
                return new HashSet<string>(
                    new[] { "ninguna" },
                    StringComparer.OrdinalIgnoreCase
                );
            }

            return new HashSet<string>(
                _opciones
                    .Where(opcion => opcion.EsVinculado)
                    .Select(opcion => opcion.Id),
                StringComparer.OrdinalIgnoreCase
            );
        }
    }
}