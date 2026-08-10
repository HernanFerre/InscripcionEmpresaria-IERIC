import { useRef, useState } from "react";
import { CloudUpload, Paperclip } from "lucide-react";

import "../../styles/components/documentoUploadModal.css";

export default function DocumentoUploadModal({ documentTitle, onClose, onFileSelected }) {
  const inputRef = useRef(null);

  const [arrastrandoArchivo, setArrastrandoArchivo] = useState(false);

  const seleccionarArchivo = (archivo) => {
    if (!archivo) {
      return;
    }

    onFileSelected?.(archivo);
  };

  const handleInputChange = (event) => {
    const archivo = event.target.files?.[0];

    seleccionarArchivo(archivo);
  };

  const handleDragOver = (event) => {
    event.preventDefault();
    setArrastrandoArchivo(true);
  };

  const handleDragLeave = (event) => {
    event.preventDefault();
    setArrastrandoArchivo(false);
  };

  const handleDrop = (event) => {
    event.preventDefault();
    setArrastrandoArchivo(false);

    const archivo = event.dataTransfer.files?.[0];

    seleccionarArchivo(archivo);
  };

  return (
    <div className="documento-upload-overlay" role="presentation" onMouseDown={onClose}>
      <div
        className="documento-upload-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="documento-upload-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <h2 id="documento-upload-title">Deberá adjuntar: {documentTitle}</h2>

        <div className="documento-upload-options">
          <div
            className={["documento-drop-area", arrastrandoArchivo ? "dragging" : ""].filter(Boolean).join(" ")}
            onDragOver={handleDragOver}
            onDragLeave={handleDragLeave}
            onDrop={handleDrop}
          >
            <CloudUpload size={36} aria-hidden="true" />

            <span>Suelte el archivo</span>
          </div>

          <button type="button" className="documento-select-area" onClick={() => inputRef.current?.click()}>
            <Paperclip size={34} aria-hidden="true" />

            <span>Seleccione el archivo</span>
          </button>
        </div>

        <input
          ref={inputRef}
          className="documento-file-input"
          type="file"
          accept=".pdf,application/pdf"
          tabIndex={-1}
          onChange={handleInputChange}
        />
      </div>
    </div>
  );
}
