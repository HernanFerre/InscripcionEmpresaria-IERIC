import { useState } from "react";

import { ArrowUp, ChevronLeft, ChevronRight, Info, Pencil, Plus, Trash2 } from "lucide-react";

import RepresentanteModal from "../../components/modals/RepresentanteModal.jsx";

import "../../styles/stepRepresentantes.css";

export default function StepRepresentantes({ representantes = [], onSave, onDelete, onBack, onNext }) {
  const [modalAbierto, setModalAbierto] = useState(false);

  const [representanteSeleccionado, setRepresentanteSeleccionado] = useState(null);

  const hayRepresentantes = representantes.length > 0;

  const abrirNuevoRepresentante = () => {
    setRepresentanteSeleccionado(null);
    setModalAbierto(true);
  };

  const abrirEdicionRepresentante = (representante) => {
    setRepresentanteSeleccionado(representante);
    setModalAbierto(true);
  };

  const cerrarModal = () => {
    setModalAbierto(false);
    setRepresentanteSeleccionado(null);
  };

  const guardarRepresentante = (datosRepresentante) => {
    onSave?.(datosRepresentante);
    cerrarModal();
  };

  return (
    <section className="representantes-step">
      <div className="representantes-header">
        <div>
          <h2 className="representantes-title">Representantes y apoderados</h2>

          <p className="representantes-subtitle">La documentación respaldatoria se solicitará en el apartado Documentación.</p>
        </div>

        <button type="button" className="representantes-add-button" onClick={abrirNuevoRepresentante}>
          <Plus size={18} aria-hidden="true" />
          Agregar Representante
        </button>
      </div>

      <div className="representantes-table-wrapper">
        <table className="representantes-table">
          <thead>
            <tr>
              <th>
                <span className="representantes-sortable-header">
                  APELLIDO
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>
                <span className="representantes-sortable-header">
                  NOMBRE
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>
                <span className="representantes-sortable-header">
                  CUIL
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>CORREO ELECTRÓNICO</th>
              <th>TELÉFONO</th>
              <th>CARGO</th>
            </tr>
          </thead>

          <tbody>
            {!hayRepresentantes && (
              <tr className="representantes-placeholder-row">
                <td>Apellido</td>
                <td>Nombre</td>
                <td>XX-XXXXXXXX-X</td>
                <td>ejemplo@correo.com</td>
                <td>+XX XX XXXX-XXXX</td>
                <td>Cargo</td>
              </tr>
            )}

            {representantes.map((representante) => (
              <tr key={representante.id}>
                <td>{representante.apellido}</td>
                <td>{representante.nombre}</td>
                <td>{representante.cuil}</td>
                <td>{representante.email}</td>
                <td>{representante.telefono}</td>

                <td>
                  <div className="representantes-cargo-cell">
                    <span>{representante.cargo}</span>

                    <div className="representantes-row-actions">
                      <button
                        type="button"
                        className="representantes-edit-button"
                        aria-label={`Editar a ${representante.nombre} ${representante.apellido}`}
                        onClick={() => abrirEdicionRepresentante(representante)}
                      >
                        <Pencil size={18} />
                      </button>

                      <button
                        type="button"
                        className="representantes-delete-button"
                        aria-label={`Eliminar a ${representante.nombre} ${representante.apellido}`}
                        onClick={() => onDelete?.(representante.id)}
                      >
                        <Trash2 size={18} />
                      </button>
                    </div>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {!hayRepresentantes && (
        <div className="representantes-info" role="status">
          <Info size={15} aria-hidden="true" />
          <span>Info/Error/Warning</span>
        </div>
      )}

      <div className="representantes-pagination">
        <span>{hayRepresentantes ? `1-${representantes.length} de ${representantes.length}` : "0 de 0"}</span>

        <button type="button" aria-label="Página anterior" disabled>
          <ChevronLeft size={17} />
        </button>

        <button type="button" aria-label="Página siguiente" disabled>
          <ChevronRight size={17} />
        </button>
      </div>

      <div className="representantes-footer-actions">
        <button type="button" className="representantes-back-button" onClick={onBack}>
          Volver
        </button>

        <button type="button" className="next-step-button" onClick={() => onNext?.(representantes)}>
          Continuar
        </button>
      </div>

      {modalAbierto && (
        <RepresentanteModal
          key={representanteSeleccionado?.id ?? "nuevo-representante"}
          initialData={representanteSeleccionado}
          onClose={cerrarModal}
          onSave={guardarRepresentante}
        />
      )}
    </section>
  );
}
