import { useState } from "react";
import { AlertTriangle, CheckCircle, Info } from "lucide-react";
import "../styles/stepCuit.css";

import { obtenerCuilesPorCuit, validarCuit } from "../services/inscripcionService.js";

import { formatCuit } from "../utils/formatters.js";

export default function StepCuit({ initialCuit = "", initialEmpresa = null, onNext }) {
  const [cuit, setCuit] = useState(initialCuit);
  const [empresa, setEmpresa] = useState(initialEmpresa);
  const [estadoSolicitud, setEstadoSolicitud] = useState(initialEmpresa?.estadoSolicitud || "");
  const [validado, setValidado] = useState(Boolean(initialEmpresa));
  const [cargando, setCargando] = useState(false);

  const [consultandoCuiles, setConsultandoCuiles] = useState(false);
  const [errorConsultaCuiles, setErrorConsultaCuiles] = useState("");

  const handleValidar = async () => {
    setCargando(true);
    setErrorConsultaCuiles("");

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
      }
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
    setErrorConsultaCuiles("");
  };

  const handleNext = async () => {
    setConsultandoCuiles(true);
    setErrorConsultaCuiles("");

    try {
      const resultado = await obtenerCuilesPorCuit(cuit);

      if (!resultado.ok) {
        setErrorConsultaCuiles(resultado.mensaje || "No fue posible obtener la información de la empresa.");
        return;
      }

      onNext({
        cuit: resultado.cuit,
        empresa: {
          ...empresa,
          estadoSolicitud,
        },
        cuiles: resultado.cuiles,
      });
    } catch {
      setErrorConsultaCuiles("No fue posible consultar los CUIL relacionados con la empresa.");
    } finally {
      setConsultandoCuiles(false);
    }
  };

  const esRegistrada = estadoSolicitud === "REGISTRADA";
  const esHabilitada = estadoSolicitud === "HABILITADA";
  const esBloqueada = estadoSolicitud === "BLOQUEADA";

  const tieneRazonSocial = Boolean(empresa?.razonSocial?.trim());

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

        <button onClick={handleValidar} disabled={cargando || cuit.replace(/\D/g, "").length < 5}>
          {cargando ? "Validando..." : "Validar CUIT"}
        </button>
      </div>

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
                <span>Sera redirigido al portal del empleador.</span>
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
                <strong>No es posible completar la solicitud, debe registrar al menos un trabajador para continuar.</strong>
                <span>Comuniquese con un representante del IERIC al 0800-111-IERIC.</span>
              </div>
            </div>
          )}

          {errorConsultaCuiles && (
            <div className="cuit-result-card error">
              <AlertTriangle size={20} />
              <div>
                <strong>No fue posible continuar.</strong>
                <span>{errorConsultaCuiles}</span>
              </div>
            </div>
          )}

          <div className={`next-step-container ${esHabilitada ? "" : "align-left"}`}>
            {esRegistrada && (
              <button className="next-step-button" type="button">
                Continuar
              </button>
            )}

            {esHabilitada && (
              <button className="next-step-button" onClick={handleNext} type="button" disabled={consultandoCuiles}>
                {consultandoCuiles ? "Consultando..." : "Continuar"}
              </button>
            )}

            {esBloqueada && (
              <button className="next-step-button" type="button">
                Continuar
              </button>
            )}
          </div>
        </>
      )}
    </>
  );
}
