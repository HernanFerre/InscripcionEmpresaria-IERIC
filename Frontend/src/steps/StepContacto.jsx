import { useState } from "react";
import { CheckCircle, Phone } from "lucide-react";
import "../styles/stepContacto.css";

export default function StepContacto({ initialContacto, onNext }) {
  const [telefono, setTelefono] = useState(initialContacto?.telefono || "");
  const [codigo, setCodigo] = useState("");

  const [codigoEnviado, setCodigoEnviado] = useState(false);
  const [telefonoValidado, setTelefonoValidado] = useState(Boolean(initialContacto?.telefonoValidado));

  const telefonoEsValido = telefono.length >= 8;
  const codigoEsValido = codigo.length > 0;

  const handleTelefonoChange = (event) => {
    const soloNumeros = event.target.value.replace(/\D/g, "");

    setTelefono(soloNumeros);
    setCodigo("");
    setCodigoEnviado(false);
    setTelefonoValidado(false);
  };

  const handleEnviarCodigo = () => {
    if (!telefonoEsValido) return;
    setCodigoEnviado(true);
  };

  const handleValidarCodigo = () => {
    if (codigo === "123456") {
      setTelefonoValidado(true);
    }
  };

  const handleNext = () => {
    onNext({
      telefono,
    });
  };

  return (
    <div className="step-content">
      <>
        <h1>VALIDACIÓN DE TELÉFONO</h1>

        <section className={`contact-validation-card ${telefonoValidado ? "validated" : ""}`}>
          <div className="contact-section-title">
            <h2>Teléfono de contacto</h2>

            {telefonoValidado && (
              <span className="verified-pill">
                <CheckCircle size={18} />
                Teléfono validado
              </span>
            )}
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
            <button className="contact-action-button" onClick={handleEnviarCodigo} disabled={!telefonoEsValido}>
              Enviar código
            </button>
          )}

          <div className="contact-token-row">
            <input
              value={codigo}
              onChange={(event) => setCodigo(event.target.value.replace(/\D/g, ""))}
              placeholder="Ingrese el código recibido"
              autoComplete="off"
              disabled={!codigoEnviado || telefonoValidado}
            />

            <button onClick={handleValidarCodigo} disabled={!codigoEnviado || !codigoEsValido || telefonoValidado}>
              Validar código
            </button>
          </div>

          {codigoEnviado && !telefonoValidado && (
            <p className="status-muted contact-helper">Código enviado. Para esta maqueta usar: 123456</p>
          )}
        </section>

        {telefonoValidado && (
          <div className="next-step-container">
            <button className="next-step-button" onClick={handleNext}>
              Continuar
            </button>
          </div>
        )}
      </>
    </div>
  );
}
