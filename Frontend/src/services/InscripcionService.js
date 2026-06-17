const delay = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

export async function validarCuit(cuit) {
  await delay(700);

  const soloNumeros = cuit.replace(/\D/g, "");

  if (soloNumeros.startsWith("300000")) {
    return {
      ok: true,
      estadoSolicitud: "REGISTRADA",
      empresa: {
        cuit,
        razonSocial: "CONSTRUCTORA REGISTRADA S.A.",
        estado: "CUIT válido",
      },
    };
  }

  if (soloNumeros.startsWith("301111")) {
    return {
      ok: true,
      estadoSolicitud: "HABILITADA",
      empresa: {
        cuit,
        razonSocial: "EMPRESA HABILITADA S.R.L.",
        estado: "CUIT válido",
      },
    };
  }

  if (soloNumeros.startsWith("302222")) {
    return {
      ok: true,
      estadoSolicitud: "BLOQUEADA",
      empresa: {
        cuit,
        razonSocial: "EMPRESA BLOQUEADA S.A.",
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

const ESCENARIO_INFORMACION_DUMMY = "UNICO_EMPLEADO";
// Opciones posibles:
// "MULTIPLES_EMPLEADOS"
// "UNICO_EMPLEADO"
// "VALIDACION_MANUAL"

export async function obtenerDesafioInformacion() {
  await delay(700);

  if (ESCENARIO_INFORMACION_DUMMY === "UNICO_EMPLEADO") {
    return {
      escenario: "UNICO_EMPLEADO",
      titulo: "INFORMACIÓN DE LA EMPRESA",
      consigna: "Seleccione el CUIL que reconoce como vinculado a la empresa.",
      intentosTotales: 3,
      intentosRestantes: 3,
      opciones: [
        { id: "a", label: "20-xxxxx458-3" },
        { id: "b", label: "27-xxxxx921-5" },
        { id: "c", label: "23-xxxxx774-1" },
        { id: "d", label: "24-xxxxx662-8" },
        { id: "ninguna", label: "Ninguna de las anteriores" },
        { id: "todas", label: "Todas las anteriores" },
      ],
      respuestasCorrectas: ["a"],
    };
  }

  return {
    escenario: "MULTIPLES_EMPLEADOS",
    titulo: "INFORMACIÓN DE LA EMPRESA",
    consigna: "Seleccione los CUIL que reconoce como vinculados a la empresa.",
    intentosTotales: 3,
    intentosRestantes: 3,
    opciones: [
      { id: "a", label: "20-xxxxx458-3" },
      { id: "b", label: "27-xxxxx921-5" },
      { id: "c", label: "23-xxxxx774-1" },
      { id: "d", label: "24-xxxxx662-8" },
      { id: "ninguna", label: "Ninguna de las anteriores" },
      { id: "todas", label: "Todas las anteriores" },
    ],
    respuestasCorrectas: ["a", "c"],
  };
}

export async function validarDesafioInformacion(seleccionadas, respuestasCorrectas) {
  await delay(500);

  const seleccionOrdenada = [...seleccionadas].sort().join(",");
  const correctaOrdenada = [...respuestasCorrectas].sort().join(",");

  return seleccionOrdenada === correctaOrdenada;
}
