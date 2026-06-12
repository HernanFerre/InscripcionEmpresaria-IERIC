import { useEffect, useState } from "react";
import { AlertTriangle, CheckCircle } from "lucide-react";
import "../styles/stepIdentidad.css";

import { obtenerDesafioInformacion, validarDesafioInformacion } from "../services/inscripcionService.js";

export default function StepIdentidad({ onNext }) {
  const [desafio, setDesafio] = useState(null);
  const [seleccionadas, setSeleccionadas] = useState([]);
  const [intentos, setIntentos] = useState(2);
  const [cargando, setCargando] = useState(true);
  const [validando, setValidando] = useState(false);
  const [informacionValidada, setInformacionValidada] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelado = false;

    obtenerDesafioInformacion()
      .then((data) => {
        if (!cancelado) {
          setDesafio(data);
          setIntentos(data.intentosRestantes ?? 2);
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

  const esSeleccionUnica = desafio?.escenario === "UNICO_EMPLEADO";

  const toggleOpcion = (id) => {
    setError("");

    if (informacionValidada) return;

    if (esSeleccionUnica) {
      setSeleccionadas([id]);
      return;
    }

    setSeleccionadas((prev) => (prev.includes(id) ? prev.filter((item) => item !== id) : [...prev, id]));
  };

  const confirmarInformacion = async () => {
    if (!seleccionadas.length || !desafio) return;

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
      setDesafio({
        escenario: "VALIDACION_MANUAL",
        mensaje:
          "No fue posible validar automáticamente la información ingresada. Deberá continuar el trámite mediante soporte o representación de IERIC.",
      });
      return;
    }

    setError("La selección no coincide con los registros disponibles.");
  };

  if (cargando) {
    return (
      <>
        <h1>INFORMACIÓN</h1>
        <p className="status-muted identity-loading">Consultando base de datos IERIC...</p>
      </>
    );
  }

  if (desafio?.escenario === "VALIDACION_MANUAL") {
    return (
      <>
        <h1>INFORMACIÓN</h1>

        <section className="identity-manual-card">
          <AlertTriangle size={34} />

          <h2>Información pendiente de revisión</h2>

          <p>{desafio.mensaje || "No contamos con información suficiente para validar automáticamente la información."}</p>

          <p>Para continuar, comuníquese con soporte o con una representación de IERIC.</p>

          <strong>Teléfono de contacto: 0800-000-IERIC</strong>
        </section>
      </>
    );
  }

  return (
    <div className="step-content">
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
                    type={esSeleccionUnica ? "radio" : "checkbox"}
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

          {!informacionValidada && (
            <p className="identity-attempts">
              Le quedan <strong>{intentos}</strong> intentos
            </p>
          )}

          {error && <p className="identity-error">{error}</p>}

          {informacionValidada && (
            <p className="identity-success">
              <CheckCircle size={20} />
              Información validada correctamente
            </p>
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
    </div>
  );
}
