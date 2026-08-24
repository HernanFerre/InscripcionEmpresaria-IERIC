USE [SolicitudInscripcionEmpresariaDigital];
GO

SET NOCOUNT ON;

DECLARE @QuizSesionId bigint = NULL;

/*
    Por defecto se analiza la última sesión creada.

    Para consultar una sesión específica, asignar su identificador:

    SET @QuizSesionId = 1;
*/

IF @QuizSesionId IS NULL
BEGIN
    SELECT TOP (1)
        @QuizSesionId = sesion.Id
    FROM dbo.QuizSesion AS sesion
    ORDER BY
        sesion.FechaCreacion DESC,
        sesion.Id DESC;
END;

IF @QuizSesionId IS NULL
BEGIN
    RAISERROR(
        'No se encontraron sesiones de quiz en la base de datos.',
        16,
        1
    );

    RETURN;
END;

SELECT
    'QUIZ SELECCIONADO' AS Resultado,
    @QuizSesionId AS QuizSesionId;


/* ================================================================
   RESUMEN GENERAL
   ================================================================ */

SELECT
    'RESUMEN' AS Resultado,
    sesion.Id AS QuizSesionId,
    sesion.CuitEmpresa,
    sesion.UsuarioId,
    CASE sesion.Estado
        WHEN 1 THEN 'Activo'
        WHEN 2 THEN 'Validado'
        WHEN 3 THEN 'Bloqueado'
        WHEN 4 THEN 'Expirado'
        ELSE 'Estado desconocido'
    END AS Estado,
    sesion.IntentosTotales,
    (
        SELECT COUNT(*)
        FROM dbo.QuizRespuesta AS respuesta
        INNER JOIN dbo.QuizDesafio AS desafio
            ON desafio.Id = respuesta.QuizDesafioId
        WHERE desafio.QuizSesionId = sesion.Id
    ) AS IntentosRealizados,
    sesion.IntentosTotales -
    (
        SELECT COUNT(*)
        FROM dbo.QuizRespuesta AS respuesta
        INNER JOIN dbo.QuizDesafio AS desafio
            ON desafio.Id = respuesta.QuizDesafioId
        WHERE desafio.QuizSesionId = sesion.Id
    ) AS IntentosRestantes,
    (
        SELECT COUNT(*)
        FROM dbo.QuizCuilVinculado AS vinculado
        WHERE vinculado.QuizSesionId = sesion.Id
    ) AS CuilesVinculados,
    (
        SELECT COUNT(*)
        FROM dbo.QuizDesafio AS desafio
        WHERE desafio.QuizSesionId = sesion.Id
    ) AS Desafios,
    (
        SELECT COUNT(*)
        FROM dbo.QuizOpcion AS opcion
        INNER JOIN dbo.QuizDesafio AS desafio
            ON desafio.Id = opcion.QuizDesafioId
        WHERE desafio.QuizSesionId = sesion.Id
    ) AS OpcionesPresentadas,
    (
        SELECT COUNT(*)
        FROM dbo.QuizRespuesta AS respuesta
        INNER JOIN dbo.QuizDesafio AS desafio
            ON desafio.Id = respuesta.QuizDesafioId
        WHERE desafio.QuizSesionId = sesion.Id
    ) AS Respuestas,
    (
        SELECT COUNT(*)
        FROM dbo.QuizRespuestaOpcion AS seleccion
        INNER JOIN dbo.QuizDesafio AS desafio
            ON desafio.Id = seleccion.QuizDesafioId
        WHERE desafio.QuizSesionId = sesion.Id
    ) AS OpcionesSeleccionadas
FROM dbo.QuizSesion AS sesion
WHERE sesion.Id = @QuizSesionId;


/* ================================================================
   1. QUIZSESION
   Cabecera, usuario, CUIT, estado, fechas y bloqueo.
   ================================================================ */

SELECT
    'QuizSesion' AS Tabla,
    sesion.Id,
    sesion.CuitEmpresa,
    sesion.UsuarioId,
    sesion.Estado AS EstadoCodigo,
    CASE sesion.Estado
        WHEN 1 THEN 'Activo'
        WHEN 2 THEN 'Validado'
        WHEN 3 THEN 'Bloqueado'
        WHEN 4 THEN 'Expirado'
        ELSE 'Estado desconocido'
    END AS EstadoDescripcion,
    sesion.IntentosTotales,
    (
        SELECT COUNT(*)
        FROM dbo.QuizRespuesta AS respuesta
        INNER JOIN dbo.QuizDesafio AS desafio
            ON desafio.Id = respuesta.QuizDesafioId
        WHERE desafio.QuizSesionId = sesion.Id
    ) AS IntentosRealizados,
    sesion.IntentosTotales -
    (
        SELECT COUNT(*)
        FROM dbo.QuizRespuesta AS respuesta
        INNER JOIN dbo.QuizDesafio AS desafio
            ON desafio.Id = respuesta.QuizDesafioId
        WHERE desafio.QuizSesionId = sesion.Id
    ) AS IntentosRestantes,
    sesion.FechaCreacion,
    sesion.FechaExpiracion,
    sesion.FechaFinalizacion,
    sesion.BloqueadoHasta
FROM dbo.QuizSesion AS sesion
WHERE sesion.Id = @QuizSesionId;


/* ================================================================
   2. QUIZCUILVINCULADO
   CUILes reales recibidos del servicio externo.
   ================================================================ */

SELECT
    'QuizCuilVinculado' AS Tabla,
    vinculado.QuizSesionId,
    vinculado.Cuil
FROM dbo.QuizCuilVinculado AS vinculado
WHERE vinculado.QuizSesionId = @QuizSesionId
ORDER BY vinculado.Cuil;


/* ================================================================
   3. QUIZDESAFIO
   Un desafío por cada intento presentado.
   ================================================================ */

SELECT
    'QuizDesafio' AS Tabla,
    desafio.Id AS QuizDesafioId,
    desafio.QuizSesionId,
    desafio.Numero AS NumeroIntento,
    desafio.Escenario AS EscenarioCodigo,
    CASE desafio.Escenario
        WHEN 1 THEN 'Una correcta'
        WHEN 2 THEN 'Dos correctas'
        WHEN 3 THEN 'Todas correctas'
        WHEN 4 THEN 'Ninguna correcta'
        ELSE 'Escenario desconocido'
    END AS EscenarioDescripcion,
    desafio.EsActual,
    desafio.FechaCreacion
FROM dbo.QuizDesafio AS desafio
WHERE desafio.QuizSesionId = @QuizSesionId
ORDER BY desafio.Numero;


/* ================================================================
   4. QUIZOPCION
   Opciones A-D, Ninguna y Todas de cada desafío.
   ================================================================ */

SELECT
    'QuizOpcion' AS Tabla,
    desafio.Numero AS NumeroIntento,
    opcion.QuizDesafioId,
    opcion.CodigoOpcion,
    CASE opcion.CodigoOpcion
        WHEN 0 THEN 'A'
        WHEN 1 THEN 'B'
        WHEN 2 THEN 'C'
        WHEN 3 THEN 'D'
        WHEN 4 THEN 'Ninguna'
        WHEN 5 THEN 'Todas'
        ELSE 'Código desconocido'
    END AS Opcion,
    opcion.Cuil,
    opcion.EsVinculado,
    CASE
        WHEN
            desafio.Escenario IN (1, 2)
            AND opcion.CodigoOpcion BETWEEN 0 AND 3
            AND opcion.EsVinculado = 1
            THEN 'Sí'
        WHEN
            desafio.Escenario = 3
            AND opcion.CodigoOpcion = 5
            THEN 'Sí'
        WHEN
            desafio.Escenario = 4
            AND opcion.CodigoOpcion = 4
            THEN 'Sí'
        ELSE 'No'
    END AS EsRespuestaEsperada
FROM dbo.QuizOpcion AS opcion
INNER JOIN dbo.QuizDesafio AS desafio
    ON desafio.Id = opcion.QuizDesafioId
WHERE desafio.QuizSesionId = @QuizSesionId
ORDER BY
    desafio.Numero,
    opcion.CodigoOpcion;


/* ================================================================
   5. QUIZRESPUESTA
   Cabecera y resultado general de cada respuesta enviada.
   ================================================================ */

SELECT
    'QuizRespuesta' AS Tabla,
    desafio.Numero AS NumeroIntento,
    respuesta.Id AS QuizRespuestaId,
    respuesta.QuizDesafioId,
    respuesta.EsCorrecta,
    CASE
        WHEN respuesta.EsCorrecta = 1 THEN 'Correcta'
        ELSE 'Incorrecta'
    END AS ResultadoRespuesta,
    (
        SELECT COUNT(*)
        FROM dbo.QuizRespuestaOpcion AS seleccion
        WHERE seleccion.QuizRespuestaId = respuesta.Id
    ) AS CantidadOpcionesSeleccionadas,
    respuesta.FechaRespuesta
FROM dbo.QuizRespuesta AS respuesta
INNER JOIN dbo.QuizDesafio AS desafio
    ON desafio.Id = respuesta.QuizDesafioId
WHERE desafio.QuizSesionId = @QuizSesionId
ORDER BY
    desafio.Numero,
    respuesta.FechaRespuesta;


/* ================================================================
   6. QUIZRESPUESTAOPCION
   Una fila por cada opción seleccionada por el usuario.
   ================================================================ */

SELECT
    'QuizRespuestaOpcion' AS Tabla,
    desafio.Numero AS NumeroIntento,
    seleccion.QuizRespuestaId,
    seleccion.QuizDesafioId,
    seleccion.CodigoOpcion,
    CASE seleccion.CodigoOpcion
        WHEN 0 THEN 'A'
        WHEN 1 THEN 'B'
        WHEN 2 THEN 'C'
        WHEN 3 THEN 'D'
        WHEN 4 THEN 'Ninguna'
        WHEN 5 THEN 'Todas'
        ELSE 'Código desconocido'
    END AS OpcionSeleccionada,
    opcion.Cuil,
    CASE
        WHEN
            desafio.Escenario IN (1, 2)
            AND seleccion.CodigoOpcion BETWEEN 0 AND 3
            AND opcion.EsVinculado = 1
            THEN 'Sí'
        WHEN
            desafio.Escenario = 3
            AND seleccion.CodigoOpcion = 5
            THEN 'Sí'
        WHEN
            desafio.Escenario = 4
            AND seleccion.CodigoOpcion = 4
            THEN 'Sí'
        ELSE 'No'
    END AS EraRespuestaEsperada,
    respuesta.EsCorrecta AS ResultadoGeneral,
    CASE
        WHEN respuesta.EsCorrecta = 1 THEN 'Correcta'
        ELSE 'Incorrecta'
    END AS ResultadoDescripcion,
    respuesta.FechaRespuesta
FROM dbo.QuizRespuestaOpcion AS seleccion
INNER JOIN dbo.QuizRespuesta AS respuesta
    ON respuesta.Id = seleccion.QuizRespuestaId
    AND respuesta.QuizDesafioId = seleccion.QuizDesafioId
INNER JOIN dbo.QuizDesafio AS desafio
    ON desafio.Id = seleccion.QuizDesafioId
INNER JOIN dbo.QuizOpcion AS opcion
    ON opcion.QuizDesafioId = seleccion.QuizDesafioId
    AND opcion.CodigoOpcion = seleccion.CodigoOpcion
WHERE desafio.QuizSesionId = @QuizSesionId
ORDER BY
    desafio.Numero,
    seleccion.CodigoOpcion;
