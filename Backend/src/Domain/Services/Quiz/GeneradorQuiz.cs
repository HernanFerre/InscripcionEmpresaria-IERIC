using System;
using System.Collections.Generic;
using System.Linq;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Services
{
    public class GeneradorQuiz : IGeneradorQuiz
    {
        private readonly IAleatoriedadQuiz _aleatoriedad;

        private readonly string[] _identificadores =
        {
            "a", "b", "c", "d"
        };

        private readonly string[] _prefijosCuil =
        {
            "20", "23", "24", "27"
        };

        public GeneradorQuiz(IAleatoriedadQuiz aleatoriedad)
        {
            _aleatoriedad =
                aleatoriedad ??
                throw new ArgumentNullException(
                    nameof(aleatoriedad)
                );
        }

        public Quiz CrearQuiz(
            Cuit cuitEmpresa,
            string usuarioId,
            IEnumerable<Cuil> cuilesVinculados,
            int intentosTotales = 3
        )
        {
            List<Cuil> cuiles =
                PrepararCuiles(
                    cuilesVinculados
                );

            EscenarioQuiz escenario =
                SeleccionarEscenario(
                    cuiles.Count
                );

            List<OpcionQuiz> opciones =
                GenerarOpciones(
                    escenario,
                    cuiles
                );

            return new Quiz(
                cuitEmpresa,
                usuarioId,
                cuiles,
                escenario,
                opciones,
                intentosTotales
            );
        }

        public void GenerarNuevoDesafio(Quiz quiz)
        {
            if (quiz == null)
            {
                throw new SumariosDomainException(
                    "El quiz no puede ser nulo"
                );
            }

            EscenarioQuiz escenario =
                SeleccionarEscenario(
                    quiz.CuilesVinculados.Count,
                    quiz.Escenario
                );

            List<OpcionQuiz> opciones =
                GenerarOpciones(
                    escenario,
                    quiz.CuilesVinculados
                );

            quiz.CambiarDesafio(escenario, opciones);
        }

        private List<Cuil> PrepararCuiles(
            IEnumerable<Cuil> cuilesVinculados
        )
        {
            List<Cuil> cuiles = cuilesVinculados?
                .GroupBy(cuil => cuil.ToInt64())
                .Select(grupo => grupo.First())
                .ToList();

            if (cuiles == null || cuiles.Count == 0)
            {
                throw new SumariosDomainException(
                    "Se necesita al menos un CUIL vinculado"
                );
            }

            return cuiles;
        }

        private EscenarioQuiz SeleccionarEscenario(
            int cantidadCuiles,
            EscenarioQuiz? escenarioAnterior = null
        )
        {
            List<EscenarioQuiz> escenariosDisponibles =
                ObtenerEscenariosDisponibles(cantidadCuiles);

            for (int intento = 0; intento < 20; intento++)
            {
                int numero =
                    _aleatoriedad.ObtenerEntero(0, 100);

                EscenarioQuiz escenario =
                    ObtenerEscenarioPorNumero(numero);

                bool estaDisponible =
                    escenariosDisponibles.Contains(escenario);

                bool repiteEscenario =
                    escenarioAnterior.HasValue &&
                    escenarioAnterior.Value == escenario &&
                    escenariosDisponibles.Count > 1;

                if (estaDisponible && !repiteEscenario)
                {
                    return escenario;
                }
            }

            List<EscenarioQuiz> alternativas =
                escenariosDisponibles
                    .Where(
                        escenario =>
                            !escenarioAnterior.HasValue ||
                            escenario != escenarioAnterior.Value
                    )
                    .ToList();

            if (alternativas.Count == 0)
            {
                alternativas = escenariosDisponibles;
            }

            int indice =
                _aleatoriedad.ObtenerEntero(
                    0,
                    alternativas.Count
                );

            return alternativas[indice];
        }

        private List<EscenarioQuiz>
            ObtenerEscenariosDisponibles(int cantidadCuiles)
        {
            List<EscenarioQuiz> escenarios =
                new List<EscenarioQuiz>
                {
                    EscenarioQuiz.UnaCorrecta,
                    EscenarioQuiz.NingunaCorrecta
                };

            if (cantidadCuiles >= 2)
            {
                escenarios.Add(
                    EscenarioQuiz.DosCorrectas
                );
            }

            if (cantidadCuiles >= 4)
            {
                escenarios.Add(
                    EscenarioQuiz.TodasCorrectas
                );
            }

            return escenarios;
        }

        private EscenarioQuiz ObtenerEscenarioPorNumero(
            int numero
        )
        {
            if (numero < 40)
            {
                return EscenarioQuiz.UnaCorrecta;
            }

            if (numero < 80)
            {
                return EscenarioQuiz.DosCorrectas;
            }

            if (numero < 90)
            {
                return EscenarioQuiz.TodasCorrectas;
            }

            return EscenarioQuiz.NingunaCorrecta;
        }

        private List<OpcionQuiz> GenerarOpciones(
            EscenarioQuiz escenario,
            IEnumerable<Cuil> cuilesVinculados
        )
        {
            List<Cuil> cuiles =
                cuilesVinculados.ToList();

            int cantidadCorrectas =
                ObtenerCantidadCorrectas(escenario);

            List<Cuil> cuilesMezclados =
                new List<Cuil>(cuiles);

            Mezclar(cuilesMezclados);

            List<(Cuil Cuil, bool EsVinculado)>
                opcionesTemporales =
                    new List<(Cuil Cuil, bool EsVinculado)>();

            foreach (
                Cuil cuil in
                cuilesMezclados.Take(cantidadCorrectas)
            )
            {
                opcionesTemporales.Add(
                    (cuil, true)
                );
            }

            HashSet<long> numerosExcluidos =
                new HashSet<long>(
                    cuiles.Select(cuil => cuil.ToInt64())
                );

            while (opcionesTemporales.Count < 4)
            {
                Cuil cuilInventado =
                    GenerarCuilInventado(numerosExcluidos);

                opcionesTemporales.Add(
                    (cuilInventado, false)
                );
            }

            Mezclar(opcionesTemporales);

            List<OpcionQuiz> opciones =
                new List<OpcionQuiz>();

            for (
                int indice = 0;
                indice < opcionesTemporales.Count;
                indice++
            )
            {
                var opcion = opcionesTemporales[indice];

                opciones.Add(
                    new OpcionQuiz(
                        _identificadores[indice],
                        opcion.Cuil,
                        opcion.EsVinculado
                    )
                );
            }

            return opciones;
        }

        private int ObtenerCantidadCorrectas(
            EscenarioQuiz escenario
        )
        {
            return escenario switch
            {
                EscenarioQuiz.UnaCorrecta => 1,
                EscenarioQuiz.DosCorrectas => 2,
                EscenarioQuiz.TodasCorrectas => 4,
                EscenarioQuiz.NingunaCorrecta => 0,
                _ => throw new SumariosDomainException(
                    "El escenario del quiz no es válido"
                )
            };
        }

        private Cuil GenerarCuilInventado(
            HashSet<long> numerosExcluidos
        )
        {
            for (int intento = 0; intento < 1000; intento++)
            {
                int indicePrefijo =
                    _aleatoriedad.ObtenerEntero(
                        0,
                        _prefijosCuil.Length
                    );

                int documento =
                    _aleatoriedad.ObtenerEntero(
                        0,
                        100000000
                    );

                string baseCuil =
                    _prefijosCuil[indicePrefijo] +
                    documento.ToString("D8");

                int digitoVerificador =
                    CalcularDigitoVerificador(baseCuil);

                long numeroCompleto =
                    long.Parse(
                        baseCuil +
                        digitoVerificador
                    );

                if (numerosExcluidos.Contains(numeroCompleto))
                {
                    continue;
                }

                numerosExcluidos.Add(numeroCompleto);

                return new Cuil(numeroCompleto);
            }

            throw new SumariosDomainException(
                "No fue posible generar un CUIL alternativo"
            );
        }

        private int CalcularDigitoVerificador(
            string primerosDiezDigitos
        )
        {
            int[] multiplicadores =
            {
                5, 4, 3, 2, 7, 6, 5, 4, 3, 2
            };

            int suma = 0;

            for (
                int indice = 0;
                indice < multiplicadores.Length;
                indice++
            )
            {
                suma +=
                    int.Parse(
                        primerosDiezDigitos[indice].ToString()
                    ) *
                    multiplicadores[indice];
            }

            int resto = suma % 11;

            return resto == 0
                ? 0
                : resto == 1
                    ? 9
                    : 11 - resto;
        }

        private void Mezclar<T>(IList<T> elementos)
        {
            for (
                int indice = elementos.Count - 1;
                indice > 0;
                indice--
            )
            {
                int indiceAleatorio =
                    _aleatoriedad.ObtenerEntero(
                        0,
                        indice + 1
                    );

                T temporal = elementos[indice];

                elementos[indice] =
                    elementos[indiceAleatorio];

                elementos[indiceAleatorio] =
                    temporal;
            }
        }
    }
}