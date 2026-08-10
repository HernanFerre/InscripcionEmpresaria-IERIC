import { useState } from "react";

import { ChevronDown, ChevronUp, Eye, FileText, Info, Plus, Trash2 } from "lucide-react";

import DocumentoUploadModal from "../../components/modals/DocumentoUploadModal.jsx";

import { CONFIGURACION_DOCUMENTOS_MOCK, SECCIONES_DOCUMENTACION_MOCK } from "../../mocks/DocumentacionMock.js";

import "../../styles/stepDocumentacion.css";

export default function StepDocumentacion({ onViewDocumento, onDeleteDocumento, onBack, onNext }) {
  const [seccionesAbiertas, setSeccionesAbiertas] = useState(() =>
    Object.fromEntries(SECCIONES_DOCUMENTACION_MOCK.map((seccion) => [seccion.id, seccion.abiertaInicialmente])),
  );

  const [documentoSeleccionado, setDocumentoSeleccionado] = useState(null);

  const alternarSeccion = (seccionId) => {
    setSeccionesAbiertas((prev) => ({
      ...prev,
      [seccionId]: !prev[seccionId],
    }));
  };

  const abrirModalDocumento = (seccion, documento) => {
    setDocumentoSeleccionado({
      seccion,
      documento,
    });
  };

  const cerrarModalDocumento = () => {
    setDocumentoSeleccionado(null);
  };

  const handleArchivoSeleccionado = () => {
    // En esta etapa mock no persistimos el archivo.
    cerrarModalDocumento();
  };

  return (
    <section className="documentacion-step">
      <div className="documentacion-introduction">
        <h2>Requisitos adjuntos</h2>

        <p>Información detallada por IERIC. --Sólo se admiten archivos PDF de hasta {CONFIGURACION_DOCUMENTOS_MOCK.tamanoMaximoMb}MB.--</p>
      </div>

      <div className="documentacion-sections">
        {SECCIONES_DOCUMENTACION_MOCK.map((seccion) => {
          const abierta = seccionesAbiertas[seccion.id];

          return (
            <article className="documentacion-section" key={seccion.id}>
              <button
                type="button"
                className="documentacion-section-header"
                aria-expanded={abierta}
                aria-controls={`documentacion-${seccion.id}`}
                onClick={() => alternarSeccion(seccion.id)}
              >
                <span>{seccion.titulo}</span>

                {abierta ? <ChevronUp size={17} aria-hidden="true" /> : <ChevronDown size={17} aria-hidden="true" />}
              </button>

              {abierta && (
                <div id={`documentacion-${seccion.id}`} className="documentacion-section-content">
                  {seccion.documentos.map((documento) => {
                    const tieneArchivo = documento.archivo !== null;

                    return (
                      <div className="documentacion-document-row" key={documento.id}>
                        <div className="documentacion-pdf-icon">
                          <FileText size={23} aria-hidden="true" />
                          <span>PDF</span>
                        </div>

                        <div className="documentacion-document-info">
                          <div className="documentacion-document-title">
                            <Info size={14} aria-hidden="true" />

                            <span>
                              {documento.titulo}
                              {documento.obligatorio ? " *" : ""}
                            </span>
                          </div>

                          {tieneArchivo && (
                            <div className="documentacion-file-data">
                              <button
                                type="button"
                                className="documentacion-file-name"
                                onClick={() =>
                                  onViewDocumento?.({
                                    seccion,
                                    documento,
                                  })
                                }
                              >
                                {documento.archivo.nombre}
                              </button>

                              <span className="documentacion-file-separator" />

                              <span>{documento.archivo.tamano}</span>
                            </div>
                          )}
                        </div>

                        <div className="documentacion-document-actions">
                          <button
                            type="button"
                            className="documentacion-add-file"
                            aria-label={`Agregar archivo para ${documento.titulo}`}
                            onClick={() => abrirModalDocumento(seccion, documento)}
                          >
                            <Plus size={19} />
                          </button>

                          {tieneArchivo && (
                            <>
                              <button
                                type="button"
                                className="documentacion-view-file"
                                aria-label={`Ver archivo de ${documento.titulo}`}
                                onClick={() =>
                                  onViewDocumento?.({
                                    seccion,
                                    documento,
                                  })
                                }
                              >
                                <Eye size={19} />
                              </button>

                              <button
                                type="button"
                                className="documentacion-delete-file"
                                aria-label={`Eliminar archivo de ${documento.titulo}`}
                                onClick={() =>
                                  onDeleteDocumento?.({
                                    seccion,
                                    documento,
                                  })
                                }
                              >
                                <Trash2 size={18} />
                              </button>
                            </>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </article>
          );
        })}
      </div>

      <div className="documentacion-footer-actions">
        <button type="button" className="documentacion-back-button" onClick={onBack}>
          Volver
        </button>

        <button type="button" className="next-step-button" onClick={onNext}>
          Continuar
        </button>
      </div>

      {documentoSeleccionado && (
        <DocumentoUploadModal
          documentTitle={documentoSeleccionado.documento.titulo}
          onClose={cerrarModalDocumento}
          onFileSelected={handleArchivoSeleccionado}
        />
      )}
    </section>
  );
}
