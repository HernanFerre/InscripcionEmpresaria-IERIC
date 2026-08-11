import { QUIZ_BLOCKING_MOCK } from "../config/featureFlags.js";

const STORAGE_KEY = "ieric_quiz_blocking_mock";

function normalizarCuit(cuit) {
  return String(cuit ?? "").replace(/\D/g, "");
}

function leerBloqueos() {
  try {
    const contenido = localStorage.getItem(STORAGE_KEY);

    if (!contenido) {
      return {};
    }

    const bloqueos = JSON.parse(contenido);

    return bloqueos && typeof bloqueos === "object" ? bloqueos : {};
  } catch {
    return {};
  }
}

function guardarBloqueos(bloqueos) {
  try {
    if (Object.keys(bloqueos).length === 0) {
      localStorage.removeItem(STORAGE_KEY);
      return;
    }

    localStorage.setItem(STORAGE_KEY, JSON.stringify(bloqueos));
  } catch {
    // El bloqueo es solamente una simulación para el navegador.
  }
}

function obtenerDuracionMilisegundos() {
  const { duration, unit } = QUIZ_BLOCKING_MOCK;

  if (duration === null) {
    return null;
  }

  const duracionNumerica = Number(duration);

  if (!Number.isFinite(duracionNumerica) || duracionNumerica < 0) {
    return 0;
  }

  const unidades = {
    seconds: 1000,
    minutes: 60 * 1000,
    hours: 60 * 60 * 1000,
  };

  const multiplicador = unidades[String(unit).toLowerCase()] ?? 1000;

  return duracionNumerica * multiplicador;
}

export function bloquearCuit(cuit) {
  if (!QUIZ_BLOCKING_MOCK.enabled) {
    return obtenerEstadoBloqueo(cuit);
  }

  const cuitNormalizado = normalizarCuit(cuit);

  if (cuitNormalizado.length !== 11) {
    return obtenerEstadoBloqueo(cuit);
  }

  const bloqueos = leerBloqueos();

  bloqueos[cuitNormalizado] = {
    fechaBloqueoUtc: new Date().toISOString(),
  };

  guardarBloqueos(bloqueos);

  return obtenerEstadoBloqueo(cuitNormalizado);
}

export function obtenerEstadoBloqueo(cuit) {
  const estadoDisponible = {
    bloqueado: false,
    permanente: false,
    bloqueadoHasta: null,
    segundosRestantes: 0,
  };

  if (!QUIZ_BLOCKING_MOCK.enabled) {
    return estadoDisponible;
  }

  const cuitNormalizado = normalizarCuit(cuit);
  const bloqueos = leerBloqueos();
  const bloqueo = bloqueos[cuitNormalizado];

  if (!bloqueo?.fechaBloqueoUtc) {
    return estadoDisponible;
  }

  const fechaBloqueo = new Date(bloqueo.fechaBloqueoUtc);

  if (Number.isNaN(fechaBloqueo.getTime())) {
    delete bloqueos[cuitNormalizado];
    guardarBloqueos(bloqueos);

    return estadoDisponible;
  }

  const duracionMilisegundos = obtenerDuracionMilisegundos();

  if (duracionMilisegundos === null) {
    return {
      bloqueado: true,
      permanente: true,
      bloqueadoHasta: null,
      segundosRestantes: null,
    };
  }

  const fechaBloqueoHasta = fechaBloqueo.getTime() + duracionMilisegundos;

  const tiempoRestante = fechaBloqueoHasta - Date.now();

  if (tiempoRestante <= 0) {
    delete bloqueos[cuitNormalizado];
    guardarBloqueos(bloqueos);

    return estadoDisponible;
  }

  return {
    bloqueado: true,
    permanente: false,
    bloqueadoHasta: new Date(fechaBloqueoHasta).toISOString(),
    segundosRestantes: Math.ceil(tiempoRestante / 1000),
  };
}

export function limpiarBloqueoCuit(cuit) {
  const cuitNormalizado = normalizarCuit(cuit);
  const bloqueos = leerBloqueos();

  delete bloqueos[cuitNormalizado];

  guardarBloqueos(bloqueos);
}
