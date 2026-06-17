import { useState } from "react";

import Topbar from "../components/layout/Topbar.jsx";
import Footer from "../components/layout/Footer.jsx";
import StepIndicator from "../components/wizard/StepIndicator.jsx";

import StepCuit from "../steps/StepCuit.jsx";
import StepContacto from "../steps/StepContacto.jsx";
import StepIdentidad from "../steps/StepIdentidad.jsx";

import "../styles/inscripcion.css";

export default function InscripcionPage() {
  const [currentStep, setCurrentStep] = useState(1);

  const [formData, setFormData] = useState({
    cuit: "",
    empresa: null,
    contacto: {
      email: "",
      telefono: "",
      emailValidado: false,
      telefonoValidado: false,
    },
  });

  const handleCuitCompletado = ({ cuit, empresa }) => {
    setFormData((prev) => ({
      ...prev,
      cuit,
      empresa,
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

  return (
    <div className="app-shell">
      <Topbar />

      <main className="main-area">
        <section className="content-grid">
          <div className="wizard-card">
            <StepIndicator currentStep={currentStep} />

            {currentStep === 1 && <StepCuit initialCuit={formData.cuit} initialEmpresa={formData.empresa} onNext={handleCuitCompletado} />}

            {currentStep === 2 && <StepContacto initialContacto={formData.contacto} onNext={handleContactoCompletado} />}

            {currentStep === 3 && <StepIdentidad onNext={() => {}} />}
          </div>
        </section>
      </main>

      <Footer />
    </div>
  );
}
