import { useEffect } from "react";
import { isTokenExpired, parseJwt } from "./authenticationClient";

export default function AuthIframe({ authenticationUrl, visible, nuevoUsuario = false, onClose, onLoginSuccess, onLoginExpired }) {
  const iframeSrc = `${authenticationUrl}/LoginExternoRegistrado.html?publica=true${nuevoUsuario ? "&nuevo=true" : ""}`;

  useEffect(() => {
    if (!visible) return;

    const handleMessage = (event) => {
      const expectedOrigin = new URL(authenticationUrl).origin;

      if (event.origin !== expectedOrigin) {
        return;
      }

      try {
        const data = event.data;

        if (data?.source !== "ieric-authentication") {
          return;
        }

        const token = data?.token;

        if (!token) {
          return;
        }

        const profile = parseJwt(token);

        if (isTokenExpired(profile)) {
          onLoginExpired?.();
          return;
        }

        onLoginSuccess?.(token, profile);
        onClose?.();
      } catch (error) {
        console.error("Error procesando autenticación:", error);
      }
    };

    window.addEventListener("message", handleMessage);

    return () => {
      window.removeEventListener("message", handleMessage);
    };
  }, [visible, authenticationUrl, onClose, onLoginSuccess, onLoginExpired]);

  if (!visible) return null;

  return (
    <div className="auth-iframe-overlay">
      <iframe title="Autenticación de usuarios" src={iframeSrc} className="auth-iframe" />
    </div>
  );
}
