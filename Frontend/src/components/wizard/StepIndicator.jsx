const steps = [
  { number: 1, label: "Empresa" },
  { number: 2, label: "Teléfono" },
  { number: 3, label: "Información" },
];

export default function StepIndicator({ currentStep = 1 }) {
  return (
    <div className="steps">
      {steps.map((step) => {
        const isActive = step.number === currentStep;
        const isCompleted = step.number < currentStep;

        return (
          <div className="step-item" key={step.number}>
            <div className={["step-circle", isActive ? "active" : "", isCompleted ? "completed" : ""].filter(Boolean).join(" ")}>
              {isCompleted ? "✓" : step.number}
            </div>

            <span>{step.label}</span>
          </div>
        );
      })}
    </div>
  );
}
