import { useEffect, useState } from "react";
import { Info } from "lucide-react";

import SearchableSelect from "../../components/forms/SearchableSelect.jsx";
import EmpresasIntegrantesSection from "../../components/empresa/EmpresasIntegrantesSection.jsx";
import EmpresaIntegranteModal from "../../components/modals/EmpresaIntegranteModal.jsx";

import { UBICAR_EMPRESAS_INTEGRANTES_AL_FINAL } from "../../config/featureFlags.js";

import { LOCALIDADES_MOCK } from "../../mocks/InscripcionCatalogosMock.js";

import { obtenerCatalogosEmpresa } from "../../services/DatosMaestrosService.js";
import { guardarEmpresaSolicitud } from "../../services/InscripcionService.js";

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

const CATALOGOS_INICIALES = {
  actividadesEmpresa: [],
  caracteresEmpresa: [],
  tiposSociedad: [],
};

/*
 * Identificadores provenientes del catálogo de tipos de sociedad:
 * 8  = U.T.E.
 * 26 = Consorcio de cooperación
 * 28 = U.T.
 */
const TIPOS_SOCIEDAD_CON_INTEGRANTES = ["8", "26", "28"];

function emailEsValido(email) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function telefonoEsValido(telefono) {
  if (!telefono.trim()) {
    return true;
  }

  const caracteresValidos = /^[\d+()\-\s]+$/.test(telefono);
  const soloNumeros = telefono.replace(/\D/g, "");

  return caracteresValidos && soloNumeros.length >= 8 && soloNumeros.length <= 15;
}

function cuitEsValido(cuit) {
  const cuitNormalizado = String(cuit ?? "").replace(/\D/g, "");

  if (cuitNormalizado.length !== 11) {
    return false;
  }

  const multiplicadores = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];

  const suma = multiplicadores.reduce((acumulado, multiplicador, indice) => acumulado + Number(cuitNormalizado[indice]) * multiplicador, 0);

  const resto = suma % 11;
  const digitoVerificador = resto === 0 ? 0 : resto === 1 ? 9 : 11 - resto;

  return digitoVerificador === Number(cuitNormalizado[10]);
}

function pisoEsValido(piso) {
  const valor = String(piso ?? "").trim();

  if (!valor) {
    return true;
  }

  if (!/^\d+$/.test(valor)) {
    return false;
  }

  const numeroPiso = Number(valor);

  return Number.isInteger(numeroPiso) && numeroPiso >= 0 && numeroPiso <= 255;
}

function identificadorEsValido(valor) {
  const identificador = Number(valor);

  return Number.isInteger(identificador) && identificador > 0;
}

export default function StepEmpresa({ token, cuit, initialData = null, onNext }) {
  const [datos, setDatos] = useState({
    ...DATOS_INICIALES,
    ...(initialData ?? {}),
    empresasIntegrantes: initialData?.empresasIntegrantes ?? [],
  });

  const [catalogos, setCatalogos] = useState(CATALOGOS_INICIALES);

  const [cargandoCatalogos, setCargandoCatalogos] = useState(true);

  const [errorCatalogos, setErrorCatalogos] = useState("");

  const [modalEmpresaIntegranteAbierto, setModalEmpresaIntegranteAbierto] = useState(false);

  const [empresaIntegranteSeleccionada, setEmpresaIntegranteSeleccionada] = useState(null);

  const [guardando, setGuardando] = useState(false);

  const [errorGuardado, setErrorGuardado] = useState("");

  useEffect(() => {
    let componenteActivo = true;

    const cargarCatalogos = async () => {
      setCargandoCatalogos(true);
      setErrorCatalogos("");

      try {
        const catalogosObtenidos = await obtenerCatalogosEmpresa(token);

        if (!componenteActivo) {
          return;
        }

        setCatalogos(catalogosObtenidos);
      } catch (error) {
        if (!componenteActivo) {
          return;
        }

        setCatalogos(CATALOGOS_INICIALES);
        setErrorCatalogos(error.message || "No fue posible obtener los datos necesarios para completar la empresa.");
      } finally {
        if (componenteActivo) {
          setCargandoCatalogos(false);
        }
      }
    };

    cargarCatalogos();

    return () => {
      componenteActivo = false;
    };
  }, [token]);

  const mostrarEmpresasIntegrantes = TIPOS_SOCIEDAD_CON_INTEGRANTES.includes(String(datos.tipoSociedadId ?? ""));

  const actualizarValor = (campo, value) => {
    setErrorGuardado("");

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
    setErrorGuardado("");

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
    setErrorGuardado("");

    setDatos((prev) => ({
      ...prev,
      empresasIntegrantes: (prev.empresasIntegrantes ?? []).filter((empresa) => empresa.id !== empresaId),
    }));
  };

  const cuitNormalizado = String(cuit ?? "").replace(/\D/g, "");

  const cuitValido = cuitEsValido(cuitNormalizado);

  const correoValido = datos.email.trim().length > 0 && datos.email.trim().length <= 254 && emailEsValido(datos.email);

  const telefonoValido = telefonoEsValido(datos.telefono);

  const numeroValido = /^\d{1,20}$/.test(datos.numero.trim());

  const pisoValido = pisoEsValido(datos.piso);

  const localidadSeleccionada = LOCALIDADES_MOCK.find((localidad) => String(localidad.value) === String(datos.localidadId));

  const integrantesValidas = (datos.empresasIntegrantes ?? []).every(
    (empresa) =>
      cuitEsValido(empresa.cuit) &&
      String(empresa.razonSocial ?? "").trim().length > 0 &&
      String(empresa.razonSocial ?? "").trim().length <= 150 &&
      identificadorEsValido(empresa.tipoSociedadId),
  );

  const empresasIntegrantesValidas = !mostrarEmpresasIntegrantes || ((datos.empresasIntegrantes ?? []).length > 0 && integrantesValidas);

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

  const longitudesValidas =
    datos.razonSocial.trim().length <= 150 &&
    datos.calle.trim().length <= 150 &&
    datos.departamento.trim().length <= 20 &&
    datos.codigoPostal.trim().length <= 10 &&
    datos.provincia.trim().length <= 100;

  const catalogosValidos =
    identificadorEsValido(datos.actividadId) && identificadorEsValido(datos.caracter) && identificadorEsValido(datos.tipoSociedadId);

  const formularioValido =
    cuitValido &&
    camposObligatoriosCompletos &&
    longitudesValidas &&
    catalogosValidos &&
    numeroValido &&
    pisoValido &&
    Boolean(localidadSeleccionada) &&
    correoValido &&
    telefonoValido &&
    empresasIntegrantesValidas &&
    !cargandoCatalogos &&
    !errorCatalogos;

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (!formularioValido || guardando) {
      return;
    }

    const empresasIntegrantes = mostrarEmpresasIntegrantes ? (datos.empresasIntegrantes ?? []) : [];

    const request = {
      cuit: cuitNormalizado,
      razonSocial: datos.razonSocial.trim(),
      actividadId: Number(datos.actividadId),
      caracterId: Number(datos.caracter),
      tipoSociedadId: Number(datos.tipoSociedadId),
      calle: datos.calle.trim(),
      numero: datos.numero.trim(),
      piso: datos.piso.trim() ? Number(datos.piso) : null,
      departamentoOficina: datos.departamento.trim() || null,
      codigoPostal: datos.codigoPostal.trim(),
      provincia: datos.provincia.trim(),
      localidad: localidadSeleccionada.label.trim(),
      correo: datos.email.trim(),
      telefono: datos.telefono.trim() || null,
      empresasIntegrantes: empresasIntegrantes.map((empresa) => ({
        cuit: String(empresa.cuit ?? "").replace(/\D/g, ""),
        razonSocial: String(empresa.razonSocial ?? "").trim(),
        tipoSociedadId: Number(empresa.tipoSociedadId),
      })),
    };

    setGuardando(true);
    setErrorGuardado("");

    try {
      const resultado = await guardarEmpresaSolicitud(request, token);

      onNext?.({
        ...datos,
        empresasIntegrantes,
        solicitudId: resultado?.solicitudId ?? resultado?.SolicitudId ?? null,
      });
    } catch (error) {
      setErrorGuardado(error.message || "No fue posible guardar la información de la empresa.");
    } finally {
      setGuardando(false);
    }
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
                maxLength={150}
                required
                onChange={actualizarCampo}
              />
            </div>

            <div className="empresa-col-6">
              <SearchableSelect
                id="actividad"
                label="Actividad de la empresa"
                value={datos.actividadId}
                options={catalogos.actividadesEmpresa}
                placeholder={cargandoCatalogos ? "Cargando actividades..." : "Actividad de la organización"}
                required
                disabled={cargandoCatalogos || Boolean(errorCatalogos)}
                onChange={(value) => actualizarValor("actividadId", value)}
              />
            </div>

            <div className="empresa-col-6">
              <SearchableSelect
                id="caracter-empresa"
                label="Carácter"
                value={datos.caracter}
                options={catalogos.caracteresEmpresa}
                placeholder={cargandoCatalogos ? "Cargando caracteres..." : "Carácter de la organización"}
                required
                disabled={cargandoCatalogos || Boolean(errorCatalogos)}
                onChange={(value) => actualizarValor("caracter", value)}
              />
            </div>

            <div className="empresa-col-6">
              <SearchableSelect
                id="tipo-sociedad"
                label="Tipo de sociedad"
                value={datos.tipoSociedadId}
                options={catalogos.tiposSociedad}
                placeholder={cargandoCatalogos ? "Cargando tipos de sociedad..." : "Busque y seleccione el tipo de sociedad"}
                required
                disabled={cargandoCatalogos || Boolean(errorCatalogos)}
                onChange={(value) => actualizarValor("tipoSociedadId", value)}
              />
            </div>
          </div>

          {errorCatalogos && (
            <span className="empresa-field-error" role="alert">
              {errorCatalogos}
            </span>
          )}
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
                maxLength={150}
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
                className={["empresa-input", datos.numero && !numeroValido ? "has-error" : ""].filter(Boolean).join(" ")}
                type="text"
                name="numero"
                value={datos.numero}
                placeholder="Número"
                inputMode="numeric"
                maxLength={20}
                required
                onChange={actualizarCampo}
              />

              {datos.numero && !numeroValido && <span className="empresa-field-error">Ingrese solamente números.</span>}
            </div>

            <div className="empresa-col-2">
              <label className="form-field-label" htmlFor="piso">
                Piso
              </label>

              <input
                id="piso"
                className={["empresa-input", !pisoValido ? "has-error" : ""].filter(Boolean).join(" ")}
                type="text"
                name="piso"
                value={datos.piso}
                placeholder="Piso"
                inputMode="numeric"
                maxLength={3}
                onChange={actualizarCampo}
              />

              {!pisoValido && <span className="empresa-field-error">Ingrese un número entre 0 y 255.</span>}
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
                maxLength={20}
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
                maxLength={10}
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
                maxLength={100}
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
                maxLength={254}
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
                maxLength={30}
                onChange={actualizarCampo}
              />

              {!telefonoValido && <span className="empresa-field-error">Ingrese un teléfono válido.</span>}
            </div>
          </div>
        </section>

        {UBICAR_EMPRESAS_INTEGRANTES_AL_FINAL && bloqueEmpresasIntegrantes}

        {mostrarEmpresasIntegrantes && (datos.empresasIntegrantes ?? []).length === 0 && (
          <span className="empresa-field-error" role="alert">
            Debe agregar al menos una empresa integrante.
          </span>
        )}

        {!cuitValido && (
          <span className="empresa-field-error" role="alert">
            No se encontró un CUIT principal válido para guardar la solicitud.
          </span>
        )}

        <div className="empresa-required-note" role="note">
          <Info size={15} aria-hidden="true" />

          <span>Los campos marcados con * son obligatorios</span>
        </div>

        {errorGuardado && (
          <span className="empresa-field-error" role="alert">
            {errorGuardado}
          </span>
        )}

        <div className="empresa-form-actions">
          <button type="button" className="empresa-back-button" disabled>
            Volver
          </button>

          {formularioValido && (
            <button type="submit" className="next-step-button" disabled={guardando}>
              {guardando ? "Guardando..." : "Continuar"}
            </button>
          )}
        </div>
      </form>

      {modalEmpresaIntegranteAbierto && (
        <EmpresaIntegranteModal
          key={empresaIntegranteSeleccionada?.id ?? "nueva-empresa"}
          initialData={empresaIntegranteSeleccionada}
          tiposSociedad={catalogos.tiposSociedad}
          onClose={cerrarModalEmpresaIntegrante}
          onSave={guardarEmpresaIntegrante}
        />
      )}
    </>
  );
}
