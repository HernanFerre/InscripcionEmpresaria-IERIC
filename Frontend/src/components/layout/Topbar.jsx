import { User, Bell } from "lucide-react";

function formatearCuit(cuit) {
  const numeros = String(cuit ?? "").replace(/\D/g, "");

  if (numeros.length !== 11) {
    return "—";
  }

  return `${numeros.slice(0, 2)}-${numeros.slice(2, 10)}-${numeros.slice(10)}`;
}

export default function Topbar({ usuario, cuit, mostrarDatosInscripcion = false, onAbrirLogin, onCambiarUsuario }) {
  const nombreUsuario = usuario
    ? usuario?.profile?.nombreUsuario ||
      usuario?.profile?.name ||
      usuario?.profile?.unique_name ||
      usuario?.profile?.email ||
      "Usuario logueado"
    : "—";

  const cuitFormateado = formatearCuit(cuit);

  return (
    <header className="topbar">
      <div className="topbar-left">
        <strong className="brand">IERIC</strong>
        <span className="divider"></span>
        <span className="portal-title">SOLICITUD DE INSCRIPCION EMPRESARIA DIGITAL</span>
      </div>

      <div className="topbar-right">
        <div className="user-icon">
          <User size={30} />
        </div>

        {mostrarDatosInscripcion && (
          <div className="topbar-inscripcion-data">
            <span>CUIT: {cuitFormateado}</span>
            <span>Usuario: {nombreUsuario}</span>
          </div>
        )}

        {!mostrarDatosInscripcion && usuario && <span className="topbar-user-name">{nombreUsuario}</span>}

        {usuario ? (
          <button className="topbar-auth-button" type="button" raised="" onClick={onCambiarUsuario}>
            Ingresar con otro usuario
          </button>
        ) : (
          <button className="topbar-auth-button" type="button" raised="" onClick={onAbrirLogin}>
            Acceso Usuarios / Crear Cuenta
          </button>
        )}

        <div className="bell-wrapper">
          <Bell size={28} />
          <span className="notification-dot"></span>
        </div>
      </div>
    </header>
  );
}
