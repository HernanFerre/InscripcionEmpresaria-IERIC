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
