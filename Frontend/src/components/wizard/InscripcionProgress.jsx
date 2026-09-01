import { ChevronRight } from "lucide-react";

import "../../styles/components/inscripcionProgress.css";

const INSCRIPCION_STEPS = [
  { id: "empresa", label: "Empresa" },
  { id: "representantes", label: "Representantes" },
  { id: "nomina", label: "Nómina" },
  { id: "documentacion", label: "Documentación" },
  { id: "revision", label: "Revisión" },
];

export default function InscripcionProgress({ currentStep = "empresa" }) {
  const currentIndex = INSCRIPCION_STEPS.findIndex((step) => step.id === currentStep);

  return (
    <nav className="inscripcion-progress" aria-label="Progreso de la inscripción">
      <ol className="inscripcion-progress-list">
        {INSCRIPCION_STEPS.map((step, index) => {
          const isActive = step.id === currentStep;
          const isCompleted = currentIndex >= 0 && index < currentIndex;

          const stepClassName = ["inscripcion-progress-step", isActive ? "active" : "", isCompleted ? "completed" : ""]
            .filter(Boolean)
            .join(" ");

          return (
            <li className={stepClassName} key={step.id}>
              <span className="inscripcion-progress-label" aria-current={isActive ? "step" : undefined}>
                {step.label}
              </span>

              {index < INSCRIPCION_STEPS.length - 1 && (
                <ChevronRight className="inscripcion-progress-separator" size={17} aria-hidden="true" />
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
