using System;
using System.Collections.Generic;
using System.Linq;
using IERIC.SumariosIERIC.Application.Exceptions;
using IERIC.SumariosIERIC.Application.Inscripcion.Models;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Application.Inscripcion.Validation
{
    public static class GuardarRepresentantesSolicitudValidator
    {
        public static void Validar(
            long solicitudId,
            GuardarRepresentantesSolicitudRequest request
        )
        {
            if (solicitudId <= 0)
            {
                LanzarError(
                    "La solicitud indicada no es válida."
                );
            }

            if (request == null)
            {
                LanzarError(
                    "La información de representantes no puede estar vacía."
                );
            }

            List<RepresentanteSolicitudRequest> representantes =
                request.Representantes ??
                new List<RepresentanteSolicitudRequest>();

            HashSet<string> representantesUnicos =
                new HashSet<string>();

            foreach (
                RepresentanteSolicitudRequest representante
                in representantes
            )
            {
                if (representante == null)
                {
                    LanzarError(
                        "La información de un representante no es válida."
                    );
                }

                string identificacion =
                    IdentificarRepresentante(representante);

                try
                {
                    if (representante.EmpresaId <= 0)
                    {
                        LanzarError(
                            "Debe seleccionar la empresa del representante."
                        );
                    }

                    long cuil = ObtenerCuilNormalizado(
                        representante.Cuil
                    );

                    string clave =
                        representante.EmpresaId + ":" + cuil;

                    if (!representantesUnicos.Add(clave))
                    {
                        LanzarError(
                            "No puede agregar dos veces el mismo representante " +
                            "para una empresa."
                        );
                    }

                    ValidarTextoObligatorio(
                        representante.Apellido,
                        "apellido",
                        100
                    );

                    ValidarTextoObligatorio(
                        representante.Nombre,
                        "nombre",
                        100
                    );

                    if (representante.TipoRepresentanteId <= 0)
                    {
                        LanzarError(
                            "Debe seleccionar el cargo del representante."
                        );
                    }

                    ValidarTextoObligatorio(
                        representante.Correo,
                        "correo electrónico",
                        254
                    );

                    ValidarTextoObligatorio(
                        representante.Telefono,
                        "teléfono",
                        30
                    );
                }
                catch (InvalidException exception)
                {
                    throw new InvalidException(
                        $"{identificacion}: {exception.Message}",
                        exception.Solution,
                        exception
                    );
                }
            }
        }

        public static long ObtenerCuilNormalizado(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                LanzarError(
                    "Debe informar el CUIL del representante."
                );
            }

            bool contieneCaracterInvalido =
                valor.Any(
                    caracter =>
                        !char.IsDigit(caracter) &&
                        caracter != '-' &&
                        !char.IsWhiteSpace(caracter)
                );

            if (contieneCaracterInvalido)
            {
                LanzarError(
                    "El CUIL del representante contiene caracteres inválidos."
                );
            }

            string cuilNormalizado =
                new string(
                    valor
                        .Where(char.IsDigit)
                        .ToArray()
                );

            if (cuilNormalizado.Length != 11)
            {
                LanzarError(
                    "El CUIL del representante debe contener 11 números."
                );
            }

            if (
                !long.TryParse(
                    cuilNormalizado,
                    out long numeroCuil
                )
            )
            {
                LanzarError(
                    "El CUIL del representante no es válido."
                );
            }

            try
            {
                new Cuil(numeroCuil);
            }
            catch (SumariosDomainException)
            {
                LanzarError(
                    "El CUIL del representante no es válido."
                );
            }

            return numeroCuil;
        }

        private static string IdentificarRepresentante(
            RepresentanteSolicitudRequest representante
        )
        {
            string nombreCompleto = string.Join(
                " ",
                new[]
                {
                    representante.Nombre,
                    representante.Apellido
                }
                    .Where(valor => !string.IsNullOrWhiteSpace(valor))
                    .Select(valor => valor.Trim())
            );

            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                nombreCompleto = "Representante sin nombre";
            }

            string cuil =
                string.IsNullOrWhiteSpace(representante.Cuil)
                    ? "sin informar"
                    : representante.Cuil.Trim();

            return $"{nombreCompleto} (CUIL {cuil})";
        }

        private static void ValidarTextoObligatorio(
            string valor,
            string nombreCampo,
            int longitudMaxima
        )
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                LanzarError(
                    $"El campo {nombreCampo} es obligatorio."
                );
            }

            if (valor.Trim().Length > longitudMaxima)
            {
                LanzarError(
                    $"El campo {nombreCampo} supera la longitud permitida."
                );
            }
        }

        private static void LanzarError(string mensaje)
        {
            throw new InvalidException(
                mensaje,
                "Revise los datos ingresados."
            );
        }
    }
}