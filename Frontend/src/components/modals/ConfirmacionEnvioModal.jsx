import { useEffect, useId, useRef } from "react";
import { createPortal } from "react-dom";
import { X } from "lucide-react";

import "../../styles/components/confirmacionEnvioModal.css";

export default function ConfirmacionEnvioModal({ onClose, onConfirm }) {
  const modalRef = useRef(null);
  const tituloId = useId();
  const descripcionId = useId();

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
      className="confirmacion-envio-modal"
      aria-labelledby={tituloId}
      aria-describedby={descripcionId}
      aria-modal="true"
      onCancel={cerrarConEscape}
      onMouseDown={cerrarDesdeFondo}
    >
      <header className="confirmacion-envio-modal-header">
        <h2 id={tituloId} className="confirmacion-envio-modal-title">
          Confirmación de envío
        </h2>

        <button type="button" className="confirmacion-envio-modal-close" aria-label="Cerrar confirmación de envío" onClick={onClose}>
          <X size={20} aria-hidden="true" />
        </button>
      </header>

      <div className="confirmacion-envio-modal-body">
        <p id={descripcionId} className="confirmacion-envio-modal-message">
          Verifique que los datos y la documentación sean correctos.
          <br />
          Una vez enviada, la solicitud no podrá modificarse.
        </p>
      </div>

      <footer className="confirmacion-envio-modal-footer">
        <button type="button" className="confirmacion-envio-modal-cancel" autoFocus onClick={onClose}>
          Cancelar
        </button>

        <button type="button" className="confirmacion-envio-modal-submit" onClick={onConfirm}>
          Enviar solicitud
        </button>
      </footer>
    </dialog>,
    document.body,
  );
}
