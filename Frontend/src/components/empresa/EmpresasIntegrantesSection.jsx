import { Info, Pencil, Plus, Trash2 } from "lucide-react";

import "../../styles/components/empresasIntegrantesSection.css";

export default function EmpresasIntegrantesSection({ empresas = [], onAdd, onEdit, onDelete }) {
  const mostrarComoGrilla = empresas.length > 2;

  return (
    <div className="empresas-integrantes-section">
      <div className="empresas-integrantes-header">
        <div className="empresas-integrantes-title-wrapper">
          <h2 className="empresa-section-title">Empresas integrantes</h2>

          <span className="empresas-integrantes-info" tabIndex="0" aria-label="Información sobre empresas integrantes">
            <Info size={16} aria-hidden="true" />

            <span className="empresas-integrantes-tooltip" role="tooltip">
              Para UTE, UT y Consorcios de Cooperación, deberá informar las empresas que integran la sociedad.
              <span className="empresas-integrantes-tooltip-more">Más información</span>
            </span>
          </span>
        </div>

        <button type="button" className="empresas-integrantes-add" onClick={onAdd}>
          <Plus size={18} aria-hidden="true" />
          Agregar empresa
        </button>
      </div>

      {empresas.length > 0 && (
        <div className={["empresas-integrantes-list", mostrarComoGrilla ? "is-grid" : ""].filter(Boolean).join(" ")}>
          {empresas.map((empresa) => (
            <div className="empresa-integrante-card" key={empresa.id}>
              <div className="empresa-integrante-card-content">
                <span className="empresa-integrante-card-name">{empresa.razonSocial}</span>

                <div className="empresa-integrante-card-details">
                  <span>CUIT: {empresa.cuit}</span>

                  <span className="empresa-integrante-card-divider" aria-hidden="true" />

                  <span>{empresa.tipoSociedad}</span>
                </div>
              </div>

              <div className="empresa-integrante-card-actions">
                <button
                  type="button"
                  className="empresa-integrante-edit"
                  aria-label={`Editar ${empresa.razonSocial}`}
                  onClick={() => onEdit?.(empresa)}
                >
                  <Pencil size={18} aria-hidden="true" />
                </button>

                <button
                  type="button"
                  className="empresa-integrante-delete"
                  aria-label={`Eliminar ${empresa.razonSocial}`}
                  onClick={() => onDelete?.(empresa.id)}
                >
                  <Trash2 size={18} aria-hidden="true" />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
