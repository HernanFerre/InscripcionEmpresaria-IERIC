import { useState } from "react";

import { ArrowUp, ChevronLeft, ChevronRight, Info, Pencil, Plus, Trash2 } from "lucide-react";

import RepresentanteModal from "../../components/modals/RepresentanteModal.jsx";
import { guardarRepresentantesSolicitud } from "../../services/InscripcionService.js";
import { esCuitValido, formatCuit, normalizarCuit } from "../../utils/formatters.js";

import "../../styles/stepRepresentantes.css";

/*
 * Identificadores provenientes del catálogo de tipos de sociedad:
 * 8  = U.T.E.
 * 26 = Consorcio de cooperación
 * 28 = U.T.
 */
const TIPOS_SOCIEDAD_CON_INTEGRANTES = ["8", "26", "28"];

function formatearCuil(value) {
  const cuil = normalizarCuit(value);

  return cuil.length > 11 ? cuil : formatCuit(cuil).replace(/-/g, " ");
}

function identificarRepresentante(representante) {
  const nombreCompleto = [representante.nombre, representante.apellido]
    .map((value) => String(value ?? "").trim())
    .filter(Boolean)
    .join(" ");

  const cuil = formatearCuil(representante.cuil) || "sin informar";

  return `${nombreCompleto || "Representante sin nombre"} (CUIL ${cuil})`;
}

export default function StepRepresentantes({
  token,
  solicitudId = null,
  empresaId = null,
  tipoSociedadId = "",
  empresasIntegrantes = [],
  representantes = [],
  onSave,
  onDelete,
  onBack,
  onNext,
}) {
  const [modalAbierto, setModalAbierto] = useState(false);

  const [representanteSeleccionado, setRepresentanteSeleccionado] = useState(null);

  const [guardando, setGuardando] = useState(false);
  const [errorGuardado, setErrorGuardado] = useState("");

  const esEmpresaConIntegrantes = TIPOS_SOCIEDAD_CON_INTEGRANTES.includes(String(tipoSociedadId ?? ""));

  const hayEmpresasIntegrantes = empresasIntegrantes.length > 0;

  const hayRepresentantes = representantes.length > 0;

  const puedeAgregarRepresentante = esEmpresaConIntegrantes ? hayEmpresasIntegrantes : Number(empresaId) > 0;

  const obtenerNombreEmpresa = (representante) => {
    const empresaActual = empresasIntegrantes.find(
      (empresa) => String(empresa.empresaId ?? empresa.id) === String(representante.empresaId),
    );

    return empresaActual?.razonSocial || representante.empresaNombre || "—";
  };

  const abrirNuevoRepresentante = () => {
    if (!puedeAgregarRepresentante || guardando) {
      return;
    }

    setErrorGuardado("");
    setRepresentanteSeleccionado(null);
    setModalAbierto(true);
  };

  const abrirEdicionRepresentante = (representante) => {
    setErrorGuardado("");
    setRepresentanteSeleccionado(representante);
    setModalAbierto(true);
  };

  const cerrarModal = () => {
    setModalAbierto(false);
    setRepresentanteSeleccionado(null);
  };

  const guardarRepresentante = (datosRepresentante) => {
    setErrorGuardado("");
    onSave?.(datosRepresentante);
    cerrarModal();
  };

  const eliminarRepresentante = (id) => {
    setErrorGuardado("");
    onDelete?.(id);
  };

  const continuar = async () => {
    if (guardando) return;

    setErrorGuardado("");

    const representanteInvalido = representantes.find((representante) => {
      const cuil = normalizarCuit(representante.cuil);

      return cuil.startsWith("0") || !esCuitValido(cuil);
    });

    if (representanteInvalido) {
      const cuil = normalizarCuit(representanteInvalido.cuil);

      const motivo = cuil.length !== 11 ? "el CUIL debe contener 11 números" : "el CUIL no es válido";

      setErrorGuardado(`${identificarRepresentante(representanteInvalido)}: ${motivo}. Edite sus datos para corregirlo.`);

      return;
    }

    const request = representantes.map((representante) => ({
      empresaId: Number(representante.empresaId),
      apellido: String(representante.apellido ?? "").trim(),
      nombre: String(representante.nombre ?? "").trim(),
      cuil: normalizarCuit(representante.cuil),
      tipoRepresentanteId: Number(representante.tipoRepresentanteId ?? representante.cargoId),
      correo: String(representante.email ?? "").trim(),
      telefono: String(representante.telefono ?? "").trim(),
    }));

    setGuardando(true);

    try {
      const resultado = await guardarRepresentantesSolicitud(solicitudId, request, token);

      const guardados = resultado.representantes ?? resultado.Representantes;

      if (guardados.length !== representantes.length) {
        throw new Error("El servicio no confirmó todos los representantes. Intente nuevamente.");
      }

      const representantesConId = representantes.map((representante) => {
        const cuil = normalizarCuit(representante.cuil);

        const guardado = guardados.find(
          (item) =>
            Number(item.empresaId ?? item.EmpresaId) === Number(representante.empresaId) && normalizarCuit(item.cuil ?? item.Cuil) === cuil,
        );

        const representanteId = Number(guardado?.representanteId ?? guardado?.RepresentanteId);

        if (!Number.isSafeInteger(representanteId) || representanteId <= 0) {
          throw new Error(`El servicio no confirmó el identificador de ${identificarRepresentante(representante)}. Intente nuevamente.`);
        }

        return {
          ...representante,
          cuil,
          representanteId,
        };
      });

      onNext?.(representantesConId);
    } catch (error) {
      setErrorGuardado(error.message || "No fue posible guardar los representantes.");
    } finally {
      setGuardando(false);
    }
  };

  return (
    <section className="representantes-step">
      <div className="representantes-header">
        <div>
          <h2 className="section-title representantes-title">Representantes y apoderados</h2>

          <p className="representantes-subtitle">La documentación respaldatoria se solicitará en el apartado Documentación.</p>
        </div>

        <button
          type="button"
          className="representantes-add-button"
          disabled={!puedeAgregarRepresentante || guardando}
          title={puedeAgregarRepresentante ? undefined : "Primero debe cargar una empresa integrante"}
          onClick={abrirNuevoRepresentante}
        >
          <Plus size={18} aria-hidden="true" />
          Agregar representante
        </button>
      </div>

      <div className="representantes-table-wrapper">
        <table
          className={["representantes-table", esEmpresaConIntegrantes ? "representantes-table--con-empresa" : ""].filter(Boolean).join(" ")}
        >
          <thead>
            <tr>
              {esEmpresaConIntegrantes && <th>Empresa</th>}

              <th>
                <span className="representantes-sortable-header">
                  Apellido
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>
                <span className="representantes-sortable-header">
                  Nombre
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>
                <span className="representantes-sortable-header">
                  CUIL
                  <ArrowUp size={15} aria-hidden="true" />
                </span>
              </th>

              <th>Correo electrónico</th>
              <th>Teléfono</th>
              <th>Cargo</th>
            </tr>
          </thead>

          <tbody>
            {!hayRepresentantes && (
              <tr className="representantes-placeholder-row">
                {esEmpresaConIntegrantes && <td>Empresa integrante</td>}

                <td>Apellido</td>
                <td>Nombre</td>
                <td>XX XXXXXXXX X</td>
                <td>ejemplo@correo.com</td>
                <td>+XX XX XXXX-XXXX</td>
                <td>Cargo</td>
              </tr>
            )}

            {representantes.map((representante) => (
              <tr key={representante.id}>
                {esEmpresaConIntegrantes && <td>{obtenerNombreEmpresa(representante)}</td>}

                <td>{representante.apellido}</td>
                <td>{representante.nombre}</td>

                <td title={formatearCuil(representante.cuil)}>{formatearCuil(representante.cuil)}</td>

                <td>{representante.email}</td>
                <td>{representante.telefono}</td>

                <td>
                  <div className="representantes-cargo-cell">
                    <span>{representante.cargo}</span>

                    <div className="representantes-row-actions">
                      <button
                        type="button"
                        className="representantes-edit-button"
                        disabled={guardando}
                        aria-label={`Editar a ${representante.nombre} ${representante.apellido}`}
                        onClick={() => abrirEdicionRepresentante(representante)}
                      >
                        <Pencil size={18} aria-hidden="true" />
                      </button>

                      <button
                        type="button"
                        className="representantes-delete-button"
                        disabled={guardando}
                        aria-label={`Eliminar a ${representante.nombre} ${representante.apellido}`}
                        onClick={() => eliminarRepresentante(representante.id)}
                      >
                        <Trash2 size={18} aria-hidden="true" />
                      </button>
                    </div>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {!hayRepresentantes && (
        <div className="representantes-info" role="status">
          <Info size={15} aria-hidden="true" />

          <span>
            {esEmpresaConIntegrantes && !hayEmpresasIntegrantes
              ? "Debe volver al paso Empresa y cargar al menos una empresa integrante."
              : "Todavía no se agregaron representantes."}
          </span>
        </div>
      )}

      <div className="representantes-pagination">
        <span>{hayRepresentantes ? `1-${representantes.length} de ${representantes.length}` : "0 de 0"}</span>

        <button type="button" aria-label="Página anterior" disabled>
          <ChevronLeft size={17} aria-hidden="true" />
        </button>

        <button type="button" aria-label="Página siguiente" disabled>
          <ChevronRight size={17} aria-hidden="true" />
        </button>
      </div>

      <div className="representantes-footer-actions">
        <button type="button" className="representantes-back-button" disabled={guardando} onClick={onBack}>
          Volver
        </button>

        <button type="button" className="next-step-button" disabled={guardando || modalAbierto} onClick={continuar}>
          {guardando ? "Guardando..." : "Continuar"}
        </button>
      </div>

      {errorGuardado && (
        <div className="empresa-field-error" role="alert">
          {errorGuardado}
        </div>
      )}

      {modalAbierto && (
        <RepresentanteModal
          key={representanteSeleccionado?.id ?? "nuevo-representante"}
          token={token}
          initialData={representanteSeleccionado}
          requiereEmpresaIntegrante={esEmpresaConIntegrantes}
          empresaIdPrincipal={empresaId}
          tipoSociedadIdPrincipal={tipoSociedadId}
          empresasIntegrantes={empresasIntegrantes}
          onClose={cerrarModal}
          onSave={guardarRepresentante}
        />
      )}
    </section>
  );
}
