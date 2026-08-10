import { useState } from "react";

import { ArrowUp, FileText, Info, Pencil, Plus, Trash2 } from "lucide-react";

import TrabajadorModal from "../../components/modals/TrabajadorModal.jsx";
import DeclaracionJuradaModal from "../../components/modals/DeclaracionJuradaModal.jsx";

import "../../styles/stepNomina.css";

const TRABAJADORES_MOCK = [
  {
    id: "trabajador-1",
    apellidoNombre: "Rodriguez, Alberto Fabian",
    cuil: "20-30456789-2",
    ingreso: "15/02/2021",
    egreso: "",
    perteneceConstruccion: true,
    origen: "manual",
  },
  {
    id: "trabajador-2",
    apellidoNombre: "Gomez, María Elena",
    cuil: "27-32987654-1",
    ingreso: "01/06/2023",
    egreso: "",
    perteneceConstruccion: true,
    origen: "manual",
  },
  {
    id: "trabajador-3",
    apellidoNombre: "Sanchez, Patricia",
    cuil: "27-69001110-4",
    ingreso: "05/03/2024",
    egreso: "01/01/2026",
    perteneceConstruccion: false,
    origen: "servicio",
  },
  {
    id: "trabajador-4",
    apellidoNombre: "Martinez, Carlos",
    cuil: "20-25123456-8",
    ingreso: "20/11/2022",
    egreso: "10/03/2025",
    perteneceConstruccion: true,
    origen: "servicio",
  },
];

export default function StepNomina({ onSaveTrabajador, onSaveDeclaracion, onDelete, onBack, onNext }) {
  const [pertenenciaConstruccion, setPertenenciaConstruccion] = useState(() =>
    Object.fromEntries(TRABAJADORES_MOCK.map((trabajador) => [trabajador.id, trabajador.perteneceConstruccion])),
  );

  const [modalTrabajadorAbierto, setModalTrabajadorAbierto] = useState(false);

  const [trabajadorSeleccionado, setTrabajadorSeleccionado] = useState(null);

  const [modalDeclaracionAbierto, setModalDeclaracionAbierto] = useState(false);

  const actualizarPertenencia = (trabajador, value) => {
    if (trabajador.origen !== "servicio") {
      return;
    }

    setPertenenciaConstruccion((prev) => ({
      ...prev,
      [trabajador.id]: value,
    }));
  };

  const abrirNuevoTrabajador = () => {
    setTrabajadorSeleccionado(null);
    setModalTrabajadorAbierto(true);
  };

  const abrirEdicionTrabajador = (trabajador) => {
    setTrabajadorSeleccionado(trabajador);
    setModalTrabajadorAbierto(true);
  };

  const cerrarModalTrabajador = () => {
    setModalTrabajadorAbierto(false);
    setTrabajadorSeleccionado(null);
  };

  const guardarTrabajador = (datosTrabajador) => {
    onSaveTrabajador?.(datosTrabajador);
    cerrarModalTrabajador();
  };

  const abrirModalDeclaracion = () => {
    setModalDeclaracionAbierto(true);
  };

  const cerrarModalDeclaracion = () => {
    setModalDeclaracionAbierto(false);
  };

  const guardarDeclaracion = (datosDeclaracion) => {
    onSaveDeclaracion?.(datosDeclaracion);
    cerrarModalDeclaracion();
  };

  return (
    <section className="nomina-step">
      <div className="nomina-header">
        <div className="nomina-start-info">
          <div>
            <h2 className="nomina-title">Inicio de actividades</h2>

            <p className="nomina-subtitle">Información detallada por IERIC.</p>
          </div>

          <span className="nomina-start-date">15/02/2021</span>

          <div className="nomina-info-wrapper">
            <button type="button" className="nomina-info-button" aria-label="Información sobre el inicio de actividades">
              <Info size={18} />
            </button>

            <div className="nomina-info-tooltip" role="tooltip">
              La fecha de ingreso del trabajador más antiguo será considerada como Inicio de Actividades y reviste carácter de Declaración
              Jurada.
              <br />
              Información proveniente del trabajador: Rodriguez, Alberto Fabian (20-30456789-2).
            </div>
          </div>
        </div>

        <button type="button" className="nomina-add-button" onClick={abrirNuevoTrabajador}>
          <Plus size={18} aria-hidden="true" />
          Agregar Trabajador
        </button>
      </div>

      <div className="nomina-table-wrapper">
        <table className="nomina-table">
          <thead>
            <tr>
              <th>
                <span className="nomina-sortable-header">
                  APELLIDO Y NOMBRE
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>
                <span className="nomina-sortable-header">
                  CUIL
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>
                <span className="nomina-sortable-header">
                  INGRESO
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>
                <span className="nomina-sortable-header">
                  EGRESO
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>PERTENECE A CONSTRUCCIÓN</th>
              <th aria-label="Acciones" />
            </tr>
          </thead>

          <tbody>
            {TRABAJADORES_MOCK.map((trabajador) => {
              const pertenece = pertenenciaConstruccion[trabajador.id];

              const esManual = trabajador.origen === "manual";

              return (
                <tr key={trabajador.id}>
                  <td>{trabajador.apellidoNombre}</td>
                  <td>{trabajador.cuil}</td>
                  <td>{trabajador.ingreso}</td>

                  <td className={trabajador.egreso ? "nomina-egreso" : ""}>{trabajador.egreso || "—"}</td>

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
                          onChange={() => actualizarPertenencia(trabajador, true)}
                        />
                      </label>

                      <label>
                        <span>No</span>

                        <input
                          type="radio"
                          name={`construccion-${trabajador.id}`}
                          checked={pertenece === false}
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
                          aria-label={`Editar a ${trabajador.apellidoNombre}`}
                          onClick={() => abrirEdicionTrabajador(trabajador)}
                        >
                          <Pencil size={18} />
                        </button>

                        <button
                          type="button"
                          className="nomina-delete-button"
                          aria-label={`Eliminar a ${trabajador.apellidoNombre}`}
                          onClick={() => onDelete?.(trabajador.id)}
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
        <button type="button" className="nomina-declaration-button" onClick={abrirModalDeclaracion}>
          <FileText size={17} aria-hidden="true" />
          Declaración Jurada
        </button>

        <div className="nomina-pagination">
          <span>1-4 de 4</span>

          <button type="button" aria-label="Página anterior" disabled>
            ‹
          </button>

          <button type="button" aria-label="Página siguiente" disabled>
            ›
          </button>
        </div>
      </div>

      <div className="nomina-footer-actions">
        <button type="button" className="nomina-back-button" onClick={onBack}>
          Volver
        </button>

        <button type="button" className="next-step-button" onClick={onNext}>
          Continuar
        </button>
      </div>

      {modalTrabajadorAbierto && (
        <TrabajadorModal
          key={trabajadorSeleccionado?.id ?? "nuevo-trabajador"}
          initialData={trabajadorSeleccionado}
          onClose={cerrarModalTrabajador}
          onSave={guardarTrabajador}
        />
      )}

      {modalDeclaracionAbierto && <DeclaracionJuradaModal onClose={cerrarModalDeclaracion} onSave={guardarDeclaracion} />}
    </section>
  );
}
