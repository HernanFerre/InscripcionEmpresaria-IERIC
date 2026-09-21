import { useState } from "react";
import { AlertTriangle, CheckCircle, Info } from "lucide-react";

import "../styles/stepCuit.css";

import { crearQuiz, validarCuit } from "../services/InscripcionService.js";

import { formatCuit } from "../utils/formatters.js";

import SkipValidationButton from "../components/common/SkipValidationButton.jsx"; // Luego sacar

export default function StepCuit({ token, initialCuit = "", initialEmpresa = null, estaLogueado = false, onLoginRequired, onNext }) {
  const [cuit, setCuit] = useState(initialCuit);

  const [empresa, setEmpresa] = useState(initialEmpresa);

  const [estadoSolicitud, setEstadoSolicitud] = useState(initialEmpresa?.estadoSolicitud || "");

  const [validado, setValidado] = useState(Boolean(initialEmpresa));

  const [cargando, setCargando] = useState(false);

  const [preparandoQuiz, setPreparandoQuiz] = useState(false);

  const [errorProceso, setErrorProceso] = useState("");

  const [requiereAutenticacion, setRequiereAutenticacion] = useState(false);

  const handleValidar = async () => {
    if (!estaLogueado) {
      setErrorProceso("");
      setRequiereAutenticacion(true);
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

    setRequiereAutenticacion(false);
    setPreparandoQuiz(true);
    setErrorProceso("");

    try {
      const cuitNormalizado = String(cuit).replace(/\D/g, "");

      const quiz = await crearQuiz(cuitNormalizado, token);

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

  const handleSaltearValidacion = () => {
    const cuitIngresado = String(cuit).replace(/\D/g, "");

    const cuitDemo = cuitIngresado.length === 11 ? cuitIngresado : "30123456789";

    onNext({
      cuit: cuitDemo,
      empresa: {
        razonSocial: "Empresa de demostración",
        estadoSolicitud: "HABILITADA",
      },
      cuiles: [],
      quiz: {
        quizId: "quiz-demostracion",
        titulo: "Información de la empresa",
        consigna: "Seleccione los trabajadores que reconoce como vinculados a la empresa.",
        intentosTotales: 3,
        intentosRestantes: 3,
        opciones: [
          {
            id: "a",
            label: "20-xxxxx458-3",
          },
          {
            id: "b",
            label: "27-xxxxx921-5",
          },
          {
            id: "c",
            label: "23-xxxxx774-1",
          },
          {
            id: "d",
            label: "24-xxxxx662-8",
          },
          {
            id: "ninguna",
            label: "Ninguna de las anteriores",
          },
          {
            id: "todas",
            label: "Todas las anteriores",
          },
        ],
      },
    });
  };

  const esRegistrada = estadoSolicitud === "REGISTRADA";

  const esHabilitada = estadoSolicitud === "HABILITADA";

  const esBloqueada = estadoSolicitud === "BLOQUEADA";

  const tieneRazonSocial = Boolean(empresa?.razonSocial?.trim());

  const cuitTieneOnceNumeros = String(cuit).replace(/\D/g, "").length === 11;

  return (
    <>
      <h1 className="section-title cuit-title">Solicitud de inscripción empresaria</h1>

      <div className="cuit-row">
        <div className="input-wrapper">
          <label className="form-field-label" htmlFor="cuit">
            CUIT de la empresa
            <span aria-hidden="true">*</span>
          </label>

          <div className={`input-with-check ${validado ? "success" : ""}`}>
            <input
              id="cuit"
              type="text"
              value={cuit}
              onChange={handleCuitChange}
              placeholder="Ingrese su CUIT"
              inputMode="numeric"
              autoComplete="off"
              required
            />

            {validado && <CheckCircle size={22} aria-hidden="true" />}
          </div>

          <p className={validado ? "status-ok" : "status-muted"} aria-live="polite">
            {validado ? "CUIT verificado con éxito" : "Pendiente de validación"}
          </p>
        </div>

        <button type="button" onClick={handleValidar} disabled={cargando || !cuitTieneOnceNumeros}>
          {cargando ? "Validando..." : "Validar CUIT"}
        </button>
      </div>

      <SkipValidationButton onClick={handleSaltearValidacion} />

      {requiereAutenticacion && !estaLogueado && (
        <div className="cuit-result-card warning">
          <AlertTriangle size={20} aria-hidden="true" />

          <div>
            <strong>Debe iniciar sesión para continuar.</strong>

            <span>Inicie sesión o cree una cuenta antes de validar el CUIT de la empresa.</span>

            <button type="button" className="next-step-button cuit-auth-button" onClick={onLoginRequired}>
              Iniciar sesión / crear cuenta
            </button>
          </div>
        </div>
      )}

      {errorProceso && (
        <div className="cuit-result-card error" role="alert">
          <AlertTriangle size={20} aria-hidden="true" />

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
              <span className="form-field-label">Razón social</span>

              <div className="readonly-box">{empresa.razonSocial}</div>
            </div>
          )}

          {esRegistrada && (
            <div className="cuit-result-card warning">
              <Info size={20} aria-hidden="true" />

              <div>
                <strong>La empresa ya se encuentra registrada.</strong>

                <span>{empresa?.mensaje || "Debe continuar desde el Portal del Empleador."}</span>
              </div>
            </div>
          )}

          {esHabilitada && (
            <div className="cuit-result-card success">
              <CheckCircle size={20} aria-hidden="true" />

              <div>
                <strong>La empresa se encuentra en condiciones de iniciar la inscripción.</strong>

                <span>Puede continuar con el proceso.</span>
              </div>
            </div>
          )}

          {esBloqueada && (
            <div className="cuit-result-card error">
              <AlertTriangle size={20} aria-hidden="true" />

              <div>
                <strong>No es posible iniciar la inscripción.</strong>

                <span>{empresa?.mensaje || "El estado de la empresa no permite continuar."}</span>
              </div>
            </div>
          )}

          {esHabilitada && (
            <div className="next-step-container">
              <button type="button" className="next-step-button" onClick={handleNext} disabled={preparandoQuiz}>
                {preparandoQuiz ? "Preparando..." : "Continuar"}
              </button>
            </div>
          )}
        </>
      )}
    </>
  );
}
