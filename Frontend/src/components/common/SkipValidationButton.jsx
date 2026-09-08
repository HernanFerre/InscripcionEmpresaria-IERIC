import { PERMITIR_SALTEAR_VALIDACIONES } from "../../config/featureFlags.js";
import "../../styles/components/skipValidationButton.css";

export default function SkipValidationButton({ onClick }) {
  if (!PERMITIR_SALTEAR_VALIDACIONES) {
    return null;
  }

  return (
    <button type="button" className="skip-validation-button" onClick={onClick}>
      Saltear Paso
    </button>
  );
}
