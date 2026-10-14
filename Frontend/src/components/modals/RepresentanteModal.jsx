import { useEffect, useState } from "react";
import { Info, X } from "lucide-react";

import SearchableSelect from "../forms/SearchableSelect.jsx";
import { obtenerTiposRepresentantes } from "../../services/DatosMaestrosService.js";
import { esCuitValido, formatCuit, normalizarCuit } from "../../utils/formatters.js";

import "../../styles/components/representanteModal.css";

const DATOS_INICIALES = {
  empresaId: "",
  apellido: "",
  nombre: "",
  email: "",
  telefono: "",
  cuil: "",
  cargoId: "",
};

function formatearCuil(value) {
  return formatCuit(value).replace(/-/g, " ");
}

export default function RepresentanteModal({
  token,
  initialData = null,
  requiereEmpresaIntegrante = false,
  empresaIdPrincipal = null,
  tipoSociedadIdPrincipal = "",
  empresasIntegrantes = [],
  onClose,
  onSave,
}) {
  const [datos, setDatos] = useState(() => ({
    ...DATOS_INICIALES,
    ...(initialData ?? {}),
    empresaId: requiereEmpresaIntegrante ? String(initialData?.empresaId ?? "") : String(empresaIdPrincipal ?? ""),
    cargoId: String(initialData?.cargoId ?? initialData?.tipoRepresentanteId ?? ""),
    cuil: normalizarCuit(initialData?.cuil).slice(0, 11),
  }));

  const [errorCuil, setErrorCuil] = useState("");

  const [catalogoCargos, setCatalogoCargos] = useState({
    tipoSociedadId: "",
    opciones: [],
    cargando: false,
    error: "",
  });

  const opcionesEmpresas = empresasIntegrantes.map((empresa) => ({
    value: String(empresa.empresaId ?? empresa.id ?? ""),
    label: empresa.razonSocial,
  }));

  const empresaSeleccionada = empresasIntegrantes.find(
    (empresa) => String(empresa.empresaId ?? empresa.id ?? "") === String(datos.empresaId),
  );

  const tipoSociedadParaCargo = String((requiereEmpresaIntegrante ? empresaSeleccionada?.tipoSociedadId : tipoSociedadIdPrincipal) ?? "");

  const catalogoActual = catalogoCargos.tipoSociedadId === tipoSociedadParaCargo;
  const cargos = catalogoActual ? catalogoCargos.opciones : [];
  const cargandoCargos = Boolean(tipoSociedadParaCargo) && (!catalogoActual || catalogoCargos.cargando);
  const errorCargos = catalogoActual ? catalogoCargos.error : "";
  const cargoSeleccionado = cargos.find((cargo) => cargo.value === String(datos.cargoId));
  const empresaIdValido = Number.isSafeInteger(Number(datos.empresaId)) && Number(datos.empresaId) > 0;

  useEffect(() => {
    let activo = true;

    if (!tipoSociedadParaCargo) {
      return () => {
        activo = false;
      };
    }

    const cargarCargos = async () => {
      setCatalogoCargos({
        tipoSociedadId: tipoSociedadParaCargo,
        opciones: [],
        cargando: true,
        error: "",
      });

      try {
        const opciones = await obtenerTiposRepresentantes(tipoSociedadParaCargo, token);

        if (activo) {
          setCatalogoCargos({
            tipoSociedadId: tipoSociedadParaCargo,
            opciones,
            cargando: false,
            error: opciones.length ? "" : "No hay cargos disponibles para el tipo de sociedad seleccionado.",
          });
        }
      } catch (error) {
        if (activo) {
          setCatalogoCargos({
            tipoSociedadId: tipoSociedadParaCargo,
            opciones: [],
            cargando: false,
            error: error.message || "No fue posible obtener los cargos disponibles.",
          });
        }
      }
    };

    cargarCargos();

    return () => {
      activo = false;
    };
  }, [tipoSociedadParaCargo, token]);

  const actualizarValor = (campo, value) => {
    setDatos((prev) => ({
      ...prev,
      [campo]: value,
    }));

    if (["cuil", "nombre", "apellido"].includes(campo)) {
      setErrorCuil("");
    }
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

  const actualizarEmpresa = (empresaId) => {
    setDatos((prev) => ({
      ...prev,
      empresaId: String(empresaId),
      cargoId: "",
    }));
  };

  const handleSubmit = (event) => {
    event.preventDefault();

    const camposObligatorios = [datos.apellido, datos.nombre, datos.email, datos.telefono, datos.cuil, datos.cargoId];

    if (requiereEmpresaIntegrante) {
      camposObligatorios.unshift(datos.empresaId);
    }

    const camposObligatoriosCompletos = camposObligatorios.every((value) => String(value).trim().length > 0);

    if (!camposObligatoriosCompletos || cargandoCargos || errorCargos || !empresaIdValido || !cargoSeleccionado) {
      return;
    }

    const cuil = normalizarCuit(datos.cuil);

    if (!esCuitValido(cuil)) {
      const nombreCompleto = [datos.nombre.trim(), datos.apellido.trim()].filter(Boolean).join(" ");

      const motivo = cuil.length !== 11 ? "debe contener 11 números" : "tiene un dígito verificador incorrecto";

      setErrorCuil(`El CUIL ${formatearCuil(cuil)} de ${nombreCompleto} ${motivo}.`);

      return;
    }

    const tipoRepresentanteId = Number(cargoSeleccionado.idTipoRepresentante ?? cargoSeleccionado.value);

    if (!Number.isSafeInteger(tipoRepresentanteId) || tipoRepresentanteId <= 0) {
      return;
    }

    onSave?.({
      ...datos,
      cuil,
      empresaId: Number(datos.empresaId),
      empresaNombre: requiereEmpresaIntegrante ? (empresaSeleccionada?.razonSocial ?? "") : "",
      cargo: cargoSeleccionado?.label ?? "",
      cargoId: String(tipoRepresentanteId),
      tipoRepresentanteId,
    });
  };

  return (
    <div className="representante-modal-overlay" role="presentation" onMouseDown={onClose}>
      <div
        className="representante-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="representante-modal-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <header className="representante-modal-header">
          <div className="representante-modal-title-wrapper">
            <h2 id="representante-modal-title" className="section-title">
              Representante
            </h2>

            {requiereEmpresaIntegrante && (
              <span
                className="representante-modal-info"
                title="Seleccione la empresa integrante representada."
                aria-label="Información sobre la empresa representada"
              >
                <Info size={16} aria-hidden="true" />
              </span>
            )}
          </div>

          <button type="button" className="representante-modal-close" aria-label="Cerrar" onClick={onClose}>
            <X size={20} aria-hidden="true" />
          </button>
        </header>

        <form onSubmit={handleSubmit}>
          <div className="representante-modal-body">
            <div className="representante-modal-grid">
              {requiereEmpresaIntegrante && (
                <div className="representante-modal-field representante-modal-field--full">
                  <SearchableSelect
                    id="empresa-representante"
                    label="Empresa"
                    value={datos.empresaId}
                    options={opcionesEmpresas}
                    placeholder="Seleccione la empresa integrante"
                    required
                    onChange={actualizarEmpresa}
                  />
                </div>
              )}

              <div className="representante-modal-field">
                <label className="form-field-label" htmlFor="representante-apellido">
                  Apellido
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="representante-apellido"
                  className="representante-modal-input"
                  type="text"
                  name="apellido"
                  value={datos.apellido}
                  placeholder="Apellido"
                  required
                  maxLength={100}
                  onChange={actualizarCampo}
                />
              </div>

              <div className="representante-modal-field">
                <label className="form-field-label" htmlFor="representante-nombre">
                  Nombre
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="representante-nombre"
                  className="representante-modal-input"
                  type="text"
                  name="nombre"
                  value={datos.nombre}
                  placeholder="Nombre"
                  required
                  maxLength={100}
                  onChange={actualizarCampo}
                />
              </div>

              <div className="representante-modal-field">
                <label className="form-field-label" htmlFor="representante-cuil">
                  CUIL
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="representante-cuil"
                  className="representante-modal-input"
                  type="text"
                  name="cuil"
                  inputMode="numeric"
                  autoComplete="off"
                  value={formatearCuil(datos.cuil)}
                  placeholder="XX XXXXXXXX X"
                  aria-invalid={Boolean(errorCuil)}
                  aria-describedby={errorCuil ? "representante-cuil-error" : undefined}
                  required
                  onChange={actualizarCuil}
                />

                {errorCuil && (
                  <div id="representante-cuil-error" className="empresa-field-error" role="alert">
                    {errorCuil}
                  </div>
                )}
              </div>

              <div className="representante-modal-field">
                <SearchableSelect
                  id="cargo-representante"
                  label="Cargo"
                  value={datos.cargoId}
                  options={cargos}
                  placeholder={
                    !tipoSociedadParaCargo
                      ? "Seleccione primero la empresa"
                      : cargandoCargos
                        ? "Cargando cargos..."
                        : "Busque y seleccione el cargo"
                  }
                  required
                  disabled={!tipoSociedadParaCargo || cargandoCargos || Boolean(errorCargos)}
                  error={errorCargos}
                  onChange={(value) => actualizarValor("cargoId", value)}
                />
              </div>

              <div className="representante-modal-field">
                <label className="form-field-label" htmlFor="representante-correo">
                  Correo electrónico
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="representante-correo"
                  className="representante-modal-input"
                  type="email"
                  name="email"
                  value={datos.email}
                  placeholder="Correo electrónico"
                  required
                  maxLength={254}
                  onChange={actualizarCampo}
                />
              </div>

              <div className="representante-modal-field">
                <label className="form-field-label" htmlFor="representante-telefono">
                  Teléfono
                  <span aria-hidden="true">*</span>
                </label>

                <input
                  id="representante-telefono"
                  className="representante-modal-input"
                  type="tel"
                  name="telefono"
                  value={datos.telefono}
                  placeholder="Teléfono"
                  required
                  maxLength={30}
                  onChange={actualizarCampo}
                />
              </div>
            </div>
          </div>

          <footer className="representante-modal-footer">
            <button type="button" className="representante-modal-cancel" onClick={onClose}>
              Cancelar
            </button>

            <button
              type="submit"
              className="representante-modal-save"
              disabled={cargandoCargos || Boolean(errorCargos) || !empresaIdValido || !cargoSeleccionado}
            >
              Guardar
            </button>
          </footer>
        </form>
      </div>
    </div>
  );
}
