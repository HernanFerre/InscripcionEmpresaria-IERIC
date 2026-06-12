import { useState } from "react";
import { User, Bell } from "lucide-react";

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
      <header className="topbar">
        <div className="topbar-left">
          <strong className="brand">IERIC</strong>
          <span className="divider"></span>
          <span className="portal-title">Portal de inscripción empresarial</span>
        </div>

        <div className="topbar-right">
          <div className="user-icon">
            <User size={30} />
          </div>

          <div className="user-info">
            <span>Usuario logueado</span>
            <strong>Admin IERIC</strong>
          </div>

          <div className="bell-wrapper">
            <Bell size={28} />
            <span className="notification-dot"></span>
          </div>
        </div>
      </header>

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
      <footer className="app-footer">
        <div>
          <strong>IERIC</strong>
          <span>Instituto de Estadística y Registro de la Industria de la Construcción</span>
          <span>© IERIC - Todos los derechos reservados.</span>
        </div>
      </footer>
    </div>
  );
}
