import { useCallback, useState } from "react";

import Topbar from "../components/layout/Topbar.jsx";
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
import StepDeclaracionJurada from "../steps/inscripcion/StepDeclaracionJurada.jsx";
import StepRevision from "../steps/inscripcion/StepRevision.jsx";

import { INICIAR_EN_INSCRIPCION, INICIAR_EN_REVISION, MOSTRAR_VALIDACION_TELEFONO } from "../config/featureFlags.js";

import "../styles/inscripcion.css";

function crearIdRepresentante() {
  return `representante-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
}

function obtenerEmpresasRepresentadas(datosEmpresa) {
  const empresas = datosEmpresa?.empresasIntegrantes?.length ? datosEmpresa.empresasIntegrantes : datosEmpresa ? [datosEmpresa] : [];

  const resultado = new Map();

  for (const empresa of empresas) {
    const empresaId = Number(empresa.empresaId ?? empresa.id);

    if (!Number.isSafeInteger(empresaId) || empresaId <= 0) {
      continue;
    }

    resultado.set(empresaId, {
      razonSocial: String(empresa.razonSocial ?? ""),
      tipoSociedadId: String(empresa.tipoSociedadId ?? ""),
    });
  }

  return resultado;
}

export default function InscripcionPage() {
  const [currentStep, setCurrentStep] = useState(1);

  const [inscripcionStep, setInscripcionStep] = useState(INICIAR_EN_REVISION ? "revision" : "empresa");

  const [faseActual, setFaseActual] = useState(INICIAR_EN_REVISION || INICIAR_EN_INSCRIPCION ? "inscripcion" : "validacion");

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
      nomina: null,
      declaracionJurada: null,
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
    setFormData((prev) => {
      const empresaAnterior = prev.datosInscripcion.empresa;
      const empresasAnteriores = obtenerEmpresasRepresentadas(empresaAnterior);
      const empresasActuales = obtenerEmpresasRepresentadas(datosEmpresa);
      const solicitudId = Number(datosEmpresa.solicitudId);

      const mismaSolicitud = Number.isSafeInteger(solicitudId) && solicitudId > 0 && solicitudId === Number(empresaAnterior?.solicitudId);

      const representantesActualizados = mismaSolicitud
        ? (prev.datosInscripcion.representantes ?? [])
            .filter((representante) => empresasActuales.has(Number(representante.empresaId)))
            .map((representante) => {
              const empresaId = Number(representante.empresaId);
              const empresaActual = empresasActuales.get(empresaId);
              const empresaPrevia = empresasAnteriores.get(empresaId);
              const cambioSociedad = empresaPrevia && empresaPrevia.tipoSociedadId !== empresaActual.tipoSociedadId;

              return {
                ...representante,
                empresaNombre: empresaActual.razonSocial,
                ...(cambioSociedad ? { cargo: "Revisar cargo", cargoId: "", tipoRepresentanteId: 0 } : {}),
              };
            })
        : [];

      return {
        ...prev,
        datosInscripcion: {
          ...prev.datosInscripcion,
          empresa: datosEmpresa,
          representantes: representantesActualizados,
          nomina: mismaSolicitud ? prev.datosInscripcion.nomina : null,
          declaracionJurada: mismaSolicitud ? prev.datosInscripcion.declaracionJurada : null,
        },
      };
    });

    setInscripcionStep("representantes");
  };

  const handleGuardarRepresentante = (datosRepresentante) => {
    setFormData((prev) => {
      const representantesActuales = prev.datosInscripcion.representantes ?? [];

      const representanteId = datosRepresentante.id;

      const representantesActualizados = representanteId
        ? representantesActuales.map((representante) =>
            representante.id === representanteId
              ? {
                  ...representante,
                  ...datosRepresentante,
                  id: representanteId,
                }
              : representante,
          )
        : [
            ...representantesActuales,
            {
              ...datosRepresentante,
              id: crearIdRepresentante(),
            },
          ];

      return {
        ...prev,
        datosInscripcion: {
          ...prev.datosInscripcion,
          representantes: representantesActualizados,
        },
      };
    });
  };

  const handleEliminarRepresentante = (representanteId) => {
    setFormData((prev) => ({
      ...prev,
      datosInscripcion: {
        ...prev.datosInscripcion,
        representantes: (prev.datosInscripcion.representantes ?? []).filter((representante) => representante.id !== representanteId),
      },
    }));
  };

  const handleVolverAEmpresa = () => {
    setInscripcionStep("empresa");
  };

  const handleContinuarANomina = (representantesGuardados) => {
    setFormData((prev) => ({
      ...prev,
      datosInscripcion: {
        ...prev.datosInscripcion,
        representantes: representantesGuardados,
      },
    }));
    setInscripcionStep("nomina");
  };

  const handleVolverARepresentantes = () => {
    setInscripcionStep("representantes");
  };

  const handleNominaActualizada = useCallback((datosNomina) => {
    setFormData((prev) => {
      const solicitudId = Number(prev.datosInscripcion.empresa?.solicitudId);

      if (
        !Number.isSafeInteger(solicitudId) ||
        solicitudId <= 0 ||
        Number(datosNomina?.solicitudId) !== solicitudId ||
        prev.datosInscripcion.nomina === datosNomina
      ) {
        return prev;
      }

      return {
        ...prev,
        datosInscripcion: {
          ...prev.datosInscripcion,
          nomina: datosNomina,
        },
      };
    });
  }, []);

  const handleNominaCompletada = (datosNomina) => {
    handleNominaActualizada(datosNomina);

    setInscripcionStep(datosNomina.requiereDeclaracionJurada ? "declaracion-jurada" : "documentacion");
  };

  const handleDeclaracionJuradaCompletada = (datosDeclaracionJurada) => {
    setFormData((prev) => ({
      ...prev,
      datosInscripcion: {
        ...prev.datosInscripcion,
        declaracionJurada: datosDeclaracionJurada,
      },
    }));

    setInscripcionStep("documentacion");
  };

  const handleVolverANomina = () => {
    setInscripcionStep("nomina");
  };

  const handleVolverDesdeDocumentacion = () => {
    setInscripcionStep(formData.datosInscripcion.nomina?.requiereDeclaracionJurada ? "declaracion-jurada" : "nomina");
  };

  const handleContinuarARevision = () => {
    setInscripcionStep("revision");
  };

  const handleVolverADocumentacion = () => {
    setInscripcionStep("documentacion");
  };

  const handleAuthenticated = () => {
    // La autenticación se vinculará al flujo de inscripción
    // después de construir las nuevas pantallas.
  };

  return (
    <IericAuth onAuthenticated={handleAuthenticated}>
      {({ token, usuario, estaLogueado, abrirLogin, cambiarUsuario }) => (
        <div className="app-shell">
          <Topbar
            usuario={usuario}
            cuit={formData.cuit}
            mostrarDatosInscripcion={faseActual === "inscripcion"}
            onAbrirLogin={abrirLogin}
            onCambiarUsuario={cambiarUsuario}
          />

          <main className="main-area">
            <section className="content-grid">
              <div className="wizard-card wizard-card--inscripcion">
                {faseActual === "validacion" && (
                  <>
                    <StepIndicator currentStep={currentStep} mostrarTelefono={MOSTRAR_VALIDACION_TELEFONO} />

                    {currentStep === 1 && (
                      <StepCuit
                        token={token}
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
                      <StepIdentidad
                        token={token}
                        cuiles={formData.cuiles}
                        initialQuiz={formData.quiz}
                        onNext={handleIdentidadCompletada}
                      />
                    )}

                    {currentStep === 3 && MOSTRAR_VALIDACION_TELEFONO && (
                      <StepIdentidad
                        token={token}
                        cuiles={formData.cuiles}
                        initialQuiz={formData.quiz}
                        onNext={handleIdentidadCompletada}
                      />
                    )}
                  </>
                )}

                {faseActual === "inscripcion" && (
                  <>
                    <InscripcionProgress
                      currentStep={inscripcionStep}
                      mostrarDeclaracionJurada={formData.datosInscripcion.nomina?.requiereDeclaracionJurada ?? true}
                    />

                    {inscripcionStep === "empresa" && (
                      <StepEmpresa
                        token={token}
                        cuit={formData.cuit}
                        initialData={formData.datosInscripcion.empresa}
                        onNext={handleEmpresaCompletada}
                      />
                    )}

                    {inscripcionStep === "representantes" && (
                      <StepRepresentantes
                        token={token}
                        solicitudId={formData.datosInscripcion.empresa?.solicitudId ?? null}
                        empresaId={formData.datosInscripcion.empresa?.empresaId ?? null}
                        tipoSociedadId={formData.datosInscripcion.empresa?.tipoSociedadId ?? ""}
                        empresasIntegrantes={formData.datosInscripcion.empresa?.empresasIntegrantes ?? []}
                        representantes={formData.datosInscripcion.representantes}
                        onSave={handleGuardarRepresentante}
                        onDelete={handleEliminarRepresentante}
                        onBack={handleVolverAEmpresa}
                        onNext={handleContinuarANomina}
                      />
                    )}

                    {inscripcionStep === "nomina" && (
                      <StepNomina
                        key={formData.datosInscripcion.empresa?.solicitudId ?? "sin-solicitud"}
                        token={token}
                        solicitudId={formData.datosInscripcion.empresa?.solicitudId ?? null}
                        nomina={formData.datosInscripcion.nomina}
                        onChange={handleNominaActualizada}
                        onBack={handleVolverARepresentantes}
                        onNext={handleNominaCompletada}
                      />
                    )}

                    {inscripcionStep === "declaracion-jurada" && (
                      <StepDeclaracionJurada
                        initialData={formData.datosInscripcion.declaracionJurada}
                        onBack={handleVolverANomina}
                        onNext={handleDeclaracionJuradaCompletada}
                      />
                    )}
                    {inscripcionStep === "documentacion" && (
                      <StepDocumentacion onBack={handleVolverDesdeDocumentacion} onNext={handleContinuarARevision} />
                    )}

                    {inscripcionStep === "revision" && <StepRevision onBack={handleVolverADocumentacion} />}
                  </>
                )}
              </div>
            </section>
          </main>
          <p className="inscripcion-contact-note">
            <span>En caso de duda podrá contactarse a:</span>
            <span>011 4363 3800</span>
            <span aria-hidden="true">-</span>
            <span>tramites.empleadores@ieric.org.ar</span>
          </p>
        </div>
      )}
    </IericAuth>
  );
}
