import { useEffect, useState } from "react";

import { ArrowDown, Info, Pencil, Plus, Trash2 } from "lucide-react";

import TrabajadorModal from "../../components/modals/TrabajadorModal.jsx";
import { cargarNominaSolicitud, guardarNominaSolicitud } from "../../services/InscripcionService.js";
import { formatCuit, normalizarCuit } from "../../utils/formatters.js";

import "../../styles/stepNomina.css";

const TAMANO_PAGINA = 10;

function formatearFecha(fecha) {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(fecha ?? "")) return "—";
  const [anio, mes, dia] = fecha.split("-");
  return `${dia}/${mes}/${anio}`;
}

function formatearCuil(value) {
  const cuil = normalizarCuit(value);
  return cuil.length > 11 ? cuil : formatCuit(cuil).replace(/-/g, " ");
}

export default function StepNomina({ token, solicitudId = null, nomina = null, onChange, onBack, onNext }) {
  const id = Number(solicitudId);
  const solicitudValida = Number.isSafeInteger(id) && id > 0;
  const tieneNomina = solicitudValida && Number(nomina?.solicitudId) === id && Array.isArray(nomina?.trabajadores);
  const trabajadores = tieneNomina ? nomina.trabajadores : [];

  const [cargando, setCargando] = useState(!tieneNomina && Boolean(token) && solicitudValida);
  const [guardando, setGuardando] = useState(false);
  const [errorProceso, setErrorProceso] = useState("");
  const [intentoCarga, setIntentoCarga] = useState(0);
  const [pagina, setPagina] = useState(0);
  const [modalTrabajadorAbierto, setModalTrabajadorAbierto] = useState(false);
  const [trabajadorSeleccionado, setTrabajadorSeleccionado] = useState(null);

  useEffect(() => {
    if (!solicitudValida || !token || tieneNomina) return;

    const controller = new AbortController();
    let activo = true;

    const cargar = async () => {
      setCargando(true);
      setErrorProceso("");

      try {
        const resultado = await cargarNominaSolicitud(id, token, controller.signal);
        if (activo) onChange?.(resultado);
      } catch (error) {
        if (activo && !controller.signal.aborted) {
          setErrorProceso(error.message || "No fue posible cargar la nómina.");
        }
      } finally {
        if (activo) setCargando(false);
      }
    };

    cargar();

    return () => {
      activo = false;
      controller.abort();
    };
  }, [id, solicitudValida, token, tieneNomina, onChange, intentoCarga]);

  const puedeOperar = tieneNomina && Boolean(token) && !cargando && !guardando;
  const totalPaginas = Math.max(1, Math.ceil(trabajadores.length / TAMANO_PAGINA));
  const paginaActual = Math.min(pagina, totalPaginas - 1);
  const inicioPagina = paginaActual * TAMANO_PAGINA;
  const trabajadoresVisibles = trabajadores.slice(inicioPagina, inicioPagina + TAMANO_PAGINA);

  const trabajadorMasAntiguo = trabajadores.reduce((anterior, trabajador) => {
    if (!trabajador.ingreso) return anterior;
    return !anterior || trabajador.ingreso < anterior.ingreso ? trabajador : anterior;
  }, null);

  const mensajeError =
    errorProceso ||
    (!solicitudValida
      ? "Debe guardar primero los datos de la empresa para cargar su nómina."
      : !token
        ? "Debe iniciar sesión para consultar la nómina."
        : "");

  const actualizarTrabajadores = (nuevos) => {
    setErrorProceso("");
    onChange?.({ ...nomina, trabajadores: nuevos });
  };

  const actualizarPertenencia = (trabajador, value) => {
    if (!puedeOperar) return;
    actualizarTrabajadores(trabajadores.map((item) => (item.id === trabajador.id ? { ...item, perteneceConstruccion: value } : item)));
  };

  const abrirNuevoTrabajador = () => {
    if (!puedeOperar) return;
    setErrorProceso("");
    setTrabajadorSeleccionado(null);
    setModalTrabajadorAbierto(true);
  };

  const abrirEdicionTrabajador = (trabajador) => {
    if (!puedeOperar || trabajador.origen !== "manual") return;
    setErrorProceso("");
    setTrabajadorSeleccionado(trabajador);
    setModalTrabajadorAbierto(true);
  };

  const cerrarModalTrabajador = () => {
    setModalTrabajadorAbierto(false);
    setTrabajadorSeleccionado(null);
  };

  const guardarTrabajador = (datosTrabajador) => {
    if (!puedeOperar || trabajadorSeleccionado?.origen === "servicio") return;

    const guardado = {
      ...datosTrabajador,
      id: trabajadorSeleccionado?.id ?? `manual-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
      trabajadorId: trabajadorSeleccionado?.trabajadorId ?? 0,
      apellidoNombre: String(datosTrabajador.apellidoNombre ?? "").trim(),
      cuil: normalizarCuit(datosTrabajador.cuil),
      egreso: datosTrabajador.egreso || "",
      perteneceConstruccion: trabajadorSeleccionado?.perteneceConstruccion ?? datosTrabajador.perteneceConstruccion ?? true,
      origen: "manual",
    };

    actualizarTrabajadores(
      trabajadorSeleccionado ? trabajadores.map((item) => (item.id === guardado.id ? guardado : item)) : [guardado, ...trabajadores],
    );

    if (!trabajadorSeleccionado) setPagina(0);
    cerrarModalTrabajador();
  };

  const eliminarTrabajador = (trabajador) => {
    if (!puedeOperar || trabajador.origen !== "manual") return;
    actualizarTrabajadores(trabajadores.filter((item) => item.id !== trabajador.id));
  };

  const continuar = async () => {
    if (!puedeOperar || modalTrabajadorAbierto) return;
    setGuardando(true);
    setErrorProceso("");

    try {
      const resultado = await guardarNominaSolicitud(id, trabajadores, token);
      onChange?.(resultado);
      onNext?.(resultado);
    } catch (error) {
      setErrorProceso(error.message || "No fue posible guardar la nómina.");
    } finally {
      setGuardando(false);
    }
  };

  return (
    <section className="nomina-step">
      <div className="nomina-header">
        <div className="nomina-start-info">
          <div>
            <h2 className="section-title nomina-title">Inicio de actividades</h2>

            <p className="nomina-subtitle">Según la nómina de trabajadores.</p>
          </div>

          <span className="nomina-start-date">{formatearFecha(trabajadorMasAntiguo?.ingreso)}</span>

          <div className="nomina-info-wrapper">
            <button type="button" className="nomina-info-button" aria-label="Información sobre el inicio de actividades">
              <Info size={18} />
            </button>

            <div className="nomina-info-tooltip" role="tooltip">
              La fecha de ingreso del trabajador más antiguo será considerada como Inicio de Actividades y reviste carácter de Declaración
              Jurada.
              <br />
              {trabajadorMasAntiguo
                ? `Información proveniente del trabajador: ${trabajadorMasAntiguo.apellidoNombre} (${formatearCuil(trabajadorMasAntiguo.cuil)}).`
                : "Agregue trabajadores para determinar la fecha de inicio de actividades."}
            </div>
          </div>
        </div>

        <button type="button" className="nomina-add-button" disabled={!puedeOperar} onClick={abrirNuevoTrabajador}>
          <Plus size={18} aria-hidden="true" />
          Agregar trabajador
        </button>
      </div>

      <div className="nomina-table-wrapper">
        <table className="nomina-table">
          <thead>
            <tr>
              <th>Apellido y nombre</th>
              <th>CUIL</th>

              <th>
                <span className="nomina-sortable-header">
                  Ingreso
                  <ArrowDown size={15} aria-hidden="true" />
                </span>
              </th>

              <th>Egreso</th>

              <th>Pertenece a construcción</th>
              <th aria-label="Acciones" />
            </tr>
          </thead>

          <tbody>
            {trabajadoresVisibles.length === 0 && (
              <tr>
                <td colSpan={6} role="status">
                  {cargando
                    ? "Cargando nómina..."
                    : tieneNomina
                      ? "No hay trabajadores informados. Puede agregarlos manualmente."
                      : "La nómina todavía no se ha cargado."}
                </td>
              </tr>
            )}

            {trabajadoresVisibles.map((trabajador) => {
              const pertenece = trabajador.perteneceConstruccion;

              const esManual = trabajador.origen === "manual";

              return (
                <tr key={trabajador.id}>
                  <td title={trabajador.apellidoNombre}>{trabajador.apellidoNombre}</td>
                  <td title={formatearCuil(trabajador.cuil)}>{formatearCuil(trabajador.cuil)}</td>
                  <td>{formatearFecha(trabajador.ingreso)}</td>

                  <td className={trabajador.egreso ? "nomina-egreso" : ""}>{formatearFecha(trabajador.egreso)}</td>

                  <td>
                    <div
                      className="nomina-construccion-options"
                      role="radiogroup"
                      aria-label={`Pertenece a construcción: ${trabajador.apellidoNombre}`}
                    >
                      <label>
                        <span>Sí</span>

                        <input
                          type="radio"
                          name={`construccion-${trabajador.id}`}
                          checked={pertenece === true}
                          disabled={!puedeOperar}
                          onChange={() => actualizarPertenencia(trabajador, true)}
                        />
                      </label>

                      <label>
                        <span>No</span>

                        <input
                          type="radio"
                          name={`construccion-${trabajador.id}`}
                          checked={pertenece === false}
                          disabled={!puedeOperar}
                          onChange={() => actualizarPertenencia(trabajador, false)}
                        />
                      </label>
                    </div>
                  </td>

                  <td>
                    {esManual && (
                      <div className="nomina-row-actions">
                        <button
                          type="button"
                          className="nomina-edit-button"
                          disabled={!puedeOperar}
                          aria-label={`Editar a ${trabajador.apellidoNombre}`}
                          onClick={() => abrirEdicionTrabajador(trabajador)}
                        >
                          <Pencil size={18} />
                        </button>

                        <button
                          type="button"
                          className="nomina-delete-button"
                          disabled={!puedeOperar}
                          aria-label={`Eliminar a ${trabajador.apellidoNombre}`}
                          onClick={() => eliminarTrabajador(trabajador)}
                        >
                          <Trash2 size={18} />
                        </button>
                      </div>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <div className="nomina-secondary-actions">
        <div className="nomina-pagination">
          <span>
            {trabajadores.length > 0
              ? `${inicioPagina + 1}-${Math.min(inicioPagina + TAMANO_PAGINA, trabajadores.length)} de ${trabajadores.length}`
              : "0 de 0"}
          </span>

          <button
            type="button"
            aria-label="Página anterior"
            disabled={paginaActual === 0 || guardando}
            onClick={() => setPagina(paginaActual - 1)}
          >
            ‹
          </button>

          <button
            type="button"
            aria-label="Página siguiente"
            disabled={paginaActual >= totalPaginas - 1 || guardando}
            onClick={() => setPagina(paginaActual + 1)}
          >
            ›
          </button>
        </div>
      </div>

      <div className="nomina-footer-actions">
        <button type="button" className="nomina-back-button" disabled={guardando} onClick={onBack}>
          Volver
        </button>

        <button type="button" className="next-step-button" disabled={!puedeOperar || modalTrabajadorAbierto} onClick={continuar}>
          {guardando ? "Guardando..." : "Continuar"}
        </button>
      </div>

      {mensajeError && (
        <div className="empresa-field-error" role="alert">
          <p>{mensajeError}</p>
          {errorProceso && !tieneNomina && solicitudValida && token && (
            <button type="button" className="nomina-back-button" disabled={cargando} onClick={() => setIntentoCarga((valor) => valor + 1)}>
              Reintentar carga
            </button>
          )}
        </div>
      )}

      {modalTrabajadorAbierto && (
        <TrabajadorModal
          key={trabajadorSeleccionado?.id ?? "nuevo-trabajador"}
          initialData={trabajadorSeleccionado}
          onClose={cerrarModalTrabajador}
          onSave={guardarTrabajador}
        />
      )}
    </section>
  );
}
