import { useEffect, useId, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { ExternalLink, X } from "lucide-react";

import { TIPOS_FIRMA_REVISION_MOCK } from "../../mocks/RevisionMock.js";

import "../../styles/components/tipoFirmaModal.css";

export default function TipoFirmaModal({ onClose, onContinue }) {
  const modalRef = useRef(null);
  const tituloId = useId();
  const grupoFirmaId = useId();

  const [tipoFirma, setTipoFirma] = useState("digital");

  useEffect(() => {
    const modal = modalRef.current;
    const elementoAnterior = document.activeElement;
    const overflowAnterior = document.body.style.overflow;

    if (!modal.open) {
      modal.showModal();
    }

    document.body.style.overflow = "hidden";

    return () => {
      if (modal.open) {
        modal.close();
      }

      document.body.style.overflow = overflowAnterior;

      if (elementoAnterior instanceof HTMLElement && elementoAnterior.isConnected) {
        elementoAnterior.focus();
      }
    };
  }, []);

  const cerrarConEscape = (event) => {
    event.preventDefault();
    onClose?.();
  };

  const cerrarDesdeFondo = (event) => {
    if (event.target !== event.currentTarget) {
      return;
    }

    const limites = event.currentTarget.getBoundingClientRect();

    const fueraDelModal =
      event.clientX < limites.left || event.clientX > limites.right || event.clientY < limites.top || event.clientY > limites.bottom;

    if (fueraDelModal) {
      onClose?.();
    }
  };

  return createPortal(
    <dialog
      ref={modalRef}
      className="tipo-firma-modal"
      aria-labelledby={tituloId}
      aria-modal="true"
      onCancel={cerrarConEscape}
      onMouseDown={cerrarDesdeFondo}
    >
      <header className="tipo-firma-modal-header">
        <h2 id={tituloId} className="tipo-firma-modal-title">
          Seleccione el tipo de firma
        </h2>

        <button type="button" className="tipo-firma-modal-close" aria-label="Cerrar selección de firma" onClick={onClose}>
          <X size={20} aria-hidden="true" />
        </button>
      </header>

      <div className="tipo-firma-modal-body">
        <fieldset className="tipo-firma-modal-options" aria-label="Tipo de firma">
          {TIPOS_FIRMA_REVISION_MOCK.map((tipo) => {
            const seleccionado = tipoFirma === tipo.id;
            const opcionId = `${grupoFirmaId}-${tipo.id}`;

            return (
              <div key={tipo.id} className={`tipo-firma-modal-option${seleccionado ? " selected" : ""}`}>
                <label className="tipo-firma-modal-choice" htmlFor={opcionId}>
                  <input
                    id={opcionId}
                    type="radio"
                    name={grupoFirmaId}
                    value={tipo.id}
                    checked={seleccionado}
                    autoFocus={tipo.id === "digital"}
                    onChange={() => setTipoFirma(tipo.id)}
                  />

                  <span>
                    <span className="tipo-firma-modal-option-title">{tipo.titulo}</span>

                    <span className="tipo-firma-modal-description">{tipo.descripcion}</span>
                  </span>
                </label>

                {seleccionado && (
                  <div className="tipo-firma-modal-details">
                    <p className="tipo-firma-modal-notice">{tipo.aviso}</p>

                    <div className="tipo-firma-modal-actions">
                      <button type="button" className="tipo-firma-modal-action" onClick={() => onContinue?.(tipo.id)}>
                        {tipo.id === "digital" && <ExternalLink size={18} aria-hidden="true" />}

                        {tipo.textoBoton}
                      </button>
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </fieldset>
      </div>
    </dialog>,
    document.body,
  );
}
