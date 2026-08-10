import { useState } from "react";
import { Info, X } from "lucide-react";

import "../../styles/components/declaracionJuradaModal.css";

const OPCIONES_DECLARACION = [
  {
    id: "1",
    titulo: "No haber recibido notificación de Inducción/Requerimiento o habérsele labrado Acta de Inspección.",
  },
  {
    id: "2",
    titulo: "Haber sido notificado y encontrarse en la etapa de inducción.",
  },
  {
    id: "3",
    titulo: "Haber sido notificado y encontrarse en la etapa de inspección.",
  },
  {
    id: "4",
    titulo: "Habérsele labrado Acta de Inspección/Infracción.",
  },
];

const DATOS_INICIALES = {
  ciudad: "",
  provincia: "",
  dia: "",
  mes: "",
  anio: "",
  firmante: "",
  caracter: "",
  empresa: "",
  numeroIeric: "",
  cuit: "",
  tipoDocumento: "",
  numeroDocumento: "",
  fechaNotificacion: "",
  numeroRequerimiento: "",
};

export default function DeclaracionJuradaModal({ initialData = null, onClose, onSave }) {
  const [datos, setDatos] = useState({
    ...DATOS_INICIALES,
    ...(initialData ?? {}),
  });

  const [opcionSeleccionada, setOpcionSeleccionada] = useState("");

  const actualizarCampo = (event) => {
    const { name, value } = event.target;

    setDatos((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = (event) => {
    event.preventDefault();

    onSave?.({
      ...datos,
      opcionSeleccionada,
    });
  };

  return (
    <div className="declaracion-modal-overlay" role="presentation" onMouseDown={onClose}>
      <div
        className="declaracion-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="declaracion-modal-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <header className="declaracion-modal-header">
          <h2 id="declaracion-modal-title">Declaración jurada</h2>

          <button type="button" className="declaracion-modal-close" aria-label="Cerrar" onClick={onClose}>
            <X size={20} />
          </button>
        </header>

        <form onSubmit={handleSubmit}>
          <div className="declaracion-modal-body">
            <div className="declaracion-identification">
              <div className="declaracion-identification-line">
                <span>En la Ciudad de</span>

                <input
                  className="declaracion-inline-input ciudad"
                  type="text"
                  name="ciudad"
                  value={datos.ciudad}
                  aria-label="Ciudad"
                  onChange={actualizarCampo}
                />

                <span>, Provincia de</span>

                <input
                  className="declaracion-inline-input provincia"
                  type="text"
                  name="provincia"
                  value={datos.provincia}
                  aria-label="Provincia"
                  onChange={actualizarCampo}
                />

                <span>, a los</span>

                <input
                  className="declaracion-inline-input dia"
                  type="text"
                  name="dia"
                  value={datos.dia}
                  aria-label="Día"
                  onChange={actualizarCampo}
                />

                <span>días del mes de</span>

                <input
                  className="declaracion-inline-input mes"
                  type="text"
                  name="mes"
                  value={datos.mes}
                  aria-label="Mes"
                  onChange={actualizarCampo}
                />
              </div>

              <div className="declaracion-identification-line">
                <span>de</span>

                <input
                  className="declaracion-inline-input anio"
                  type="text"
                  name="anio"
                  value={datos.anio}
                  aria-label="Año"
                  onChange={actualizarCampo}
                />

                <span>, el que suscribe</span>

                <input
                  className="declaracion-inline-input firmante"
                  type="text"
                  name="firmante"
                  value={datos.firmante}
                  aria-label="Firmante"
                  onChange={actualizarCampo}
                />

                <span>, en su carácter de</span>

                <select
                  className="declaracion-inline-input caracter"
                  name="caracter"
                  value={datos.caracter}
                  aria-label="Carácter del firmante"
                  onChange={actualizarCampo}
                >
                  <option value="" disabled>
                    Seleccione
                  </option>

                  <option value="presidente">Presidente</option>

                  <option value="vicepresidente">Vicepresidente</option>

                  <option value="apoderado">Apoderado</option>
                </select>

                <span>,</span>
              </div>

              <div className="declaracion-identification-line">
                <span>de la empresa</span>

                <input
                  className="declaracion-inline-input empresa"
                  type="text"
                  name="empresa"
                  value={datos.empresa}
                  aria-label="Empresa"
                  onChange={actualizarCampo}
                />

                <span>IERIC N°</span>

                <input
                  className="declaracion-inline-input numero-ieric"
                  type="text"
                  name="numeroIeric"
                  value={datos.numeroIeric}
                  aria-label="Número IERIC"
                  onChange={actualizarCampo}
                />

                <span>(en caso que corresponda), CUIT N°</span>
              </div>

              <div className="declaracion-identification-line">
                <input
                  className="declaracion-inline-input cuit"
                  type="text"
                  name="cuit"
                  value={datos.cuit}
                  aria-label="CUIT"
                  onChange={actualizarCampo}
                />

                <span>, quien acredita identidad con Documento Nacional de Identidad: Tipo:</span>

                <input
                  className="declaracion-inline-input tipo-documento"
                  type="text"
                  name="tipoDocumento"
                  value={datos.tipoDocumento}
                  aria-label="Tipo de documento"
                  onChange={actualizarCampo}
                />

                <span>N°:</span>

                <input
                  className="declaracion-inline-input documento"
                  type="text"
                  name="numeroDocumento"
                  value={datos.numeroDocumento}
                  aria-label="Número de documento"
                  onChange={actualizarCampo}
                />

                <span>.</span>
              </div>
            </div>

            <div className="declaracion-section-title">
              <h3>Manifiesta con carácter de declaración jurada</h3>

              <Info size={18} aria-hidden="true" />
            </div>

            <div className="declaracion-options">
              {OPCIONES_DECLARACION.map((opcion) => (
                <div className="declaracion-option-wrapper" key={opcion.id}>
                  <label className={["declaracion-option", opcionSeleccionada === opcion.id ? "selected" : ""].filter(Boolean).join(" ")}>
                    <input
                      type="radio"
                      name="opcion-declaracion"
                      value={opcion.id}
                      checked={opcionSeleccionada === opcion.id}
                      onChange={(event) => setOpcionSeleccionada(event.target.value)}
                    />

                    <span>
                      {opcion.id}. {opcion.titulo}
                    </span>
                  </label>

                  {opcionSeleccionada === opcion.id && (
                    <div className="declaracion-option-detail">
                      <span>Haber sido notificado con fecha</span>

                      <input
                        className="declaracion-detail-input fecha"
                        type="text"
                        name="fechaNotificacion"
                        value={datos.fechaNotificacion}
                        aria-label="Fecha de notificación"
                        onChange={actualizarCampo}
                      />

                      <span>
                        por el sector Fiscalizaciones del Instituto de Estadística y Registro de la Industria de la Construcción de la
                        Inducción a la Regularización/Requerimiento N°
                      </span>

                      <input
                        className="declaracion-detail-input numero"
                        type="text"
                        name="numeroRequerimiento"
                        value={datos.numeroRequerimiento}
                        aria-label="Número de requerimiento"
                        onChange={actualizarCampo}
                      />

                      <span>
                        y encontrarse en la etapa correspondiente dentro del plazo de quince (15) días hábiles previsto para efectivizar el
                        pago de la multa reducida.
                      </span>
                    </div>
                  )}
                </div>
              ))}
            </div>
          </div>

          <footer className="declaracion-modal-footer">
            <button type="button" className="declaracion-modal-cancel" onClick={onClose}>
              Cancelar
            </button>

            <button type="submit" className="declaracion-modal-save">
              Guardar
            </button>
          </footer>
        </form>
      </div>
    </div>
  );
}
