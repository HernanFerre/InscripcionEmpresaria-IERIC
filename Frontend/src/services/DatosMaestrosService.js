const DATOS_MAESTROS_API_URL = (import.meta.env.VITE_DATOS_MAESTROS_API_URL || "https://apidesa.ieric.org.ar:50100").replace(/\/+$/, "");

async function consultarCatalogo(endpoint, nombreCatalogo) {
  const response = await fetch(`${DATOS_MAESTROS_API_URL}${endpoint}`, {
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  });

  if (!response.ok) {
    throw new Error(`No fue posible obtener ${nombreCatalogo}. El servicio devolvió el estado ${response.status}.`);
  }

  const data = await response.json();

  if (!Array.isArray(data)) {
    throw new Error(`El servicio de ${nombreCatalogo} devolvió un formato inválido.`);
  }

  return data;
}

export async function obtenerActividadesConstruccion() {
  const actividades = await consultarCatalogo("/datos-maestros/actividades-construccion", "las actividades de construcción");

  return actividades.map((actividad) => ({
    value: String(actividad.idActividad),
    label: [actividad.idActividad, actividad.codigoActividadAFIP, actividad.descripcion]
      .filter((value) => value !== null && value !== undefined && String(value).trim())
      .join(" - "),
    idActividad: actividad.idActividad,
    codigoActividadAFIP: actividad.codigoActividadAFIP,
    descripcion: actividad.descripcion,
  }));
}

export async function obtenerCaracteresEmpresa() {
  const caracteres = await consultarCatalogo("/datos-maestros/caracteres-empresa", "los caracteres de empresa");

  return caracteres.map((caracter) => ({
    value: String(caracter.idCaracter),
    label: [caracter.idCaracter, caracter.descripcion]
      .filter((value) => value !== null && value !== undefined && String(value).trim())
      .join(" - "),
    idCaracter: caracter.idCaracter,
    descripcion: caracter.descripcion,
  }));
}

export async function obtenerTiposSociedad() {
  const tiposSociedad = await consultarCatalogo("/datos-maestros/tipos-sociedad", "los tipos de sociedad");

  return tiposSociedad.map((tipoSociedad) => ({
    value: String(tipoSociedad.idTipoSociedad),
    label: [tipoSociedad.idTipoSociedad, tipoSociedad.descripcion]
      .filter((value) => value !== null && value !== undefined && String(value).trim())
      .join(" - "),
    idTipoSociedad: tipoSociedad.idTipoSociedad,
    descripcion: tipoSociedad.descripcion,
  }));
}

export async function obtenerCatalogosEmpresa() {
  const [actividadesEmpresa, caracteresEmpresa, tiposSociedad] = await Promise.all([
    obtenerActividadesConstruccion(),
    obtenerCaracteresEmpresa(),
    obtenerTiposSociedad(),
  ]);

  return {
    actividadesEmpresa,
    caracteresEmpresa,
    tiposSociedad,
  };
}
