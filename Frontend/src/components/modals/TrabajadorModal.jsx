import { useState } from "react";
import { X } from "lucide-react";

import "../../styles/components/trabajadorModal.css";

const DATOS_INICIALES = {
  apellidoNombre: "",
  cuil: "",
  ingreso: "",
  egreso: "",
};

export default function TrabajadorModal({ initialData = null, onClose, onSave }) {
  const [datos, setDatos] = useState({
    ...DATOS_INICIALES,
    ...(initialData ?? {}),
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
          <h2 id="trabajador-modal-title">Trabajador</h2>

          <button type="button" className="trabajador-modal-close" aria-label="Cerrar" onClick={onClose}>
            <X size={20} />
          </button>
        </header>

        <form onSubmit={handleSubmit}>
          <div className="trabajador-modal-body">
            <div className="trabajador-modal-grid">
              <input
                className="trabajador-modal-input"
                type="text"
                name="apellidoNombre"
                value={datos.apellidoNombre}
                placeholder="Apellido y nombre*"
                aria-label="Apellido y nombre"
                required
                onChange={actualizarCampo}
              />

              <input
                className="trabajador-modal-input"
                type="text"
                name="cuil"
                value={datos.cuil}
                placeholder="CUIL*"
                aria-label="CUIL"
                required
                onChange={actualizarCampo}
              />

              <input
                className="trabajador-modal-input"
                type="text"
                name="ingreso"
                value={datos.ingreso}
                placeholder="Fecha de ingreso*"
                aria-label="Fecha de ingreso"
                required
                onChange={actualizarCampo}
              />

              <input
                className="trabajador-modal-input"
                type="text"
                name="egreso"
                value={datos.egreso}
                placeholder="Fecha de egreso"
                aria-label="Fecha de egreso"
                onChange={actualizarCampo}
              />
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
