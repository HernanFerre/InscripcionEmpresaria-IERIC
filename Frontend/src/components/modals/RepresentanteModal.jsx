import { useState } from "react";
import { X } from "lucide-react";

import SearchableSelect from "../forms/SearchableSelect.jsx";

import "../../styles/components/representanteModal.css";

const CARGOS_REPRESENTANTE_MOCK = [
  { value: "presidente", label: "Presidente" },
  { value: "vicepresidente", label: "Vicepresidente" },
  { value: "director", label: "Director" },
  { value: "socio-gerente", label: "Socio gerente" },
  { value: "apoderado", label: "Apoderado" },
  { value: "representante-legal", label: "Representante legal" },
];

const DATOS_INICIALES = {
  apellido: "",
  nombre: "",
  email: "",
  telefono: "",
  cuil: "",
  cargoId: "",
};

export default function RepresentanteModal({ initialData = null, onClose, onSave }) {
  const [datos, setDatos] = useState({
    ...DATOS_INICIALES,
    ...(initialData ?? {}),
  });

  const actualizarValor = (campo, value) => {
    setDatos((prev) => ({
      ...prev,
      [campo]: value,
    }));
  };

  const actualizarCampo = (event) => {
    actualizarValor(event.target.name, event.target.value);
  };

  const handleSubmit = (event) => {
    event.preventDefault();

    const camposObligatoriosCompletos = [datos.apellido, datos.nombre, datos.email, datos.telefono, datos.cuil, datos.cargoId].every(
      (value) => String(value).trim().length > 0,
    );

    if (!camposObligatoriosCompletos) {
      return;
    }

    const cargoSeleccionado = CARGOS_REPRESENTANTE_MOCK.find((cargo) => cargo.value === datos.cargoId);

    onSave?.({
      ...datos,
      cargo: cargoSeleccionado?.label ?? "",
      firmaAutorizada: false,
    });
  };

  return (
    <div className="representante-modal-overlay" role="presentation" onMouseDown={onClose}>
      <div
        className="representante-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="representante-modal-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <header className="representante-modal-header">
          <h2 id="representante-modal-title" className="section-title">
            Representante
          </h2>

          <button type="button" className="representante-modal-close" aria-label="Cerrar" onClick={onClose}>
            <X size={20} />
          </button>
        </header>

        <form onSubmit={handleSubmit}>
          <div className="representante-modal-body">
            <div className="representante-modal-grid">
              <div className="representante-modal-field">
                <label className="form-field-label" htmlFor="representante-apellido">
                  Apellido
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="representante-apellido"
                  className="representante-modal-input"
                  type="text"
                  name="apellido"
                  value={datos.apellido}
                  placeholder="Apellido"
                  required
                  onChange={actualizarCampo}
                />
              </div>

              <div className="representante-modal-field">
                <label className="form-field-label" htmlFor="representante-nombre">
                  Nombre
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="representante-nombre"
                  className="representante-modal-input"
                  type="text"
                  name="nombre"
                  value={datos.nombre}
                  placeholder="Nombre"
                  required
                  onChange={actualizarCampo}
                />
              </div>

              <div className="representante-modal-field">
                <label className="form-field-label" htmlFor="representante-cuil">
                  CUIL
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="representante-cuil"
                  className="representante-modal-input"
                  type="text"
                  name="cuil"
                  value={datos.cuil}
                  placeholder="XX-XXXXXXXX-X"
                  required
                  onChange={actualizarCampo}
                />
              </div>

              <div className="representante-modal-field">
                <SearchableSelect
                  id="cargo-representante"
                  label="Cargo"
                  value={datos.cargoId}
                  options={CARGOS_REPRESENTANTE_MOCK}
                  placeholder="Busque y seleccione el cargo"
                  required
                  onChange={(value) => actualizarValor("cargoId", value)}
                />
              </div>

              <div className="representante-modal-field">
                <label className="form-field-label" htmlFor="representante-correo">
                  Correo electrónico
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="representante-correo"
                  className="representante-modal-input"
                  type="email"
                  name="email"
                  value={datos.email}
                  placeholder="Correo electrónico"
                  required
                  onChange={actualizarCampo}
                />
              </div>

              <div className="representante-modal-field">
                <label className="form-field-label" htmlFor="representante-telefono">
                  Teléfono
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="representante-telefono"
                  className="representante-modal-input"
                  type="tel"
                  name="telefono"
                  value={datos.telefono}
                  placeholder="Teléfono"
                  required
                  onChange={actualizarCampo}
                />
              </div>
            </div>
          </div>

          <footer className="representante-modal-footer">
            <button type="button" className="representante-modal-cancel" onClick={onClose}>
              Cancelar
            </button>

            <button type="submit" className="representante-modal-save">
              Guardar
            </button>
          </footer>
        </form>
      </div>
    </div>
  );
}
