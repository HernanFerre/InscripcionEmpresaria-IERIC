import { useState } from "react";

import Topbar from "../components/layout/Topbar.jsx";
import Footer from "../components/layout/Footer.jsx";
import StepIndicator from "../components/wizard/StepIndicator.jsx";
import InscripcionProgress from "../components/wizard/InscripcionProgress.jsx";

import StepCuit from "../steps/StepCuit.jsx";
import StepContacto from "../steps/StepContacto.jsx";
import StepIdentidad from "../steps/StepIdentidad.jsx";

import IericAuth from "../auth/IericAuth.jsx";
import StepEmpresa from "../steps/inscripcion/StepEmpresa.jsx";
import StepRepresentantes from "../steps/inscripcion/StepRepresentantes.jsx";
import StepNomina from "../steps/inscripcion/StepNomina.jsx";
import StepDocumentacion from "../steps/inscripcion/StepDocumentacion.jsx";

import { INICIAR_EN_INSCRIPCION, MOSTRAR_VALIDACION_TELEFONO } from "../config/featureFlags.js";

import "../styles/inscripcion.css";

export default function InscripcionPage() {
  const [currentStep, setCurrentStep] = useState(1);

  const [inscripcionStep, setInscripcionStep] = useState("empresa");

  const [faseActual, setFaseActual] = useState(INICIAR_EN_INSCRIPCION ? "inscripcion" : "validacion");

  const [formData, setFormData] = useState({
    cuit: "",
    empresa: null,
    cuiles: [],
    quiz: null,

    contacto: {
      email: "",
      telefono: "",
      emailValidado: false,
      telefonoValidado: false,
    },

    datosInscripcion: {
      empresa: null,
      representantes: [],
    },
  });

  const handleCuitCompletado = ({ cuit, empresa, cuiles = [], quiz = null }) => {
    setFormData((prev) => ({
      ...prev,
      cuit,
      empresa,
      cuiles,
      quiz,
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

  const handleIdentidadCompletada = () => {
    setInscripcionStep("empresa");
    setFaseActual("inscripcion");
  };

  const handleEmpresaCompletada = (datosEmpresa) => {
    setFormData((prev) => ({
      ...prev,
      datosInscripcion: {
        ...prev.datosInscripcion,
        empresa: datosEmpresa,
      },
    }));

    setInscripcionStep("representantes");
  };

  const handleVolverAEmpresa = () => {
    setInscripcionStep("empresa");
  };

  const handleContinuarANomina = () => {
    setInscripcionStep("nomina");
  };

  const handleVolverARepresentantes = () => {
    setInscripcionStep("representantes");
  };

  const handleContinuarADocumentacion = () => {
    setInscripcionStep("documentacion");
  };

  const handleVolverANomina = () => {
    setInscripcionStep("nomina");
  };

  const handleAuthenticated = () => {
    // La autenticación se vinculará al flujo de inscripción
    // después de construir las nuevas pantallas.
  };

  return (
    <IericAuth onAuthenticated={handleAuthenticated}>
      {({ usuario, estaLogueado, abrirLogin, cambiarUsuario }) => (
        <div className="app-shell">
          <Topbar usuario={usuario} onAbrirLogin={abrirLogin} onCambiarUsuario={cambiarUsuario} />

          <main className="main-area">
            <section className="content-grid">
              <div className="wizard-card">
                {faseActual === "validacion" && (
                  <>
                    <StepIndicator currentStep={currentStep} mostrarTelefono={MOSTRAR_VALIDACION_TELEFONO} />

                    {currentStep === 1 && (
                      <StepCuit
                        initialCuit={formData.cuit}
                        initialEmpresa={formData.empresa}
                        estaLogueado={estaLogueado}
                        onLoginRequired={abrirLogin}
                        onNext={handleCuitCompletado}
                      />
                    )}

                    {currentStep === 2 && MOSTRAR_VALIDACION_TELEFONO && (
                      <StepContacto initialContacto={formData.contacto} onNext={handleContactoCompletado} />
                    )}

                    {currentStep === 2 && !MOSTRAR_VALIDACION_TELEFONO && (
                      <StepIdentidad cuit={formData.cuit} cuiles={formData.cuiles} initialQuiz={formData.quiz} onNext={() => {}} />
                    )}

                    {currentStep === 3 && MOSTRAR_VALIDACION_TELEFONO && (
                      <StepIdentidad
                        cuit={formData.cuit}
                        cuiles={formData.cuiles}
                        initialQuiz={formData.quiz}
                        onNext={handleIdentidadCompletada}
                      />
                    )}
                  </>
                )}

                {faseActual === "inscripcion" && (
                  <>
                    <InscripcionProgress currentStep={inscripcionStep} />

                    {inscripcionStep === "empresa" && (
                      <StepEmpresa initialData={formData.datosInscripcion.empresa} onNext={handleEmpresaCompletada} />
                    )}

                    {inscripcionStep === "representantes" && (
                      <StepRepresentantes
                        representantes={formData.datosInscripcion.representantes}
                        onBack={handleVolverAEmpresa}
                        onNext={handleContinuarANomina}
                      />
                    )}

                    {inscripcionStep === "nomina" && (
                      <StepNomina onBack={handleVolverARepresentantes} onNext={handleContinuarADocumentacion} />
                    )}

                    {inscripcionStep === "documentacion" && <StepDocumentacion onBack={handleVolverANomina} />}
                  </>
                )}
              </div>
            </section>
          </main>

          <Footer />
        </div>
      )}
    </IericAuth>
  );
}
