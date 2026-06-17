import { useEffect, useState } from "react";
import { AlertTriangle, CheckCircle } from "lucide-react";
import "../styles/stepIdentidad.css";

import { obtenerDesafioInformacion, validarDesafioInformacion } from "../services/inscripcionService.js";

export default function StepIdentidad({ onNext }) {
  const [desafio, setDesafio] = useState(null);
  const [seleccionadas, setSeleccionadas] = useState([]);
  const [intentos, setIntentos] = useState(3);
  const [intentosTotales, setIntentosTotales] = useState(3);
  const [cargando, setCargando] = useState(true);
  const [validando, setValidando] = useState(false);
  const [informacionValidada, setInformacionValidada] = useState(false);
  const [limiteExcedido, setLimiteExcedido] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelado = false;

    obtenerDesafioInformacion()
      .then((data) => {
        if (!cancelado) {
          setDesafio(data);
          setIntentos(data.intentosRestantes ?? 3);
          setIntentosTotales(data.intentosTotales ?? 3);
        }
      })
      .finally(() => {
        if (!cancelado) {
          setCargando(false);
        }
      });

    return () => {
      cancelado = true;
    };
  }, []);

  const toggleOpcion = (id) => {
    setError("");

    if (informacionValidada || limiteExcedido) return;

    setSeleccionadas((prev) => (prev.includes(id) ? prev.filter((item) => item !== id) : [...prev, id]));
  };

  const confirmarInformacion = async () => {
    if (!seleccionadas.length || !desafio || limiteExcedido) return;

    setValidando(true);

    const esCorrecto = await validarDesafioInformacion(seleccionadas, desafio.respuestasCorrectas);

    setValidando(false);

    if (esCorrecto) {
      setInformacionValidada(true);
      setError("");
      return;
    }

    const nuevosIntentos = intentos - 1;

    setIntentos(nuevosIntentos);
    setSeleccionadas([]);

    if (nuevosIntentos <= 0) {
      setLimiteExcedido(true);
      setError("");
      return;
    }

    setError(`Respuesta incorrecta. Le quedan ${nuevosIntentos} intentos para completar la validación.`);
  };

  if (cargando) {
    return (
      <>
        <h1>INFORMACIÓN DE LA EMPRESA</h1>
        <p className="status-muted identity-loading">Consultando base de datos IERIC...</p>
      </>
    );
  }

  if (limiteExcedido) {
    return (
      <>
        <h1>No fue posible validar la información</h1>

        <section className="identity-limit-card">
          <p className="identity-limit-intro">
            El sistema de seguridad institucional ha detectado múltiples inconsistencias durante el proceso de verificación.
          </p>

          <div className="identity-warning-message">
            <AlertTriangle size={22} />
            <div>
              <strong>Límite de intentos excedido</strong>
              <span>
                Ha alcanzado el límite máximo de intentos permitidos para esta validación. Para continuar con el proceso deberá comunicarse
                con un representante del IERIC.
              </span>
            </div>
          </div>

          <div className="identity-limit-actions">
            <button type="button" className="next-step-button">
              Contactar a IERIC
            </button>

            <button type="button" className="identity-secondary-button">
              Volver al Inicio
            </button>
          </div>
        </section>
      </>
    );
  }

  return (
    <>
      <h1>{desafio.titulo}</h1>

      <section className={`identity-card ${informacionValidada ? "validated" : ""}`}>
        <p className="identity-instruction">{desafio.consigna}</p>

        <div className="identity-options">
          {desafio.opciones.map((opcion) => {
            const checked = seleccionadas.includes(opcion.id);

            return (
              <label key={opcion.id} className={`identity-option ${checked ? "selected" : ""}`}>
                <input
                  type="checkbox"
                  name="identity-option"
                  checked={checked}
                  disabled={informacionValidada}
                  onChange={() => toggleOpcion(opcion.id)}
                />

                <span>{opcion.label}</span>
              </label>
            );
          })}
        </div>

        {!informacionValidada && !error && (
          <p className="identity-attempts">
            Tiene <strong>{intentosTotales}</strong> intentos
          </p>
        )}

        {!informacionValidada && error && (
          <div className="identity-error-message">
            <AlertTriangle size={18} />
            <span>{error}</span>
          </div>
        )}

        {informacionValidada && (
          <div className="identity-success-message">
            <CheckCircle size={20} />
            <span>Información de la empresa validada correctamente</span>
          </div>
        )}

        {!informacionValidada && (
          <button className="identity-confirm-button" onClick={confirmarInformacion} disabled={!seleccionadas.length || validando}>
            {validando ? "Validando..." : "Confirmar información"}
          </button>
        )}
      </section>

      {informacionValidada && (
        <div className="next-step-container">
          <button className="next-step-button" onClick={onNext}>
            Finalizar
          </button>
        </div>
      )}
    </>
  );
}
