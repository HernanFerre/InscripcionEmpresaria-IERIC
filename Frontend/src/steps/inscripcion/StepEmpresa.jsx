import { useState } from "react";
import { Info } from "lucide-react";

import SearchableSelect from "../../components/forms/SearchableSelect.jsx";
import EmpresasIntegrantesSection from "../../components/empresa/EmpresasIntegrantesSection.jsx";
import EmpresaIntegranteModal from "../../components/modals/EmpresaIntegranteModal.jsx";

import { UBICAR_EMPRESAS_INTEGRANTES_AL_FINAL } from "../../config/featureFlags.js";

import {
  ACTIVIDADES_EMPRESA_MOCK,
  CARACTERES_EMPRESA_MOCK,
  LOCALIDADES_MOCK,
  TIPOS_SOCIEDAD_MOCK,
} from "../../mocks/InscripcionCatalogosMock.js";

import "../../styles/stepEmpresa.css";

const DATOS_INICIALES = {
  razonSocial: "",
  actividadId: "",
  caracter: "",
  tipoSociedadId: "",
  empresasIntegrantes: [],
  calle: "",
  numero: "",
  piso: "",
  departamento: "",
  codigoPostal: "",
  provincia: "",
  localidadId: "",
  email: "",
  telefono: "",
};

const PERMITIR_CONTINUAR_MOCK = true;

const TIPOS_SOCIEDAD_CON_INTEGRANTES = ["ut", "ute", "consorcio-cooperacion"];

function emailEsValido(email) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function telefonoEsValido(telefono) {
  if (!telefono.trim()) {
    return true;
  }

  const soloNumeros = telefono.replace(/\D/g, "");

  return soloNumeros.length >= 8 && soloNumeros.length <= 15;
}

export default function StepEmpresa({ initialData = null, onNext }) {
  const [datos, setDatos] = useState({
    ...DATOS_INICIALES,
    ...(initialData ?? {}),
    empresasIntegrantes: initialData?.empresasIntegrantes ?? [],
  });

  const [modalEmpresaIntegranteAbierto, setModalEmpresaIntegranteAbierto] = useState(false);

  const [empresaIntegranteSeleccionada, setEmpresaIntegranteSeleccionada] = useState(null);

  const mostrarEmpresasIntegrantes = TIPOS_SOCIEDAD_CON_INTEGRANTES.includes(datos.tipoSociedadId);

  const actualizarValor = (campo, value) => {
    setDatos((prev) => ({
      ...prev,
      [campo]: value,
    }));
  };

  const actualizarCampo = (event) => {
    actualizarValor(event.target.name, event.target.value);
  };

  const abrirNuevaEmpresaIntegrante = () => {
    setEmpresaIntegranteSeleccionada(null);
    setModalEmpresaIntegranteAbierto(true);
  };

  const abrirEdicionEmpresaIntegrante = (empresa) => {
    setEmpresaIntegranteSeleccionada(empresa);
    setModalEmpresaIntegranteAbierto(true);
  };

  const cerrarModalEmpresaIntegrante = () => {
    setModalEmpresaIntegranteAbierto(false);
    setEmpresaIntegranteSeleccionada(null);
  };

  const guardarEmpresaIntegrante = (empresa) => {
    setDatos((prev) => {
      const empresasActuales = prev.empresasIntegrantes ?? [];
      const empresaSeleccionadaId = empresaIntegranteSeleccionada?.id;

      if (empresaSeleccionadaId) {
        return {
          ...prev,
          empresasIntegrantes: empresasActuales.map((empresaActual) =>
            empresaActual.id === empresaSeleccionadaId
              ? {
                  ...empresa,
                  id: empresaSeleccionadaId,
                }
              : empresaActual,
          ),
        };
      }

      return {
        ...prev,
        empresasIntegrantes: [
          ...empresasActuales,
          {
            ...empresa,
            id: `empresa-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
          },
        ],
      };
    });

    cerrarModalEmpresaIntegrante();
  };

  const eliminarEmpresaIntegrante = (empresaId) => {
    setDatos((prev) => ({
      ...prev,
      empresasIntegrantes: (prev.empresasIntegrantes ?? []).filter((empresa) => empresa.id !== empresaId),
    }));
  };

  const correoValido = datos.email.trim().length > 0 && emailEsValido(datos.email);

  const telefonoValido = telefonoEsValido(datos.telefono);

  const camposObligatoriosCompletos = [
    datos.razonSocial,
    datos.actividadId,
    datos.caracter,
    datos.tipoSociedadId,
    datos.calle,
    datos.numero,
    datos.codigoPostal,
    datos.provincia,
    datos.localidadId,
    datos.email,
  ].every((value) => String(value).trim().length > 0);

  const formularioValido = camposObligatoriosCompletos && correoValido && telefonoValido;

  const handleSubmit = (event) => {
    event.preventDefault();

    if (!PERMITIR_CONTINUAR_MOCK && !formularioValido) {
      return;
    }

    onNext?.(datos);
  };

  const bloqueEmpresasIntegrantes = mostrarEmpresasIntegrantes ? (
    <EmpresasIntegrantesSection
      empresas={datos.empresasIntegrantes}
      onAdd={abrirNuevaEmpresaIntegrante}
      onEdit={abrirEdicionEmpresaIntegrante}
      onDelete={eliminarEmpresaIntegrante}
    />
  ) : null;

  return (
    <>
      <form className="empresa-step-form" noValidate onSubmit={handleSubmit}>
        <section className="empresa-form-section">
          <h2 className="section-title empresa-section-title">Información de la empresa</h2>

          <div className="empresa-form-grid">
            <div className="empresa-col-6">
              <label className="form-field-label" htmlFor="razon-social">
                Razón social
                <span aria-hidden="true">*</span>
              </label>

              <input
                id="razon-social"
                className="empresa-input"
                type="text"
                name="razonSocial"
                value={datos.razonSocial}
                placeholder="Nombre de la organización"
                required
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-6">
              <SearchableSelect
                id="actividad"
                label="Actividad de la empresa"
                value={datos.actividadId}
                options={ACTIVIDADES_EMPRESA_MOCK}
                placeholder="Actividad de la organización"
                required
                onChange={(value) => actualizarValor("actividadId", value)}
              />
            </div>

            <div className="empresa-col-6">
              <label className="form-field-label" htmlFor="caracter-empresa">
                Carácter
                <span aria-hidden="true">*</span>
              </label>

              <select
                id="caracter-empresa"
                className="empresa-input empresa-select"
                name="caracter"
                value={datos.caracter}
                required
                onChange={actualizarCampo}
              >
                <option value="" disabled>
                  Carácter de la organización
                </option>

                {CARACTERES_EMPRESA_MOCK.map((option) => (
                  <option value={option.value} key={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="empresa-col-6">
              <SearchableSelect
                id="tipo-sociedad"
                label="Tipo de sociedad"
                value={datos.tipoSociedadId}
                options={TIPOS_SOCIEDAD_MOCK}
                placeholder="Busque y seleccione el tipo de sociedad"
                required
                onChange={(value) => actualizarValor("tipoSociedadId", value)}
              />
            </div>
          </div>
        </section>

        {!UBICAR_EMPRESAS_INTEGRANTES_AL_FINAL && bloqueEmpresasIntegrantes}

        <section className="empresa-form-section">
          <h2 className="section-title empresa-section-title">Domicilio</h2>

          <div className="empresa-form-grid">
            <div className="empresa-col-6">
              <label className="form-field-label" htmlFor="calle">
                Calle
                <span aria-hidden="true">*</span>
              </label>

              <input
                id="calle"
                className="empresa-input"
                type="text"
                name="calle"
                value={datos.calle}
                placeholder="Calle"
                required
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-2">
              <label className="form-field-label" htmlFor="numero">
                Número
                <span aria-hidden="true">*</span>
              </label>

              <input
                id="numero"
                className="empresa-input"
                type="text"
                name="numero"
                value={datos.numero}
                placeholder="Número"
                required
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-2">
              <label className="form-field-label" htmlFor="piso">
                Piso
              </label>

              <input
                id="piso"
                className="empresa-input"
                type="text"
                name="piso"
                value={datos.piso}
                placeholder="Piso"
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-2">
              <label className="form-field-label" htmlFor="departamento">
                Depto./Oficina
              </label>

              <input
                id="departamento"
                className="empresa-input"
                type="text"
                name="departamento"
                value={datos.departamento}
                placeholder="Depto./Oficina"
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-2">
              <label className="form-field-label" htmlFor="codigo-postal">
                Código postal
                <span aria-hidden="true">*</span>
              </label>

              <input
                id="codigo-postal"
                className="empresa-input"
                type="text"
                name="codigoPostal"
                value={datos.codigoPostal}
                placeholder="Código postal"
                required
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-4">
              <label className="form-field-label" htmlFor="provincia">
                Provincia
                <span aria-hidden="true">*</span>
              </label>

              <input
                id="provincia"
                className="empresa-input"
                type="text"
                name="provincia"
                value={datos.provincia}
                placeholder="Provincia"
                required
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-6">
              <SearchableSelect
                id="localidad"
                label="Localidad"
                value={datos.localidadId}
                options={LOCALIDADES_MOCK}
                placeholder="Localidad"
                required
                onChange={(value) => actualizarValor("localidadId", value)}
              />
            </div>
          </div>
        </section>

        <section className="empresa-form-section">
          <h2 className="section-title empresa-section-title">Datos de contacto</h2>

          <div className="empresa-form-grid">
            <div className="empresa-col-6">
              <label className="form-field-label" htmlFor="correo-electronico">
                Correo electrónico
                <span aria-hidden="true">*</span>
              </label>

              <input
                id="correo-electronico"
                className={["empresa-input", datos.email && !correoValido ? "has-error" : ""].filter(Boolean).join(" ")}
                type="email"
                name="email"
                value={datos.email}
                placeholder="Correo electrónico"
                required
                onChange={actualizarCampo}
              />

              {datos.email && !correoValido && <span className="empresa-field-error">Ingrese un correo electrónico válido.</span>}
            </div>

            <div className="empresa-col-6">
              <label className="form-field-label" htmlFor="telefono">
                Teléfono
              </label>

              <input
                id="telefono"
                className={["empresa-input", !telefonoValido ? "has-error" : ""].filter(Boolean).join(" ")}
                type="tel"
                name="telefono"
                value={datos.telefono}
                placeholder="Teléfono"
                onChange={actualizarCampo}
              />

              {!telefonoValido && <span className="empresa-field-error">Ingrese un teléfono válido.</span>}
            </div>
          </div>
        </section>

        {UBICAR_EMPRESAS_INTEGRANTES_AL_FINAL && bloqueEmpresasIntegrantes}

        <div className="empresa-required-note" role="note">
          <Info size={15} aria-hidden="true" />

          <span>Los campos marcados con * son obligatorios</span>
        </div>

        <div className="empresa-form-actions">
          <button type="button" className="empresa-back-button" disabled>
            Volver
          </button>

          <button type="submit" className="next-step-button" disabled={!PERMITIR_CONTINUAR_MOCK && !formularioValido}>
            Continuar
          </button>
        </div>
      </form>

      {modalEmpresaIntegranteAbierto && (
        <EmpresaIntegranteModal
          key={empresaIntegranteSeleccionada?.id ?? "nueva-empresa"}
          initialData={empresaIntegranteSeleccionada}
          onClose={cerrarModalEmpresaIntegrante}
          onSave={guardarEmpresaIntegrante}
        />
      )}
    </>
  );
}
