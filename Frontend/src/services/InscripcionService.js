import { ESCENARIO_INFORMACION_DUMMY, empresasDummy, opcionesInformacionDummy } from "../mocks/inscripcionMocks";
import { delay } from "../utils/delay";

export async function validarCuit(cuit) {
  await delay(700);

  const soloNumeros = cuit.replace(/\D/g, "");

  if (soloNumeros.startsWith("300000")) {
    return {
      ok: true,
      estadoSolicitud: empresasDummy.REGISTRADA.estadoSolicitud,
      empresa: {
        cuit,
        razonSocial: empresasDummy.REGISTRADA.razonSocial,
        estado: "CUIT válido",
      },
    };
  }

  if (soloNumeros.startsWith("301111")) {
    return {
      ok: true,
      estadoSolicitud: empresasDummy.HABILITADA.estadoSolicitud,
      empresa: {
        cuit,
        razonSocial: empresasDummy.HABILITADA.razonSocial,
        estado: "CUIT válido",
      },
    };
  }

  if (soloNumeros.startsWith("302222")) {
    return {
      ok: true,
      estadoSolicitud: empresasDummy.BLOQUEADA.estadoSolicitud,
      empresa: {
        cuit,
        razonSocial: empresasDummy.BLOQUEADA.razonSocial,
        estado: "CUIT válido",
      },
    };
  }

  return {
    ok: false,
    estadoSolicitud: "NO_ENCONTRADA",
    empresa: null,
    mensaje: "No se encontró información para el CUIT ingresado.",
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

export async function obtenerCuilesPorCuit(cuit) {
  await delay(700);

  const cuitNormalizado = String(cuit ?? "").replace(/\D/g, "");

  if (cuitNormalizado.length !== 11) {
    return {
      ok: false,
      cuit: cuitNormalizado,
      cuiles: [],
      mensaje: "El CUIT debe contener 11 números.",
    };
  }

  return {
    ok: true,
    cuit: cuitNormalizado,
    cuiles: [
      "20000000001",
      "27000000006",
      "23000000000",
      "24000000007",
      "20000000019",
      "27000000014",
      "23000000019",
      "24000000015",
      "20000000028",
      "27000000022",
    ],
  };
}
