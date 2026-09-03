import { useState } from "react";
import { Info, X } from "lucide-react";

import SearchableSelect from "../forms/SearchableSelect.jsx";

import { TIPOS_SOCIEDAD_MOCK } from "../../mocks/InscripcionCatalogosMock.js";

import "../../styles/components/empresaIntegranteModal.css";

const DATOS_INICIALES = {
  cuit: "",
  razonSocial: "",
  tipoSociedadId: "",
};

export default function EmpresaIntegranteModal({ initialData = null, onClose, onSave }) {
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

    const camposCompletos = [datos.cuit, datos.razonSocial, datos.tipoSociedadId].every((value) => String(value).trim().length > 0);

    if (!camposCompletos) {
      return;
    }

    const tipoSociedadSeleccionado = TIPOS_SOCIEDAD_MOCK.find((tipoSociedad) => tipoSociedad.value === datos.tipoSociedadId);

    onSave?.({
      ...datos,
      tipoSociedad: tipoSociedadSeleccionado?.label ?? "",
    });
  };

  return (
    <div className="empresa-integrante-modal-overlay" role="presentation" onMouseDown={onClose}>
      <div
        className="empresa-integrante-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="empresa-integrante-modal-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <header className="empresa-integrante-modal-header">
          <div className="empresa-integrante-modal-title-wrapper">
            <h2 id="empresa-integrante-modal-title">Empresa integrante</h2>

            <span className="empresa-integrante-modal-info" tabIndex="0" aria-label="Más información">
              <Info size={16} aria-hidden="true" />

              <span className="empresa-integrante-modal-tooltip" role="tooltip">
                Deberá informar los representantes en el módulo REPRESENTANTES.
              </span>
            </span>
          </div>

          <button type="button" className="empresa-integrante-modal-close" aria-label="Cerrar" onClick={onClose}>
            <X size={20} aria-hidden="true" />
          </button>
        </header>

        <form onSubmit={handleSubmit}>
          <div className="empresa-integrante-modal-body">
            <div className="empresa-integrante-modal-grid">
              <input
                className="empresa-integrante-modal-input"
                type="text"
                name="cuit"
                value={datos.cuit}
                placeholder="CUIT*"
                aria-label="CUIT"
                required
                onChange={actualizarCampo}
              />

              <input
                className="empresa-integrante-modal-input"
                type="text"
                name="razonSocial"
                value={datos.razonSocial}
                placeholder="Razón social*"
                aria-label="Razón social"
                required
                onChange={actualizarCampo}
              />

              <div className="empresa-integrante-modal-sociedad">
                <SearchableSelect
                  id="tipo-sociedad-integrante"
                  value={datos.tipoSociedadId}
                  options={TIPOS_SOCIEDAD_MOCK}
                  placeholder="Tipo de sociedad"
                  required
                  onChange={(value) => actualizarValor("tipoSociedadId", value)}
                />
              </div>
            </div>
          </div>

          <footer className="empresa-integrante-modal-footer">
            <button type="button" className="empresa-integrante-modal-cancel" onClick={onClose}>
              Cancelar
            </button>

            <button type="submit" className="empresa-integrante-modal-save">
              Guardar
            </button>
          </footer>
        </form>
      </div>
    </div>
  );
}
