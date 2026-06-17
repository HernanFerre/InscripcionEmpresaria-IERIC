import { useEffect, useState } from "react";
import { CheckCircle, Phone } from "lucide-react";
import "../styles/stepContacto.css";

export default function StepContacto({ initialContacto, onNext }) {
  const [telefono, setTelefono] = useState(initialContacto?.telefono || "");
  const [codigo, setCodigo] = useState("");

  const [codigoEnviado, setCodigoEnviado] = useState(false);
  const [telefonoValidado, setTelefonoValidado] = useState(Boolean(initialContacto?.telefonoValidado));

  const [intentosRestantes, setIntentosRestantes] = useState(3);
  const [errorCodigo, setErrorCodigo] = useState("");
  const [bloqueado, setBloqueado] = useState(false);
  const [segundosRestantes, setSegundosRestantes] = useState(0);

  const telefonoEsValido = telefono.length >= 8;
  const codigoEsValido = codigo.length > 0;

  useEffect(() => {
    if (!bloqueado || segundosRestantes <= 0) return;

    const timer = setTimeout(() => {
      setSegundosRestantes((prev) => prev - 1);
    }, 1000);

    return () => clearTimeout(timer);
  }, [bloqueado, segundosRestantes]);

  useEffect(() => {
    if (!bloqueado || segundosRestantes !== 0) return;

    const desbloquearValidacion = () => {
      setBloqueado(false);
      setIntentosRestantes(3);
      setCodigo("");
      setErrorCodigo("");
    };

    desbloquearValidacion();
  }, [bloqueado, segundosRestantes]);

  const handleTelefonoChange = (event) => {
    const soloNumeros = event.target.value.replace(/\D/g, "");

    setTelefono(soloNumeros);
    setCodigo("");
    setCodigoEnviado(false);
    setTelefonoValidado(false);
    setIntentosRestantes(3);
    setErrorCodigo("");
    setBloqueado(false);
    setSegundosRestantes(0);
  };

  const handleEnviarCodigo = () => {
    if (!telefonoEsValido) return;

    setCodigoEnviado(true);
    setCodigo("");
    setIntentosRestantes(3);
    setErrorCodigo("");
    setBloqueado(false);
    setSegundosRestantes(0);
  };

  const handleValidarCodigo = () => {
    if (bloqueado || telefonoValidado) return;

    if (codigo === "123456") {
      setTelefonoValidado(true);
      setErrorCodigo("");
      setBloqueado(false);
      setSegundosRestantes(0);
      return;
    }

    const nuevosIntentos = intentosRestantes - 1;

    setIntentosRestantes(nuevosIntentos);
    setCodigo("");

    if (nuevosIntentos <= 0) {
      setBloqueado(true);
      setSegundosRestantes(40);
      setErrorCodigo("Se alcanzó el límite de intentos para validar el teléfono.");
      return;
    }

    setErrorCodigo(`Código incorrecto. Le quedan ${nuevosIntentos} intentos.`);
  };

  const handleNext = () => {
    onNext({
      telefono,
    });
  };

  return (
    <div className="step-content">
      <h1>VALIDACIÓN DE TELÉFONO</h1>

      <section className={`contact-validation-card ${telefonoValidado ? "validated" : ""}`}>
        <div className="contact-section-title">
          <h2>Teléfono de contacto</h2>
        </div>

        <div className={`contact-input-row ${telefonoValidado ? "success" : ""}`}>
          <span className="contact-input-icon">
            <Phone size={18} />
          </span>

          <input
            type="text"
            value={telefono}
            onChange={handleTelefonoChange}
            placeholder="Ingrese su teléfono"
            disabled={telefonoValidado}
            inputMode="numeric"
            autoComplete="off"
          />
        </div>

        {!telefonoValidado && (
          <button className="contact-action-button" onClick={handleEnviarCodigo} disabled={!telefonoEsValido || bloqueado}>
            Enviar código
          </button>
        )}

        <div className="contact-token-row">
          <input
            value={codigo}
            onChange={(event) => setCodigo(event.target.value.replace(/\D/g, ""))}
            placeholder="Ingrese el código recibido"
            autoComplete="off"
            disabled={!codigoEnviado || telefonoValidado || bloqueado}
          />

          <button onClick={handleValidarCodigo} disabled={!codigoEnviado || !codigoEsValido || telefonoValidado || bloqueado}>
            Validar código
          </button>
        </div>

        {codigoEnviado && !telefonoValidado && !bloqueado && (
          <div className="telefono-info-message">Se envió un código de validación al teléfono informado.</div>
        )}

        {codigoEnviado && !telefonoValidado && !bloqueado && <p className="telefono-attempts">Tiene {intentosRestantes} intentos</p>}

        {errorCodigo && (
          <div className={bloqueado ? "telefono-error-blocked" : "telefono-error-message"}>
            <span>{errorCodigo}</span>

            {bloqueado && segundosRestantes > 0 && <span>Podrá volver a intentar en {segundosRestantes} segundos.</span>}
          </div>
        )}

        {telefonoValidado && (
          <div className="telefono-success-message">
            <CheckCircle size={20} />
            <span>Teléfono validado correctamente.</span>
          </div>
        )}
      </section>

      {telefonoValidado && (
        <div className="next-step-container">
          <button className="next-step-button" onClick={handleNext}>
            Continuar
          </button>
        </div>
      )}
    </div>
  );
}
