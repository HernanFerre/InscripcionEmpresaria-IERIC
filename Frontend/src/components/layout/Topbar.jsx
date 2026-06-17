import { User, Bell } from "lucide-react";

export default function Topbar() {
  return (
    <header className="topbar">
      <div className="topbar-left">
        <strong className="brand">IERIC</strong>
        <span className="divider"></span>
        <span className="portal-title">Portal de inscripción empresarial</span>
      </div>

      <div className="topbar-right">
        <div className="user-icon">
          <User size={30} />
        </div>

        <div className="user-info">
          <span>Hernan</span>
          <strong>Admin IERIC</strong>
        </div>

        <div className="bell-wrapper">
          <Bell size={28} />
          <span className="notification-dot"></span>
        </div>
      </div>
    </header>
  );
}
