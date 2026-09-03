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
          <h2 className="empresa-section-title">Información de la empresa</h2>

          <div className="empresa-form-grid">
            <div className="empresa-col-6">
              <input
                className="empresa-input"
                type="text"
                name="razonSocial"
                value={datos.razonSocial}
                placeholder="Razón social*"
                aria-label="Razón social"
                required
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-6">
              <SearchableSelect
                id="actividad"
                value={datos.actividadId}
                options={ACTIVIDADES_EMPRESA_MOCK}
                placeholder="Actividad de la empresa"
                required
                onChange={(value) => actualizarValor("actividadId", value)}
              />
            </div>

            <div className="empresa-col-6">
              <select
                className="empresa-input empresa-select"
                name="caracter"
                value={datos.caracter}
                aria-label="Carácter de la empresa"
                required
                onChange={actualizarCampo}
              >
                <option value="" disabled>
                  Carácter de la empresa*
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
                value={datos.tipoSociedadId}
                options={TIPOS_SOCIEDAD_MOCK}
                placeholder="Tipo de sociedad"
                required
                onChange={(value) => actualizarValor("tipoSociedadId", value)}
              />
            </div>
          </div>
        </section>

        {!UBICAR_EMPRESAS_INTEGRANTES_AL_FINAL && bloqueEmpresasIntegrantes}

        <section className="empresa-form-section">
          <h2 className="empresa-section-title">Domicilio</h2>

          <div className="empresa-form-grid">
            <div className="empresa-col-6">
              <input
                className="empresa-input"
                type="text"
                name="calle"
                value={datos.calle}
                placeholder="Calle*"
                aria-label="Calle"
                required
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-2">
              <input
                className="empresa-input"
                type="text"
                name="numero"
                value={datos.numero}
                placeholder="Número*"
                aria-label="Número"
                required
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-2">
              <input
                className="empresa-input"
                type="text"
                name="piso"
                value={datos.piso}
                placeholder="Piso"
                aria-label="Piso"
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-2">
              <input
                className="empresa-input"
                type="text"
                name="departamento"
                value={datos.departamento}
                placeholder="Depto./Oficina"
                aria-label="Departamento u oficina"
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-2">
              <input
                className="empresa-input"
                type="text"
                name="codigoPostal"
                value={datos.codigoPostal}
                placeholder="Código postal*"
                aria-label="Código postal"
                required
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-4">
              <input
                className="empresa-input"
                type="text"
                name="provincia"
                value={datos.provincia}
                placeholder="Provincia*"
                aria-label="Provincia"
                required
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-6">
              <SearchableSelect
                id="localidad"
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
          <h2 className="empresa-section-title">Datos de contacto</h2>

          <div className="empresa-form-grid">
            <div className="empresa-col-6">
              <input
                className={["empresa-input", datos.email && !correoValido ? "has-error" : ""].filter(Boolean).join(" ")}
                type="email"
                name="email"
                value={datos.email}
                placeholder="Correo electrónico*"
                aria-label="Correo electrónico"
                required
                onChange={actualizarCampo}
              />

              {datos.email && !correoValido && <span className="empresa-field-error">Ingrese un correo electrónico válido.</span>}
            </div>

            <div className="empresa-col-6">
              <input
                className={["empresa-input", !telefonoValido ? "has-error" : ""].filter(Boolean).join(" ")}
                type="tel"
                name="telefono"
                value={datos.telefono}
                placeholder="Teléfono"
                aria-label="Teléfono"
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
