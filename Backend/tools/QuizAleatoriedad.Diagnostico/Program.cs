using System;
using System.Collections.Generic;
using System.Linq;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.Services;
using IERIC.SumariosIERIC.Domain.ValueObjects;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
using IERIC.SumariosIERIC.Infrastructure.Services;

const int cantidadPruebas = 100_000;

GeneradorQuiz generador = new GeneradorQuiz(
    new AleatoriedadCriptograficaQuiz()
);

Cuit cuitEmpresa = new Cuit(
    30714589624L
);

List<Cuil> cuilesVinculados = new List<Cuil>
{
    new Cuil(20304567899L),
    new Cuil(27329876549L),
    new Cuil(27690011102L),
    new Cuil(20251234567L)
};

Dictionary<EscenarioQuiz, int> escenarios =
    Enum.GetValues<EscenarioQuiz>()
        .ToDictionary(
            escenario => escenario,
            _ => 0
        );

int[] posicionesUnaCorrecta = new int[4];

int primeraOpcionEsVinculada = 0;
int primeraOpcionEsRespuestaCompleta = 0;
int totalUnaCorrecta = 0;

for (int numero = 0; numero < cantidadPruebas; numero++)
{
    Quiz quiz = generador.CrearQuiz(
        cuitEmpresa,
        cuilesVinculados
    );

    escenarios[quiz.Escenario]++;

    List<OpcionQuiz> opciones =
        quiz.Opciones.ToList();

    if (opciones[0].EsVinculado)
    {
        primeraOpcionEsVinculada++;
    }

    if (quiz.Escenario == EscenarioQuiz.UnaCorrecta)
    {
        totalUnaCorrecta++;

        int posicionCorrecta =
            opciones.FindIndex(
                opcion => opcion.EsVinculado
            );

        if (posicionCorrecta >= 0)
        {
            posicionesUnaCorrecta[posicionCorrecta]++;
        }

        if (posicionCorrecta == 0)
        {
            primeraOpcionEsRespuestaCompleta++;
        }
    }
}

Console.WriteLine();
Console.WriteLine("DIAGNÓSTICO DE ALEATORIEDAD DEL QUIZ");
Console.WriteLine("-----------------------------------");
Console.WriteLine(
    $"Cantidad de quizzes generados: {cantidadPruebas:N0}"
);

Console.WriteLine();
Console.WriteLine("DISTRIBUCIÓN DE ESCENARIOS");

foreach (EscenarioQuiz escenario in
         Enum.GetValues<EscenarioQuiz>())
{
    int cantidad = escenarios[escenario];

    Console.WriteLine(
        $"{escenario,-20}: " +
        $"{cantidad,7:N0} " +
        $"({ObtenerPorcentaje(cantidad, cantidadPruebas),6:F2}%)"
    );
}

Console.WriteLine();
Console.WriteLine(
    "POSICIÓN DE LA RESPUESTA EN EL ESCENARIO UNA CORRECTA"
);

for (int posicion = 0;
     posicion < posicionesUnaCorrecta.Length;
     posicion++)
{
    int cantidad = posicionesUnaCorrecta[posicion];

    Console.WriteLine(
        $"Posición {(char)('A' + posicion)}: " +
        $"{cantidad,7:N0} " +
        $"({ObtenerPorcentaje(cantidad, totalUnaCorrecta),6:F2}%)"
    );
}

Console.WriteLine();
Console.WriteLine("COMPROBACIONES ADICIONALES");

Console.WriteLine(
    "Primera opción vinculada: " +
    $"{ObtenerPorcentaje(
        primeraOpcionEsVinculada,
        cantidadPruebas
    ):F2}%"
);

Console.WriteLine(
    "Primera opción como respuesta completa correcta: " +
    $"{ObtenerPorcentaje(
        primeraOpcionEsRespuestaCompleta,
        cantidadPruebas
    ):F2}%"
);

Console.WriteLine();
Console.WriteLine("VALORES ESPERADOS");
Console.WriteLine(
    "- Cada posición en UnaCorrecta: aproximadamente 25%."
);
Console.WriteLine(
    "- Primera opción vinculada considerando todos los escenarios: aproximadamente 40%."
);
Console.WriteLine(
    "- Primera opción como respuesta completa correcta: aproximadamente 10%."
);

static double ObtenerPorcentaje(
    int cantidad,
    int total
)
{
    if (total == 0)
    {
        return 0;
    }

    return cantidad * 100.0 / total;
}