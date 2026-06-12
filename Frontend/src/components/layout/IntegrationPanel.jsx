import { CheckCircle } from "lucide-react";

export default function IntegrationPanel({ services = [], loading = false }) {
  return (
    <aside className="integration-panel">
      <h2>INTEGRACIÓN</h2>

      {services.map((service) => {
        const isValidated = service.status === "validated";

        return (
          <div className={`integration-item ${isValidated ? "validated" : "pending"}`} key={service.id}>
            <CheckCircle size={42} />

            <div>
              <strong>{service.name}</strong>
              <span>{loading ? "Consultando..." : isValidated ? "Validado" : "Pendiente"}</span>
            </div>
          </div>
        );
      })}
    </aside>
  );
}
