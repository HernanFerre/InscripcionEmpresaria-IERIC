import { useState } from "react";
import { Info } from "lucide-react";

import "../../styles/stepDeclaracionJurada.css";

const DATOS_INICIALES = {
  opcion: "etapa-inspeccion",
  fechaNotificacion: "",
  numeroRequerimiento: "",
  motivoCargaManual: "",
};

export default function StepDeclaracionJurada({ onBack, onNext }) {
  const [datos, setDatos] = useState(DATOS_INICIALES);

  const actualizarValor = (campo, value) => {
    setDatos((prev) => ({
      ...prev,
      [campo]: value,
    }));
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    onNext?.();
  };

  return (
    <form className="declaracion-step" onSubmit={handleSubmit}>
      <div className="declaracion-heading">
        <h2>Manifiesta con carácter de declaración jurada:</h2>

        <Info size={19} aria-hidden="true" />
      </div>

      <p className="declaracion-introduction">
        Que habiendo vencido el plazo de 45 días previstos para la presentación voluntaria tardía establecida en el art. 33, incs. b) y c)
        de la Ley 22.250, reglamentada por el art. 17° Dec. 1342/81, se presenta a regularizar la/s infracción/es a/la/los arts. 3° y/o 13°
        de la Ley 22.250 respecto a los trabajadores que se detallan en el anexo, y a efectos de obtener la quita en la multa de acuerdo a
        las etapas previstas en el artículo 3° de las Resoluciones Conjuntas N° 4/2008-12/2008 y 634/2008-14/2008 del MTEySS/IERIC y su
        Reglamentación declara:
      </p>

      <div className="declaracion-options">
        <label className={`declaracion-option ${datos.opcion === "sin-notificacion" ? "selected" : ""}`}>
          <input
            type="radio"
            name="declaracion-jurada"
            value="sin-notificacion"
            checked={datos.opcion === "sin-notificacion"}
            onChange={(event) => actualizarValor("opcion", event.target.value)}
          />

          <span>
            <strong>1.</strong> No haber recibido notificación de Inducción/Requerimiento o habérsele labrado Acta de Inspección.
          </span>
        </label>

        <label className={`declaracion-option ${datos.opcion === "etapa-induccion" ? "selected" : ""}`}>
          <input
            type="radio"
            name="declaracion-jurada"
            value="etapa-induccion"
            checked={datos.opcion === "etapa-induccion"}
            onChange={(event) => actualizarValor("opcion", event.target.value)}
          />

          <span>
            <strong>2.</strong> Haber sido notificado y encontrarse en la etapa de inducción.
          </span>
        </label>

        <label className={`declaracion-option ${datos.opcion === "etapa-inspeccion" ? "selected declaracion-option-detailed" : ""}`}>
          <input
            type="radio"
            name="declaracion-jurada"
            value="etapa-inspeccion"
            checked={datos.opcion === "etapa-inspeccion"}
            onChange={(event) => actualizarValor("opcion", event.target.value)}
          />

          {datos.opcion === "etapa-inspeccion" ? (
            <span>
              <strong>3.</strong> Haber sido notificado con fecha{" "}
              <input
                className="declaracion-inline-input declaracion-date-input"
                type="date"
                value={datos.fechaNotificacion}
                aria-label="Fecha de notificación"
                onChange={(event) => actualizarValor("fechaNotificacion", event.target.value)}
                onClick={(event) => event.stopPropagation()}
              />{" "}
              por el sector Fiscalizaciones del Instituto de Estadística y Registro de la Industria de la Construcción de la Inducción a la
              Regularización/Requerimiento N°{" "}
              <input
                className="declaracion-inline-input declaracion-number-input"
                type="text"
                inputMode="numeric"
                value={datos.numeroRequerimiento}
                placeholder="Número"
                aria-label="Número de requerimiento"
                onChange={(event) => actualizarValor("numeroRequerimiento", event.target.value.replace(/\D/g, ""))}
                onClick={(event) => event.stopPropagation()}
              />{" "}
              y <strong>encontrarse en la etapa de inspección</strong> dentro del plazo de quince (15) días hábiles previsto para
              efectivizar el pago de la multa reducida (art. 3°, inc. 2° de las Resoluciones Conjuntas citadas y art. 2°, inc. 2° de su
              Reglamentación).
            </span>
          ) : (
            <span>
              <strong>3.</strong> Haber sido notificado y encontrarse en la etapa de inspección.
            </span>
          )}
        </label>

        <label className={`declaracion-option ${datos.opcion === "acta-inspeccion" ? "selected" : ""}`}>
          <input
            type="radio"
            name="declaracion-jurada"
            value="acta-inspeccion"
            checked={datos.opcion === "acta-inspeccion"}
            onChange={(event) => actualizarValor("opcion", event.target.value)}
          />

          <span>
            <strong>4.</strong> Habérsele labrado Acta de Inspección/Infracción.
          </span>
        </label>
      </div>

      {datos.opcion !== "etapa-inspeccion" && (
        <section className="declaracion-manual-reason">
          <div className="declaracion-heading">
            <h2>Informe el motivo de la carga manual:</h2>

            <Info size={19} aria-hidden="true" />
          </div>

          <textarea
            value={datos.motivoCargaManual}
            placeholder="Ingrese el motivo"
            aria-label="Motivo de la carga manual"
            onChange={(event) => actualizarValor("motivoCargaManual", event.target.value)}
          />
        </section>
      )}

      <div className="declaracion-footer-actions">
        <button type="button" className="declaracion-back-button" onClick={onBack}>
          Volver
        </button>

        <button type="submit" className="next-step-button">
          Continuar
        </button>
      </div>
    </form>
  );
}
