import { User, Bell } from "lucide-react";

export default function Topbar({ usuario, onAbrirLogin, onCambiarUsuario }) {
  const nombreUsuario =
    usuario?.profile?.nombreUsuario ||
    usuario?.profile?.name ||
    usuario?.profile?.unique_name ||
    usuario?.profile?.email ||
    "Usuario logueado";

  return (
    <header className="topbar">
      <div className="topbar-left">
        <strong className="brand">IERIC</strong>
        <span className="divider"></span>
        <span className="portal-title">Portal de inscripción empresaria</span>
      </div>

      <div className="topbar-right">
        <div className="user-icon">
          <User size={30} />
        </div>

        {usuario ? (
          <>
            <span className="topbar-user-name">{nombreUsuario}</span>

            <button type="button" onClick={onCambiarUsuario}>
              Ingresar con otro usuario
            </button>
          </>
        ) : (
          <button type="button" onClick={onAbrirLogin}>
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
