import { useState } from "react";

import { ArrowUp, ChevronLeft, ChevronRight, Info, Pencil, Plus, Trash2 } from "lucide-react";

import RepresentanteModal from "../../components/modals/RepresentanteModal.jsx";

import "../../styles/stepRepresentantes.css";

const TIPOS_SOCIEDAD_CON_INTEGRANTES = ["ut", "ute", "consorcio-cooperacion"];

export default function StepRepresentantes({
  tipoSociedadId = "",
  empresasIntegrantes = [],
  representantes = [],
  onSave,
  onDelete,
  onBack,
  onNext,
}) {
  const [modalAbierto, setModalAbierto] = useState(false);

  const [representanteSeleccionado, setRepresentanteSeleccionado] = useState(null);

  const esEmpresaConIntegrantes = TIPOS_SOCIEDAD_CON_INTEGRANTES.includes(tipoSociedadId);

  const hayEmpresasIntegrantes = empresasIntegrantes.length > 0;

  const hayRepresentantes = representantes.length > 0;

  const puedeAgregarRepresentante = !esEmpresaConIntegrantes || hayEmpresasIntegrantes;

  const obtenerNombreEmpresa = (representante) => {
    const empresaActual = empresasIntegrantes.find((empresa) => empresa.id === representante.empresaId);

    return empresaActual?.razonSocial || representante.empresaNombre || "—";
  };

  const abrirNuevoRepresentante = () => {
    if (!puedeAgregarRepresentante) {
      return;
    }

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
          <h2 className="section-title representantes-title">Representantes y apoderados</h2>

          <p className="representantes-subtitle">La documentación respaldatoria se solicitará en el apartado Documentación.</p>
        </div>

        <button
          type="button"
          className="representantes-add-button"
          disabled={!puedeAgregarRepresentante}
          title={puedeAgregarRepresentante ? undefined : "Primero debe cargar una empresa integrante"}
          onClick={abrirNuevoRepresentante}
        >
          <Plus size={18} aria-hidden="true" />
          Agregar representante
        </button>
      </div>

      <div className="representantes-table-wrapper">
        <table
          className={["representantes-table", esEmpresaConIntegrantes ? "representantes-table--con-empresa" : ""].filter(Boolean).join(" ")}
        >
          <thead>
            <tr>
              {esEmpresaConIntegrantes && <th>Empresa</th>}

              <th>
                <span className="representantes-sortable-header">
                  Apellido
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>
                <span className="representantes-sortable-header">
                  Nombre
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>
                <span className="representantes-sortable-header">
                  CUIL
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>Correo electrónico</th>
              <th>Teléfono</th>
              <th>Cargo</th>
            </tr>
          </thead>

          <tbody>
            {!hayRepresentantes && (
              <tr className="representantes-placeholder-row">
                {esEmpresaConIntegrantes && <td>Empresa integrante</td>}

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
                {esEmpresaConIntegrantes && <td>{obtenerNombreEmpresa(representante)}</td>}

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
                        <Pencil size={18} aria-hidden="true" />
                      </button>

                      <button
                        type="button"
                        className="representantes-delete-button"
                        aria-label={`Eliminar a ${representante.nombre} ${representante.apellido}`}
                        onClick={() => onDelete?.(representante.id)}
                      >
                        <Trash2 size={18} aria-hidden="true" />
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

          <span>
            {esEmpresaConIntegrantes && !hayEmpresasIntegrantes
              ? "Debe volver al paso Empresa y cargar al menos una empresa integrante."
              : "Todavía no se agregaron representantes."}
          </span>
        </div>
      )}

      <div className="representantes-pagination">
        <span>{hayRepresentantes ? `1-${representantes.length} de ${representantes.length}` : "0 de 0"}</span>

        <button type="button" aria-label="Página anterior" disabled>
          <ChevronLeft size={17} aria-hidden="true" />
        </button>

        <button type="button" aria-label="Página siguiente" disabled>
          <ChevronRight size={17} aria-hidden="true" />
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
          requiereEmpresaIntegrante={esEmpresaConIntegrantes}
          empresasIntegrantes={empresasIntegrantes}
          onClose={cerrarModal}
          onSave={guardarRepresentante}
        />
      )}
    </section>
  );
}
