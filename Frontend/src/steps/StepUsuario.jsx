import { useState } from "react";
import { Eye, EyeOff, Lock, User, CheckCircle, XCircle } from "lucide-react";

import { crearUsuarioPortal } from "../services/inscripcionService.js";

export default function StepUsuario() {
  const [usuario, setUsuario] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [aceptaTerminos, setAceptaTerminos] = useState(false);
  const [mostrarPassword, setMostrarPassword] = useState(false);
  const [creando, setCreando] = useState(false);
  const [usuarioCreado, setUsuarioCreado] = useState(false);

  const passwordChecks = {
    length: password.length >= 8,
    uppercase: /[A-Z]/.test(password),
    lowercase: /[a-z]/.test(password),
    number: /\d/.test(password),
    special: /[^A-Za-z0-9]/.test(password),
  };

  const passwordValida = Object.values(passwordChecks).every(Boolean);
  const usuarioValido = usuario.length >= 4 && !usuario.includes(" ");
  const passwordsCoinciden = password && password === confirmPassword;

  const puedeCrearUsuario = usuarioValido && passwordValida && passwordsCoinciden && aceptaTerminos;

  const handleCrearUsuario = async () => {
    if (!puedeCrearUsuario) return;

    setCreando(true);

    const resultado = await crearUsuarioPortal({
      usuario,
      password,
    });

    setCreando(false);

    if (resultado.ok) {
      setUsuarioCreado(true);
    }
  };

  return (
    <>
      <h1>CREACIÓN DE USUARIO</h1>

      <section className="user-step-card">
        <p className="user-step-subtitle">Defina sus credenciales de acceso para el portal</p>

        <div className="user-input-row">
          <span>
            <User size={19} />
          </span>

          <input
            value={usuario}
            onChange={(event) => setUsuario(event.target.value)}
            placeholder="Usuario"
            disabled={usuarioCreado}
            autoComplete="off"
          />
        </div>

        <div className="user-input-row">
          <span>
            <Lock size={19} />
          </span>

          <input
            type={mostrarPassword ? "text" : "password"}
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="Contraseña"
            disabled={usuarioCreado}
            autoComplete="new-password"
          />

          <button type="button" className="password-toggle" onClick={() => setMostrarPassword((prev) => !prev)}>
            {mostrarPassword ? <EyeOff size={20} /> : <Eye size={20} />}
          </button>
        </div>

        <div className="user-input-row">
          <span>
            <Lock size={19} />
          </span>

          <input
            type={mostrarPassword ? "text" : "password"}
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            placeholder="Confirmar contraseña"
            disabled={usuarioCreado}
            autoComplete="new-password"
          />
        </div>

        <div className="password-rules">
          <PasswordRule valid={passwordChecks.length} text="Mínimo 8 caracteres" />
          <PasswordRule valid={passwordChecks.uppercase} text="Al menos una mayúscula" />
          <PasswordRule valid={passwordChecks.number} text="Al menos un número" />
          <PasswordRule valid={passwordChecks.special} text="Al menos un carácter especial" />
        </div>

        {confirmPassword && (
          <p className={passwordsCoinciden ? "user-success" : "user-error"}>
            {passwordsCoinciden ? "Las contraseñas coinciden" : "Las contraseñas no coinciden"}
          </p>
        )}

        <label className="terms-row">
          <input
            type="checkbox"
            checked={aceptaTerminos}
            disabled={usuarioCreado}
            onChange={(event) => setAceptaTerminos(event.target.checked)}
          />

          <span>Acepto términos y condiciones</span>
        </label>

        {!usuarioCreado && (
          <button className="create-user-button" onClick={handleCrearUsuario} disabled={!puedeCrearUsuario || creando}>
            {creando ? "Creando usuario..." : "Crear usuario y acceder"}
          </button>
        )}

        {usuarioCreado && (
          <p className="user-created-message">
            <CheckCircle size={22} />
            Usuario creado correctamente
          </p>
        )}
      </section>
    </>
  );
}

function PasswordRule({ valid, text }) {
  return (
    <div className={valid ? "password-rule valid" : "password-rule invalid"}>
      {valid ? <CheckCircle size={16} /> : <XCircle size={16} />}
      <span>{text}</span>
    </div>
  );
}
