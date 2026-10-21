import { useId, useState } from "react";
import { ChevronDown, ChevronUp, Eye, FilePenLine, FileText, Info } from "lucide-react";

import ConfirmacionEnvioModal from "../../components/modals/ConfirmacionEnvioModal.jsx";
import TipoFirmaModal from "../../components/modals/TipoFirmaModal.jsx";

import { CORRECCIONES_REVISION_MOCK, SECCIONES_REVISION_MOCK } from "../../mocks/RevisionMock.js";

import "../../styles/stepRevision.css";

export default function StepRevision({ onBack }) {
  const componenteId = useId();

  const [seccionAbierta, setSeccionAbierta] = useState("estado");
  const [vistaActual, setVistaActual] = useState("estado");
  const [modalAbierto, setModalAbierto] = useState(null);

  const vista = SECCIONES_REVISION_MOCK.find((seccion) => seccion.id === vistaActual) ?? SECCIONES_REVISION_MOCK[0];

  const alternarSeccion = (seccionId) => {
    setVistaActual(seccionId);
    setSeccionAbierta((actual) => (actual === seccionId ? null : seccionId));
  };

  const cerrarModal = () => {
    setModalAbierto(null);
  };

  return (
    <section className="revision-step">
      <div className="revision-introduction">
        <h2 className="section-title">Estado de solicitud: {vista.estado}</h2>

        <p>{vista.descripcion}</p>
      </div>

      <div className="revision-sections">
        {SECCIONES_REVISION_MOCK.map((seccion) => {
          const abierta = seccionAbierta === seccion.id;
          const tituloId = `${componenteId}-${seccion.id}-titulo`;
          const contenidoId = `${componenteId}-${seccion.id}-contenido`;

          return (
            <article key={seccion.id}>
              <button
                id={tituloId}
                type="button"
                className="revision-section-header"
                aria-expanded={abierta}
                aria-controls={contenidoId}
                onClick={() => alternarSeccion(seccion.id)}
              >
                <span>{seccion.titulo}</span>

                {abierta ? <ChevronUp size={17} aria-hidden="true" /> : <ChevronDown size={17} aria-hidden="true" />}
              </button>

              <div id={contenidoId} className="revision-section-content" role="region" aria-labelledby={tituloId} hidden={!abierta}>
                {seccion.id === "estado" && (
                  <>
                    <div className="revision-panel">
                      <p className="revision-panel-title">Pendiente de aprobación</p>

                      <ul className="revision-corrections">
                        {CORRECCIONES_REVISION_MOCK.map((correccion) => (
                          <li className="revision-correction" key={correccion.id}>
                            <span className="revision-correction-text">{correccion.descripcion}</span>

                            <button type="button" className="revision-link-button">
                              {correccion.textoEnlace}
                            </button>
                          </li>
                        ))}
                      </ul>
                    </div>

                    <div className="revision-actions">
                      <button type="button" className="revision-primary-button" onClick={() => setModalAbierto("envio")}>
                        Enviar solicitud
                      </button>
                    </div>
                  </>
                )}

                {seccion.id === "firma" && (
                  <div className="revision-document-row">
                    <div className="revision-document-info">
                      <FileText size={24} aria-hidden="true" />

                      <Info size={14} className="revision-info-icon" aria-hidden="true" />

                      <span>
                        Solicitud de Inscripción Empresaria
                        <span className="revision-required-mark">*</span>
                      </span>
                    </div>

                    <div className="revision-document-actions">
                      <button type="button" className="revision-link-button">
                        <Eye size={18} aria-hidden="true" />
                        Ver solicitud
                      </button>

                      <button type="button" className="revision-primary-button" onClick={() => setModalAbierto("firma")}>
                        <FilePenLine size={18} aria-hidden="true" />
                        Iniciar firma
                      </button>
                    </div>
                  </div>
                )}

                {seccion.id === "pago" && (
                  <div className="revision-payment-row">
                    <div className="revision-panel">
                      <p className="revision-panel-title">Solicitud aprobada</p>

                      <p>Para finalizar el trámite, deberá abonar el arancel correspondiente mediante IERIC Cobros.</p>
                    </div>

                    <button type="button" className="revision-primary-button">
                      Ir a IERIC Cobros
                    </button>
                  </div>
                )}
              </div>
            </article>
          );
        })}
      </div>

      <div className="revision-footer-actions">
        <button type="button" className="revision-secondary-button" onClick={onBack}>
          Volver
        </button>
      </div>

      {modalAbierto === "envio" && <ConfirmacionEnvioModal onClose={cerrarModal} />}

      {modalAbierto === "firma" && <TipoFirmaModal onClose={cerrarModal} />}
    </section>
  );
}
