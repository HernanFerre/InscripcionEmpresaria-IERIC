import { useEffect, useState } from "react";

import AuthIframe from "./AuthIframe.jsx";
import { parseJwt } from "./authenticationClient.js";
import { USAR_TOKEN_BACKEND_DESARROLLO } from "../config/featureFlags.js";

export default function IericAuth({ children, onAuthenticated }) {
  const [token, setToken] = useState(null);
  const [profile, setProfile] = useState(null);

  const [mostrarLogin, setMostrarLogin] = useState(false);
  const [nuevoUsuario, setNuevoUsuario] = useState(false);

  const [verificandoSesion, setVerificandoSesion] = useState(true);

  const authenticationUrl = import.meta.env.VITE_AUTHENTICATION_URL;

  const inscripcionApiUrl = (import.meta.env.VITE_INSCRIPCION_API_URL || "").replace(/\/+$/, "");

  const usarTokenBackendDesarrollo = USAR_TOKEN_BACKEND_DESARROLLO;

  const abrirLogin = () => {
    if (usarTokenBackendDesarrollo) {
      window.location.reload();
      return;
    }

    setVerificandoSesion(false);
    setNuevoUsuario(false);
    setMostrarLogin(true);
  };

  const cambiarUsuario = () => {
    setToken(null);
    setProfile(null);
    localStorage.removeItem("token");

    if (usarTokenBackendDesarrollo) {
      window.location.reload();
      return;
    }

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

  useEffect(() => {
    if (!usarTokenBackendDesarrollo) {
      return undefined;
    }

    let componenteActivo = true;

    const autenticarDesdeBackend = async () => {
      try {
        if (!inscripcionApiUrl) {
          throw new Error("No se configuró VITE_INSCRIPCION_API_URL.");
        }

        const response = await fetch(`${inscripcionApiUrl}/v1/autenticacion-desarrollo/token`, {
          method: "GET",
          headers: {
            Accept: "application/json",
          },
          cache: "no-store",
        });

        let data;

        try {
          data = await response.json();
        } catch {
          data = null;
        }

        if (!response.ok) {
          throw new Error(data?.mensaje || data?.message || `La autenticación devolvió el estado ${response.status}.`);
        }

        if (!data?.token) {
          throw new Error("El backend no devolvió un token de autenticación.");
        }

        if (componenteActivo) {
          handleLoginSuccess(data.token);
        }
      } catch (error) {
        if (componenteActivo) {
          console.error("No fue posible iniciar la sesión de desarrollo:", error);
          setVerificandoSesion(false);
        }
      }
    };

    autenticarDesdeBackend();

    return () => {
      componenteActivo = false;
    };
  }, []);

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
        visible={!usarTokenBackendDesarrollo && verificandoSesion && !token}
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
        visible={!usarTokenBackendDesarrollo && mostrarLogin}
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
