import { useState } from "react";
import { AlertTriangle, CheckCircle, Info } from "lucide-react";

import "../styles/stepCuit.css";

import { crearQuiz, validarCuit } from "../services/InscripcionService.js";

import { formatCuit } from "../utils/formatters.js";

import { obtenerEstadoBloqueo } from "../services/QuizBlockingMockService.js";

export default function StepCuit({ initialCuit = "", initialEmpresa = null, estaLogueado = false, onLoginRequired, onNext }) {
  const [cuit, setCuit] = useState(initialCuit);

  const [empresa, setEmpresa] = useState(initialEmpresa);

  const [estadoSolicitud, setEstadoSolicitud] = useState(initialEmpresa?.estadoSolicitud || "");

  const [validado, setValidado] = useState(Boolean(initialEmpresa));

  const [cargando, setCargando] = useState(false);

  const [preparandoQuiz, setPreparandoQuiz] = useState(false);

  const [errorProceso, setErrorProceso] = useState("");

  const [requiereAutenticacion, setRequiereAutenticacion] = useState(false);

  const puedeContinuarPorBloqueo = () => {
    const estadoBloqueo = obtenerEstadoBloqueo(cuit);

    if (!estadoBloqueo.bloqueado) {
      return true;
    }

    setRequiereAutenticacion(false);

    if (estadoBloqueo.permanente) {
      setErrorProceso("El CUIT se encuentra bloqueado permanentemente por haber agotado los intentos del quiz.");

      return false;
    }

    setErrorProceso(
      `El CUIT se encuentra temporalmente bloqueado. ` +
        `Podrá volver a intentarlo en aproximadamente ` +
        `${estadoBloqueo.segundosRestantes} segundos.`,
    );

    return false;
  };

  const handleValidar = async () => {
    if (!estaLogueado) {
      setErrorProceso("");
      setRequiereAutenticacion(true);
      return;
    }

    if (!puedeContinuarPorBloqueo()) {
      return;
    }

    setRequiereAutenticacion(false);
    setCargando(true);
    setErrorProceso("");

    try {
      const resultado = await validarCuit(cuit);

      if (resultado.ok) {
        setEmpresa(resultado.empresa);

        setEstadoSolicitud(resultado.estadoSolicitud);

        setValidado(true);
      } else {
        setEmpresa(null);

        setEstadoSolicitud(resultado.estadoSolicitud || "NO_ENCONTRADA");

        setValidado(false);

        setErrorProceso(resultado.mensaje || "No fue posible validar el CUIT.");
      }
    } catch (error) {
      setEmpresa(null);
      setEstadoSolicitud("");
      setValidado(false);

      setErrorProceso(error.message || "No fue posible consultar el estado de la empresa.");
    } finally {
      setCargando(false);
    }
  };

  const handleCuitChange = (event) => {
    const formatted = formatCuit(event.target.value);

    setCuit(formatted);
    setValidado(false);
    setEmpresa(null);
    setEstadoSolicitud("");
    setErrorProceso("");
  };

  const handleNext = async () => {
    if (!estaLogueado) {
      setErrorProceso("");
      setRequiereAutenticacion(true);
      return;
    }

    if (!puedeContinuarPorBloqueo()) {
      return;
    }

    setRequiereAutenticacion(false);
    setPreparandoQuiz(true);
    setErrorProceso("");

    try {
      const cuitNormalizado = String(cuit).replace(/\D/g, "");

      const quiz = await crearQuiz(cuitNormalizado);

      onNext({
        cuit: cuitNormalizado,

        empresa: {
          ...empresa,
          estadoSolicitud,
        },

        cuiles: [],

        quiz,
      });
    } catch (error) {
      setErrorProceso(error.message || "No fue posible preparar la validación de información.");
    } finally {
      setPreparandoQuiz(false);
    }
  };

  const esRegistrada = estadoSolicitud === "REGISTRADA";

  const esHabilitada = estadoSolicitud === "HABILITADA";

  const esBloqueada = estadoSolicitud === "BLOQUEADA";

  const tieneRazonSocial = Boolean(empresa?.razonSocial?.trim());

  const cuitTieneOnceNumeros = String(cuit).replace(/\D/g, "").length === 11;

  return (
    <>
      <h1>SOLICITUD DE INSCRIPCIÓN EMPRESARIA</h1>

      <div className="cuit-row">
        <div className="input-wrapper">
          <label htmlFor="cuit">CUIT de la Empresa</label>

          <div className={`input-with-check ${validado ? "success" : ""}`}>
            <input
              id="cuit"
              value={cuit}
              onChange={handleCuitChange}
              placeholder="Ingrese su CUIT"
              inputMode="numeric"
              autoComplete="off"
            />

            {validado && <CheckCircle size={22} />}
          </div>

          <p className={validado ? "status-ok" : "status-muted"}>{validado ? "CUIT verificado con éxito" : "Pendiente de validación"}</p>
        </div>

        <button onClick={handleValidar} disabled={cargando || !cuitTieneOnceNumeros} type="button">
          {cargando ? "Validando..." : "Validar CUIT"}
        </button>
      </div>

      {requiereAutenticacion && !estaLogueado && (
        <div className="cuit-result-card warning">
          <AlertTriangle size={20} />

          <div>
            <strong>Debe iniciar sesión para continuar.</strong>

            <span>Inicie sesión o cree una cuenta antes de validar el CUIT de la empresa.</span>

            <button type="button" className="next-step-button cuit-auth-button" onClick={onLoginRequired}>
              Iniciar sesión / Crear cuenta
            </button>
          </div>
        </div>
      )}

      {errorProceso && (
        <div className="cuit-result-card error">
          <AlertTriangle size={20} />

          <div>
            <strong>No fue posible continuar.</strong>

            <span>{errorProceso}</span>
          </div>
        </div>
      )}

      {validado && (
        <>
          {tieneRazonSocial && (
            <div className="readonly-group">
              <label>Razón Social</label>

              <div className="readonly-box">{empresa.razonSocial}</div>
            </div>
          )}

          {esRegistrada && (
            <div className="cuit-result-card warning">
              <Info size={20} />

              <div>
                <strong>La empresa ya se encuentra registrada.</strong>

                <span>{empresa?.mensaje || "Debe continuar desde el Portal del Empleador."}</span>
              </div>
            </div>
          )}

          {esHabilitada && (
            <div className="cuit-result-card success">
              <CheckCircle size={20} />

              <div>
                <strong>La empresa se encuentra en condiciones de iniciar la inscripción.</strong>

                <span>Puede continuar con el proceso.</span>
              </div>
            </div>
          )}

          {esBloqueada && (
            <div className="cuit-result-card error">
              <AlertTriangle size={20} />

              <div>
                <strong>No es posible iniciar la inscripción.</strong>

                <span>{empresa?.mensaje || "El estado de la empresa no permite continuar."}</span>
              </div>
            </div>
          )}

          {esHabilitada && (
            <div className="next-step-container">
              <button className="next-step-button" onClick={handleNext} type="button" disabled={preparandoQuiz}>
                {preparandoQuiz ? "Preparando..." : "Continuar"}
              </button>
            </div>
          )}
        </>
      )}
    </>
  );
}
