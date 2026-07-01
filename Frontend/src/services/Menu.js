// window.addEventListener(
//             "message",

//             (e) => {
//                 var origin = e.origin;

//                 var data = e.data;

//                 if (origin == AUTHENTICATION_URL) {
//                     // && data.source == "ospecon-authentication") {
//                     //&& data.source == "ospecon-authentication") {

//                     try {
//                         this.popUp.close();

//                         document.getElementsByName("iframeLogin")[0].style.display = "none";

//                         const profile = this.parseJwt(data);
//                         if (profile.exp < new Date().getTime() / 1000) {
//                             store.dispatch(showConfirm("Control de Accesos", "Su permiso ha expirado, ¿ quiere actualizalo ?", loguearConNuevoUsuario(), null));
//                             return;
//                         } else {
//                             this.logueado = true;
//                             store.dispatch(autorizacion(data.token));
//                         }
//                     } catch (err) {
//                         console.log(err);
//                     }
//                 }
//             },

//             false,
//         );

//  abrir(e) {
//         if (this.profile == "ACCEDER") {
//             document.getElementsByName("iframeLogin")[0].style.display = "";
//             this.popUp = window.open(AUTHENTICATION_URL + "/index.html", "iframeLogin");
//         }
//     }
//     abrirForzado(e) {
//         store.dispatch(goTo(HOME_SCREEN));
//         document.getElementsByName("iframeLogin")[0].style.display = "";
//         this.popUp = window.open(AUTHENTICATION_URL + "/index.html?nuevo=true", "iframeLogin");
//     }

// parseJwt(data) {
//         var base64Url = data.token.split(".")[1];
//         var base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
//         var jsonPayload = decodeURIComponent(
//             window
//                 .atob(base64)
//                 .split("")
//                 .map(function (c) {
//                     return "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2);
//                 })
//                 .join(""),
//         );

//         return JSON.parse(jsonPayload);
//     }
