import { useState } from "react";
import { AlertTriangle, CheckCircle } from "lucide-react";

import "../styles/stepIdentidad.css";

import { validarQuiz } from "../services/InscripcionService.js";

import SkipValidationButton from "../components/common/SkipValidationButton.jsx"; // Luego sacar

function formatearTitulo(titulo) {
  const texto = String(titulo ?? "").trim();

  if (!texto) {
    return "Información de la empresa";
  }

  return texto.charAt(0).toLocaleUpperCase("es-AR") + texto.slice(1).toLocaleLowerCase("es-AR");
}

export default function StepIdentidad({ token, initialQuiz, onNext }) {
  const [desafio, setDesafio] = useState(initialQuiz);

  const [seleccionadas, setSeleccionadas] = useState([]);

  const [intentos, setIntentos] = useState(initialQuiz?.intentosRestantes ?? 3);

  const [intentosTotales, setIntentosTotales] = useState(initialQuiz?.intentosTotales ?? 3);

  const [validando, setValidando] = useState(false);

  const [informacionValidada, setInformacionValidada] = useState(false);

  const [limiteExcedido, setLimiteExcedido] = useState(false);

  const [error, setError] = useState("");

  const toggleOpcion = (id) => {
    setError("");

    if (informacionValidada || limiteExcedido || validando) {
      return;
    }

    const esOpcionExclusiva = id === "ninguna" || id === "todas";

    setSeleccionadas((prev) => {
      if (esOpcionExclusiva) {
        return prev.includes(id) ? [] : [id];
      }

      const seleccionSinExclusivas = prev.filter((opcionId) => opcionId !== "ninguna" && opcionId !== "todas");

      if (seleccionSinExclusivas.includes(id)) {
        return seleccionSinExclusivas.filter((opcionId) => opcionId !== id);
      }

      return [...seleccionSinExclusivas, id];
    });
  };

  const confirmarInformacion = async () => {
    if (!seleccionadas.length || !desafio || limiteExcedido || validando) {
      return;
    }

    setValidando(true);
    setError("");

    try {
      const resultado = await validarQuiz(desafio.quizId, seleccionadas, token);

      setIntentos(resultado.intentosRestantes);

      if (resultado.ok) {
        setInformacionValidada(true);
        setSeleccionadas([]);
        return;
      }

      if (resultado.limiteExcedido) {
        setLimiteExcedido(true);
        setSeleccionadas([]);
        return;
      }

      if (resultado.nuevoQuiz) {
        setDesafio(resultado.nuevoQuiz);
        setIntentos(resultado.nuevoQuiz.intentosRestantes);
        setIntentosTotales(resultado.nuevoQuiz.intentosTotales);
        setSeleccionadas([]);

        setError(`${resultado.mensaje} Le quedan ${resultado.intentosRestantes} intentos.`);

        return;
      }

      setError(resultado.mensaje || "No fue posible validar la información.");
    } catch (errorValidacion) {
      setError(errorValidacion.message || "No fue posible comunicarse con el servidor.");
    } finally {
      setValidando(false);
    }
  };

  if (!desafio) {
    return (
      <>
        <h1 className="section-title identity-title">Información de la empresa</h1>

        <section className="identity-card">
          <div className="identity-error-message" role="alert">
            <AlertTriangle size={18} aria-hidden="true" />

            <span>No fue posible generar la validación de información.</span>
          </div>
        </section>
      </>
    );
  }

  if (limiteExcedido) {
    return (
      <>
        <h1 className="section-title identity-title">No fue posible validar la información</h1>

        <section className="identity-limit-card">
          <p className="identity-limit-intro">
            El sistema de seguridad institucional ha detectado múltiples inconsistencias durante el proceso de verificación.
          </p>

          <div className="identity-warning-message" role="alert">
            <AlertTriangle size={22} aria-hidden="true" />

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

            <button type="button" className="identity-secondary-button" onClick={() => window.location.reload()}>
              Volver al inicio
            </button>
          </div>
        </section>
      </>
    );
  }

  return (
    <>
      <h1 className="section-title identity-title">{formatearTitulo(desafio.titulo)}</h1>

      <section className={`identity-card ${informacionValidada ? "validated" : ""}`}>
        <p className="identity-instruction">{desafio.consigna}</p>

        <div className="identity-options" role="group" aria-label="Opciones de validación">
          {desafio.opciones.map((opcion) => {
            const checked = seleccionadas.includes(opcion.id);

            return (
              <label key={opcion.id} className={`identity-option ${checked ? "selected" : ""}`}>
                <input
                  type="checkbox"
                  name="identity-option"
                  checked={checked}
                  disabled={informacionValidada || validando}
                  onChange={() => toggleOpcion(opcion.id)}
                />

                <span>{opcion.label}</span>
              </label>
            );
          })}
        </div>

        {!informacionValidada && !error && (
          <p className="identity-attempts" aria-live="polite">
            Tiene <strong>{intentos}</strong> de <strong>{intentosTotales}</strong> intentos disponibles
          </p>
        )}

        {!informacionValidada && error && (
          <div className="identity-error-message" role="alert">
            <AlertTriangle size={18} aria-hidden="true" />

            <span>{error}</span>
          </div>
        )}

        {informacionValidada && (
          <div className="identity-success-message" role="status">
            <CheckCircle size={20} aria-hidden="true" />

            <span>Información de la empresa validada correctamente</span>
          </div>
        )}

        {!informacionValidada && (
          <button
            type="button"
            className="identity-confirm-button"
            onClick={confirmarInformacion}
            disabled={!seleccionadas.length || validando}
          >
            {validando ? "Validando..." : "Confirmar información"}
          </button>
        )}

        {!informacionValidada && <SkipValidationButton onClick={onNext} />}
      </section>

      {informacionValidada && (
        <div className="next-step-container">
          <button type="button" className="next-step-button" onClick={onNext}>
            Continuar
          </button>
        </div>
      )}
    </>
  );
}
