import { useState } from "react";
import { CheckCircle, Info } from "lucide-react";
import "../styles/stepCuit.css";
import { formatCuit } from "../utils/formatters.js";

export default function StepCuit({ initialCuit = "", initialEmpresa = null, onNext }) {
  const [cuit, setCuit] = useState(initialCuit);
  const [empresa, setEmpresa] = useState(initialEmpresa);
  const [validado, setValidado] = useState(Boolean(initialEmpresa));
  const [cargando, setCargando] = useState(false);

  const validarCuitDummy = (cuitFormateado) => {
    const soloNumeros = cuitFormateado.replace(/\D/g, "");

    if (soloNumeros.startsWith("300000")) {
      return {
        ok: true,
        registrada: true,
        empresa: {
          razonSocial: "CONSTRUCTORA REGISTRADA S.A.",
          estado: "CUIT válido",
        },
      };
    }

    if (soloNumeros.startsWith("30111")) {
      return {
        ok: true,
        registrada: false,
        empresa: {
          razonSocial: "EMPRESA NUEVA S.R.L.",
          estado: "CUIT válido",
        },
      };
    }

    return {
      ok: false,
      registrada: null,
      empresa: null,
    };
  };

  const handleValidar = async () => {
    setCargando(true);

    setTimeout(() => {
      const resultado = validarCuitDummy(cuit);

      if (resultado.ok) {
        setEmpresa({
          ...resultado.empresa,
          registrada: resultado.registrada,
        });
        setValidado(true);
      } else {
        setEmpresa(null);
        setValidado(false);
      }

      setCargando(false);
    }, 600);
  };

  const handleCuitChange = (event) => {
    const formatted = formatCuit(event.target.value);

    setCuit(formatted);
    setValidado(false);
    setEmpresa(null);
  };

  const handleNext = () => {
    onNext({
      cuit,
      empresa,
    });
  };

  const empresaRegistrada = validado && empresa?.registrada === true;
  const empresaNoRegistrada = validado && empresa?.registrada === false;

  return (
    <div className="step-content">
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
            <div className="readonly-group">
              <label>Razón Social</label>
              <div className="readonly-box">{empresa?.razonSocial || ""}</div>
            </div>

            <div className="readonly-group">
              <label>Validación IERIC</label>
              <div className="readonly-box">{empresa?.estado || ""}</div>
            </div>

            {empresaRegistrada && (
              <div className="cuit-result-card warning">
                <Info size={20} />
                <div>
                  <strong>La empresa ya se encuentra registrada.</strong>
                  <span>sera dirigido al portal del Empleador.</span>
                </div>
              </div>
            )}

            {empresaNoRegistrada && (
              <div className="cuit-result-card success">
                <CheckCircle size={20} />
                <div>
                  <strong>La empresa se encuentra en condiciones de continuar con el proceso de inscripción.</strong>
                  {/* <span>Puede continuar.</span> */}
                </div>
              </div>
            )}

            <div className={`next-step-container ${empresaRegistrada ? "align-left" : ""}`}>
              {empresaRegistrada && (
                <button className="next-step-button" type="button">
                  Continuar
                </button>
              )}

              {empresaNoRegistrada && (
                <button className="next-step-button" onClick={handleNext} type="button">
                  Continuar
                </button>
              )}
            </div>
          </>
        )}
      </>
    </div>
  );
}
