import { useState } from "react";
import { X } from "lucide-react";

import { esCuitValido, formatCuit, normalizarCuit } from "../../utils/formatters.js";

import "../../styles/components/trabajadorModal.css";

const DATOS_INICIALES = {
  apellidoNombre: "",
  cuil: "",
  ingreso: "",
  egreso: "",
};

function formatearCuil(value) {
  return formatCuit(value).replace(/-/g, " ");
}

function normalizarFechaParaInput(value) {
  const fecha = String(value ?? "").trim();

  if (!fecha) {
    return "";
  }

  if (/^\d{4}-\d{2}-\d{2}$/.test(fecha)) {
    return fecha;
  }

  const coincidencia = fecha.match(/^(\d{2})\/(\d{2})\/(\d{4})$/);

  if (!coincidencia) {
    return "";
  }

  const [, dia, mes, anio] = coincidencia;

  return `${anio}-${mes}-${dia}`;
}

export default function TrabajadorModal({ initialData = null, onClose, onSave }) {
  const [datos, setDatos] = useState(() => ({
    ...DATOS_INICIALES,
    ...(initialData ?? {}),
    cuil: normalizarCuit(initialData?.cuil).slice(0, 11),
    ingreso: normalizarFechaParaInput(initialData?.ingreso),
    egreso: normalizarFechaParaInput(initialData?.egreso),
  }));

  const [errorProceso, setErrorProceso] = useState("");

  const actualizarValor = (campo, value) => {
    setDatos((prev) => ({
      ...prev,
      [campo]: value,
    }));

    setErrorProceso("");
  };

  const actualizarCampo = (event) => {
    actualizarValor(event.target.name, event.target.value);
  };

  const actualizarCuil = (event) => {
    const input = event.target;
    const posicionOriginal = input.selectionStart ?? input.value.length;

    let digitosAntesDelCursor = normalizarCuit(input.value.slice(0, posicionOriginal)).length;

    let cuil = normalizarCuit(input.value).slice(0, 11);

    // Permite borrar a través de los espacios que agrega la máscara.
    if (cuil === datos.cuil && input.value !== formatearCuil(datos.cuil)) {
      if (event.nativeEvent.inputType === "deleteContentBackward" && digitosAntesDelCursor > 0) {
        cuil = cuil.slice(0, digitosAntesDelCursor - 1) + cuil.slice(digitosAntesDelCursor);

        digitosAntesDelCursor -= 1;
      } else if (event.nativeEvent.inputType === "deleteContentForward") {
        cuil = cuil.slice(0, digitosAntesDelCursor) + cuil.slice(digitosAntesDelCursor + 1);
      }
    }

    actualizarValor("cuil", cuil);

    const cantidadDigitos = Math.min(digitosAntesDelCursor, cuil.length);

    const posicionNueva = cantidadDigitos + (cantidadDigitos > 2 ? 1 : 0) + (cantidadDigitos > 10 ? 1 : 0);

    requestAnimationFrame(() => {
      if (document.activeElement === input) {
        input.setSelectionRange(posicionNueva, posicionNueva);
      }
    });
  };

  const handleSubmit = (event) => {
    event.preventDefault();

    const apellidoNombre = String(datos.apellidoNombre ?? "").trim();
    const cuil = normalizarCuit(datos.cuil);

    if (initialData?.origen === "servicio") {
      setErrorProceso("Los datos de los trabajadores provenientes del servicio no pueden modificarse.");
      return;
    }

    if (!apellidoNombre) {
      setErrorProceso("Ingrese el apellido y nombre del trabajador.");
      return;
    }

    if (apellidoNombre.length > 200) {
      setErrorProceso("El apellido y nombre del trabajador no puede superar los 200 caracteres.");
      return;
    }

    if (cuil.length !== 11 || cuil.startsWith("0") || !esCuitValido(cuil)) {
      const motivo =
        cuil.length !== 11 ? "debe contener 11 números" : cuil.startsWith("0") ? "no es válido" : "tiene un dígito verificador incorrecto";

      setErrorProceso(`El CUIL ${formatearCuil(cuil)} de ${apellidoNombre} ${motivo}.`);
      return;
    }

    if (!datos.ingreso) {
      setErrorProceso(`Ingrese la fecha de ingreso de ${apellidoNombre}.`);
      return;
    }

    if (datos.egreso && datos.egreso < datos.ingreso) {
      setErrorProceso(`La fecha de egreso de ${apellidoNombre} no puede ser anterior a la fecha de ingreso.`);
      return;
    }

    onSave?.({
      ...datos,
      apellidoNombre,
      cuil,
      egreso: datos.egreso || "",
      perteneceConstruccion: initialData?.perteneceConstruccion ?? true,
      origen: "manual",
    });
  };

  return (
    <div className="trabajador-modal-overlay" role="presentation" onMouseDown={onClose}>
      <div
        className="trabajador-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="trabajador-modal-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <header className="trabajador-modal-header">
          <h2 id="trabajador-modal-title" className="section-title">
            Trabajador
          </h2>

          <button type="button" className="trabajador-modal-close" aria-label="Cerrar" onClick={onClose}>
            <X size={20} aria-hidden="true" />
          </button>
        </header>

        <form onSubmit={handleSubmit} autoComplete="off">
          <div className="trabajador-modal-body">
            <div className="trabajador-modal-grid">
              <div className="trabajador-modal-field">
                <label className="form-field-label" htmlFor="trabajador-apellido-nombre">
                  Apellido y nombre
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="trabajador-apellido-nombre"
                  className="trabajador-modal-input"
                  type="text"
                  name="apellidoNombre"
                  autoComplete="off"
                  value={datos.apellidoNombre}
                  placeholder="Sin comas ni separaciones"
                  required
                  maxLength={200}
                  onChange={actualizarCampo}
                />
              </div>

              <div className="trabajador-modal-field">
                <label className="form-field-label" htmlFor="trabajador-cuil">
                  CUIL
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="trabajador-cuil"
                  className="trabajador-modal-input"
                  type="text"
                  name="cuil"
                  inputMode="numeric"
                  autoComplete="off"
                  value={formatearCuil(datos.cuil)}
                  placeholder="XX XXXXXXXX X"
                  required
                  onChange={actualizarCuil}
                />
              </div>

              <div className="trabajador-modal-field">
                <label className="form-field-label" htmlFor="trabajador-ingreso">
                  Fecha de ingreso
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="trabajador-ingreso"
                  className="trabajador-modal-input"
                  type="date"
                  name="ingreso"
                  autoComplete="off"
                  value={datos.ingreso}
                  required
                  onChange={actualizarCampo}
                />
              </div>

              <div className="trabajador-modal-field">
                <label className="form-field-label" htmlFor="trabajador-egreso">
                  Fecha de egreso
                </label>

                <input
                  id="trabajador-egreso"
                  className="trabajador-modal-input"
                  type="date"
                  name="egreso"
                  autoComplete="off"
                  value={datos.egreso}
                  onChange={actualizarCampo}
                />
              </div>
            </div>

            {errorProceso && (
              <div className="empresa-field-error" role="alert">
                {errorProceso}
              </div>
            )}
          </div>

          <footer className="trabajador-modal-footer">
            <button type="button" className="trabajador-modal-cancel" onClick={onClose}>
              Cancelar
            </button>

            <button type="submit" className="trabajador-modal-save">
              Guardar
            </button>
          </footer>
        </form>
      </div>
    </div>
  );
}
