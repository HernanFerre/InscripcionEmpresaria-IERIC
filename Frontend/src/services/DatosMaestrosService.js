const INSCRIPCION_API_URL = (import.meta.env.VITE_INSCRIPCION_API_URL || "").replace(/\/+$/, "");

function crearHeadersAutorizados(token) {
  const headers = {
    Accept: "application/json",
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
}

async function consultarCatalogo(endpoint, nombreCatalogo, token) {
  if (!INSCRIPCION_API_URL) {
    throw new Error("No se configuró VITE_INSCRIPCION_API_URL.");
  }

  const response = await fetch(`${INSCRIPCION_API_URL}${endpoint}`, {
    method: "GET",
    headers: crearHeadersAutorizados(token),
  });

  let data;

  try {
    data = await response.json();
  } catch {
    data = null;
  }

  if (!response.ok) {
    throw new Error(
      data?.mensaje ||
        data?.message ||
        data?.Message ||
        `No fue posible obtener ${nombreCatalogo}. ` + `El servicio devolvió el estado ${response.status}.`,
    );
  }

  if (!Array.isArray(data)) {
    throw new Error(`El servicio de ${nombreCatalogo} devolvió un formato inválido.`);
  }

  return data;
}

export async function obtenerActividadesConstruccion(token) {
  const actividades = await consultarCatalogo("/v1/datos-maestros/actividades-construccion", "las actividades de construcción", token);

  return actividades.map((actividad) => ({
    value: String(actividad.idActividad),

    label: [actividad.codigoActividadAFIP, actividad.descripcion]
      .filter((value) => value !== null && value !== undefined && String(value).trim())
      .join(" - "),

    idActividad: actividad.idActividad,

    codigoActividadAFIP: actividad.codigoActividadAFIP,

    descripcion: actividad.descripcion,
  }));
}

export async function obtenerCaracteresEmpresa(token) {
  const caracteres = await consultarCatalogo("/v1/datos-maestros/caracteres-empresa", "los caracteres de empresa", token);

  return caracteres.map((caracter) => ({
    value: String(caracter.idCaracter),

    label: String(caracter.descripcion ?? "").trim(),

    idCaracter: caracter.idCaracter,

    descripcion: caracter.descripcion,
  }));
}

export async function obtenerTiposSociedad(token) {
  const tiposSociedad = await consultarCatalogo("/v1/datos-maestros/tipos-sociedad", "los tipos de sociedad", token);

  return tiposSociedad.map((tipoSociedad) => ({
    value: String(tipoSociedad.idTipoSociedad),

    label: String(tipoSociedad.descripcion ?? "").trim(),

    idTipoSociedad: tipoSociedad.idTipoSociedad,

    descripcion: tipoSociedad.descripcion,
  }));
}

export async function obtenerCatalogosEmpresa(token) {
  const [actividadesEmpresa, caracteresEmpresa, tiposSociedad] = await Promise.all([
    obtenerActividadesConstruccion(token),

    obtenerCaracteresEmpresa(token),

    obtenerTiposSociedad(token),
  ]);

  return {
    actividadesEmpresa,
    caracteresEmpresa,
    tiposSociedad,
  };
}
