import { useState } from "react";

import AuthIframe from "./AuthIframe.jsx";
import { parseJwt } from "./authenticationClient.js";

export default function IericAuth({ children, onAuthenticated }) {
  const [token, setToken] = useState(null);
  const [profile, setProfile] = useState(null);

  const [mostrarLogin, setMostrarLogin] = useState(false);
  const [nuevoUsuario, setNuevoUsuario] = useState(false);

  const [verificandoSesion, setVerificandoSesion] = useState(true);

  const authenticationUrl = import.meta.env.VITE_AUTHENTICATION_URL;

  const abrirLogin = () => {
    setVerificandoSesion(false);
    setNuevoUsuario(false);
    setMostrarLogin(true);
  };

  const cambiarUsuario = () => {
    setToken(null);
    setProfile(null);
    localStorage.removeItem("token");

    setVerificandoSesion(false);
    setNuevoUsuario(true);
    setMostrarLogin(true);
  };

  const cerrarLogin = () => {
    setMostrarLogin(false);
  };

  const cerrarVerificacionSilenciosa = () => {
    setVerificandoSesion(false);
  };

  const handleLoginSuccess = (tokenRecibido) => {
    setToken(tokenRecibido);

    try {
      const profileDecodificado = parseJwt(tokenRecibido);
      setProfile(profileDecodificado);
    } catch (error) {
      console.error("No se pudo decodificar el token:", error);
      setProfile(null);
    }

    setMostrarLogin(false);
    setVerificandoSesion(false);

    onAuthenticated?.(tokenRecibido);
  };

  const usuario = token
    ? {
        token,
        profile,
      }
    : null;

  return (
    <>
      {children({
        token,
        usuario,
        profile,
        estaLogueado: Boolean(token),
        abrirLogin,
        cambiarUsuario,
      })}

      <AuthIframe
        authenticationUrl={authenticationUrl}
        visible={verificandoSesion && !token}
        nuevoUsuario={false}
        silent
        onClose={cerrarVerificacionSilenciosa}
        onLoginSuccess={handleLoginSuccess}
        onLoginExpired={() => {
          cerrarVerificacionSilenciosa();
        }}
      />

      <AuthIframe
        authenticationUrl={authenticationUrl}
        visible={mostrarLogin}
        nuevoUsuario={nuevoUsuario}
        onClose={cerrarLogin}
        onLoginSuccess={handleLoginSuccess}
        onLoginExpired={() => {
          alert("Su permiso ha expirado. Debe iniciar sesión nuevamente.");
          cambiarUsuario();
        }}
      />
    </>
  );
}
