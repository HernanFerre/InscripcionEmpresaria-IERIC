import { useState } from "react";

import Topbar from "../components/layout/Topbar.jsx";
import Footer from "../components/layout/Footer.jsx";
import StepIndicator from "../components/wizard/StepIndicator.jsx";

import StepCuit from "../steps/StepCuit.jsx";
import StepContacto from "../steps/StepContacto.jsx";
import StepIdentidad from "../steps/StepIdentidad.jsx";

import IericAuth from "../auth/IericAuth.jsx";

import { MOSTRAR_VALIDACION_TELEFONO } from "../config/featureFlags.js";

import "../styles/inscripcion.css";

export default function InscripcionPage() {
  const [currentStep, setCurrentStep] = useState(1);

  const [formData, setFormData] = useState({
    cuit: "",
    empresa: null,
    cuiles: [],
    contacto: {
      email: "",
      telefono: "",
      emailValidado: false,
      telefonoValidado: false,
    },
  });

  const handleCuitCompletado = ({ cuit, empresa, cuiles = [] }) => {
    setFormData((prev) => ({
      ...prev,
      cuit,
      empresa,
      cuiles,
    }));

    setCurrentStep(2);
  };

  const handleContactoCompletado = ({ email, telefono }) => {
    setFormData((prev) => ({
      ...prev,
      contacto: {
        email,
        telefono,
        emailValidado: true,
        telefonoValidado: true,
      },
    }));

    setCurrentStep(3);
  };

  const handleAuthenticated = (token) => {
    console.log("Token autenticación:", token);

    // A partir de acá, esta aplicación decide qué hacer con el token.
    // Por ejemplo:
    // - guardarlo en sessionStorage/localStorage
    // - decodificarlo localmente si necesita mostrar datos
    // - enviarlo al backend propio de inscripción empresaria
    // - consultar permisos propios del sistema
  };

  return (
    <IericAuth onAuthenticated={handleAuthenticated}>
      {({ usuario, abrirLogin, cambiarUsuario }) => (
        <div className="app-shell">
          <Topbar usuario={usuario} onAbrirLogin={abrirLogin} onCambiarUsuario={cambiarUsuario} />

          <main className="main-area">
            <section className="content-grid">
              <div className="wizard-card">
                <StepIndicator currentStep={currentStep} mostrarTelefono={MOSTRAR_VALIDACION_TELEFONO} />

                {currentStep === 1 && (
                  <StepCuit initialCuit={formData.cuit} initialEmpresa={formData.empresa} onNext={handleCuitCompletado} />
                )}

                {currentStep === 2 && MOSTRAR_VALIDACION_TELEFONO && (
                  <StepContacto initialContacto={formData.contacto} onNext={handleContactoCompletado} />
                )}

                {currentStep === 2 && !MOSTRAR_VALIDACION_TELEFONO && <StepIdentidad onNext={() => {}} />}

                {currentStep === 3 && MOSTRAR_VALIDACION_TELEFONO && <StepIdentidad onNext={() => {}} />}
              </div>
            </section>
          </main>

          <Footer />
        </div>
      )}
    </IericAuth>
  );
}
