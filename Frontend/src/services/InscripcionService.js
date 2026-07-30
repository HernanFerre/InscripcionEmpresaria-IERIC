import { ESCENARIO_INFORMACION_DUMMY, opcionesInformacionDummy } from "../mocks/inscripcionMocks";

import { delay } from "../utils/delay";

const INSCRIPCION_API_URL = (import.meta.env.VITE_INSCRIPCION_API_URL || "").replace(/\/+$/, "");

const EMPRESAS_ESTADO_SERVIDOR = (import.meta.env.VITE_EMPRESAS_ESTADO_SERVIDOR || "").replace(/\/+$/, "");

const EMPRESAS_ESTADO_PUERTO = import.meta.env.VITE_EMPRESAS_ESTADO_PUERTO || "";

const ESTADOS_HABILITADOS = new Set([5, 8, 9]);

async function procesarRespuestaApi(response) {
  let data;

  try {
    data = await response.json();
  } catch {
    data = null;
  }

  if (!response.ok) {
    throw new Error(data?.mensaje || data?.message || data?.Message || `La solicitud devolvió el estado ${response.status}.`);
  }

  return data;
}

function obtenerEstadoSolicitud(codigoEstado) {
  if (ESTADOS_HABILITADOS.has(codigoEstado)) {
    return "HABILITADA";
  }

  if (codigoEstado === 0 || codigoEstado === 1) {
    return "REGISTRADA";
  }

  return "BLOQUEADA";
}

export async function validarCuit(cuit) {
  const cuitNormalizado = String(cuit ?? "").replace(/\D/g, "");

  if (cuitNormalizado.length !== 11) {
    return {
      ok: false,
      estadoSolicitud: "NO_ENCONTRADA",
      empresa: null,
      mensaje: "El CUIT debe contener 11 números.",
    };
  }

  if (!EMPRESAS_ESTADO_SERVIDOR) {
    throw new Error("No se configuró VITE_EMPRESAS_ESTADO_SERVIDOR.");
  }

  if (!EMPRESAS_ESTADO_PUERTO) {
    throw new Error("No se configuró VITE_EMPRESAS_ESTADO_PUERTO.");
  }

  const estadoApiUrl = `${EMPRESAS_ESTADO_SERVIDOR}:` + `${EMPRESAS_ESTADO_PUERTO}`;

  const response = await fetch(`${estadoApiUrl}/empresas/estado/${cuitNormalizado}`, {
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  });

  const data = await procesarRespuestaApi(response);

  const codigoEstado = Number(data.codigoEstado ?? data.codigoestado);

  if (!Number.isInteger(codigoEstado)) {
    throw new Error("El servicio devolvió un código de estado inválido.");
  }

  const razonSocial = typeof data.razon === "string" ? data.razon.trim() : "";

  const mensajeEstado = typeof data.mensaje === "string" ? data.mensaje.replace(/^\s*\d+\s*-\s*/, "").trim() : "";

  return {
    ok: true,

    estadoSolicitud: obtenerEstadoSolicitud(codigoEstado),

    empresa: {
      cuit: String(data.cuit ?? cuitNormalizado),

      razonSocial,

      estado: "CUIT válido",

      codigoEstado,

      mensaje: mensajeEstado,
    },

    mensaje: mensajeEstado,
  };
}

export async function enviarCodigoTelefono(telefono) {
  await delay(600);

  return {
    ok: true,
    telefono,
    mensaje: "Código enviado correctamente.",
  };
}

export async function validarCodigoTelefono(codigo) {
  await delay(500);

  return {
    ok: codigo === "123456",
  };
}

export async function obtenerDesafioInformacion() {
  await delay(700);

  if (ESCENARIO_INFORMACION_DUMMY === "UNICO_EMPLEADO") {
    return {
      escenario: "UNICO_EMPLEADO",

      titulo: "INFORMACIÓN DE LA EMPRESA",

      consigna: "Seleccione el CUIL que reconoce como vinculado a la empresa.",

      intentosTotales: 3,

      intentosRestantes: 3,

      opciones: opcionesInformacionDummy,

      respuestasCorrectas: ["a"],
    };
  }

  return {
    escenario: "MULTIPLES_EMPLEADOS",

    titulo: "INFORMACIÓN DE LA EMPRESA",

    consigna: "Seleccione los CUIL que reconoce como vinculados a la empresa.",

    intentosTotales: 3,

    intentosRestantes: 3,

    opciones: opcionesInformacionDummy,

    respuestasCorrectas: ["a", "c"],
  };
}

export async function validarDesafioInformacion(seleccionadas, respuestasCorrectas) {
  await delay(500);

  const seleccionOrdenada = [...seleccionadas].sort().join(",");

  const correctaOrdenada = [...respuestasCorrectas].sort().join(",");

  return seleccionOrdenada === correctaOrdenada;
}

/*
 * Esta función se mantiene temporalmente para no romper
 * otras referencias mientras terminamos la integración.
 */
export async function obtenerCuilesPorCuit(cuit) {
  const cuitNormalizado = String(cuit ?? "").replace(/\D/g, "");

  return {
    ok: cuitNormalizado.length === 11,

    cuit: cuitNormalizado,

    cuiles: [],

    mensaje: cuitNormalizado.length === 11 ? "" : "El CUIT debe contener 11 números.",
  };
}

export async function crearQuiz(cuit) {
  if (!INSCRIPCION_API_URL) {
    throw new Error("No se configuró VITE_INSCRIPCION_API_URL.");
  }

  const cuitNormalizado = String(cuit ?? "").replace(/\D/g, "");

  if (cuitNormalizado.length !== 11) {
    throw new Error("El CUIT debe contener 11 números.");
  }

  const response = await fetch(`${INSCRIPCION_API_URL}/v1/Quiz/Crear`, {
    method: "POST",

    headers: {
      "Content-Type": "application/json",

      Accept: "application/json",
    },

    body: JSON.stringify({
      cuit: cuitNormalizado,
    }),
  });

  return procesarRespuestaApi(response);
}

export async function validarQuiz(quizId, opcionesSeleccionadas) {
  if (!INSCRIPCION_API_URL) {
    throw new Error("No se configuró VITE_INSCRIPCION_API_URL.");
  }

  const response = await fetch(`${INSCRIPCION_API_URL}/v1/Quiz/Validar`, {
    method: "POST",

    headers: {
      "Content-Type": "application/json",

      Accept: "application/json",
    },

    body: JSON.stringify({
      quizId,
      opcionesSeleccionadas,
    }),
  });

  return procesarRespuestaApi(response);
}
