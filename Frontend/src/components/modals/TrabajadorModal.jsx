import { useState } from "react";
import { X } from "lucide-react";

import "../../styles/components/trabajadorModal.css";

const DATOS_INICIALES = {
  apellidoNombre: "",
  cuil: "",
  ingreso: "",
  egreso: "",
};

function normalizarFechaParaInput(value) {
  const fecha = String(value ?? "").trim();

  if (!fecha) {
    return "";
  }

  if (/^\d{4}-\d{2}-\d{2}$/.test(fecha)) {
    return fecha;
  }

  const coincidencia = fecha.match(/^(\d{2})\/(\d{2})\/(\d{4})$/);

  if (!coincidencia) {
    return "";
  }

  const [, dia, mes, anio] = coincidencia;

  return `${anio}-${mes}-${dia}`;
}

export default function TrabajadorModal({ initialData = null, onClose, onSave }) {
  const [datos, setDatos] = useState({
    ...DATOS_INICIALES,
    ...(initialData ?? {}),
    ingreso: normalizarFechaParaInput(initialData?.ingreso),
    egreso: normalizarFechaParaInput(initialData?.egreso),
  });

  const actualizarCampo = (event) => {
    const { name, value } = event.target;

    setDatos((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = (event) => {
    event.preventDefault();

    const camposObligatoriosCompletos = [datos.apellidoNombre, datos.cuil, datos.ingreso].every((value) => String(value).trim().length > 0);

    if (!camposObligatoriosCompletos) {
      return;
    }

    onSave?.({
      ...datos,
      perteneceConstruccion: true,
      origen: "manual",
    });
  };

  return (
    <div className="trabajador-modal-overlay" role="presentation" onMouseDown={onClose}>
      <div
        className="trabajador-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="trabajador-modal-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <header className="trabajador-modal-header">
          <h2 id="trabajador-modal-title" className="section-title">
            Trabajador
          </h2>

          <button type="button" className="trabajador-modal-close" aria-label="Cerrar" onClick={onClose}>
            <X size={20} />
          </button>
        </header>

        <form onSubmit={handleSubmit}>
          <div className="trabajador-modal-body">
            <div className="trabajador-modal-grid">
              <div className="trabajador-modal-field">
                <label className="form-field-label" htmlFor="trabajador-apellido-nombre">
                  Apellido y nombre
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="trabajador-apellido-nombre"
                  className="trabajador-modal-input"
                  type="text"
                  name="apellidoNombre"
                  value={datos.apellidoNombre}
                  placeholder="Sin comas ni separaciones"
                  required
                  onChange={actualizarCampo}
                />
              </div>

              <div className="trabajador-modal-field">
                <label className="form-field-label" htmlFor="trabajador-cuil">
                  CUIL
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="trabajador-cuil"
                  className="trabajador-modal-input"
                  type="text"
                  name="cuil"
                  value={datos.cuil}
                  placeholder="XX-XXXXXXXX-X"
                  required
                  onChange={actualizarCampo}
                />
              </div>

              <div className="trabajador-modal-field">
                <label className="form-field-label" htmlFor="trabajador-ingreso">
                  Fecha de ingreso
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="trabajador-ingreso"
                  className="trabajador-modal-input"
                  type="date"
                  name="ingreso"
                  value={datos.ingreso}
                  required
                  onChange={actualizarCampo}
                />
              </div>

              <div className="trabajador-modal-field">
                <label className="form-field-label" htmlFor="trabajador-egreso">
                  Fecha de egreso
                </label>

                <input
                  id="trabajador-egreso"
                  className="trabajador-modal-input"
                  type="date"
                  name="egreso"
                  value={datos.egreso}
                  onChange={actualizarCampo}
                />
              </div>
            </div>
          </div>

          <footer className="trabajador-modal-footer">
            <button type="button" className="trabajador-modal-cancel" onClick={onClose}>
              Cancelar
            </button>

            <button type="submit" className="trabajador-modal-save">
              Guardar
            </button>
          </footer>
        </form>
      </div>
    </div>
  );
}
