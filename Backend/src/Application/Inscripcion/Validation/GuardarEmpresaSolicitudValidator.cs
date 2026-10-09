using System;
using System.Collections.Generic;
using System.Linq;
using IERIC.SumariosIERIC.Application.Exceptions;
using IERIC.SumariosIERIC.Application.Inscripcion.Models;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Application.Inscripcion.Validation
{
    public static class GuardarEmpresaSolicitudValidator
    {
        private static readonly HashSet<int>
            TiposSociedadConIntegrantes =
                new HashSet<int>
                {
                    8,
                    26,
                    28
                };

        public static void Validar(
            GuardarEmpresaSolicitudRequest request
        )
        {
            if (request == null)
            {
                LanzarError(
                    "La información de la empresa no puede estar vacía."
                );
            }

            long cuitPrincipal = ValidarCuit(
                request.Cuit,
                "empresa principal"
            );

            ValidarTextoObligatorio(
                request.RazonSocial,
                "razón social",
                150
            );

            ValidarIdentificador(
                request.ActividadId,
                "actividad"
            );

            ValidarIdentificador(
                request.CaracterId,
                "carácter de la empresa"
            );

            ValidarIdentificador(
                request.TipoSociedadId,
                "tipo de sociedad"
            );

            ValidarTextoObligatorio(
                request.Calle,
                "calle",
                150
            );

            ValidarNumeroObligatorio(
                request.Numero,
                "número",
                20
            );

            ValidarTextoOpcional(
                request.DepartamentoOficina,
                "departamento u oficina",
                20
            );

            ValidarTextoObligatorio(
                request.CodigoPostal,
                "código postal",
                10
            );

            ValidarIdentificador(
                request.IdProvincia.GetValueOrDefault(),
                "provincia"
            );

            ValidarIdentificador(
                request.IdLocalidad.GetValueOrDefault(),
                "localidad"
            );

            ValidarCorreo(
                request.Correo
            );

            ValidarTelefono(
                request.Telefono
            );

            List<EmpresaIntegranteRequest> integrantes =
                request.EmpresasIntegrantes
                ??
                new List<EmpresaIntegranteRequest>();

            bool requiereIntegrantes =
                TiposSociedadConIntegrantes.Contains(
                    request.TipoSociedadId
                );

            if (
                requiereIntegrantes &&
                integrantes.Count == 0
            )
            {
                LanzarError(
                    "Debe informar al menos una empresa integrante."
                );
            }

            if (
                !requiereIntegrantes &&
                integrantes.Count > 0
            )
            {
                LanzarError(
                    "El tipo de sociedad seleccionado no admite " +
                    "empresas integrantes."
                );
            }

            HashSet<long> cuitIntegrantes =
                new HashSet<long>();

            foreach (
                EmpresaIntegranteRequest integrante
                in integrantes
            )
            {
                if (integrante == null)
                {
                    LanzarError(
                        "La información de una empresa integrante " +
                        "no es válida."
                    );
                }

                long cuitIntegrante = ValidarCuit(
                    integrante.Cuit,
                    "empresa integrante"
                );

                if (cuitIntegrante == cuitPrincipal)
                {
                    LanzarError(
                        "La empresa principal no puede agregarse " +
                        "como empresa integrante."
                    );
                }

                if (!cuitIntegrantes.Add(cuitIntegrante))
                {
                    LanzarError(
                        "No puede agregar dos veces la misma " +
                        "empresa integrante."
                    );
                }

                ValidarTextoObligatorio(
                    integrante.RazonSocial,
                    "razón social de la empresa integrante",
                    150
                );

                ValidarIdentificador(
                    integrante.TipoSociedadId,
                    "tipo de sociedad de la empresa integrante"
                );
            }
        }

        public static long ObtenerCuitNormalizado(
            string valor,
            string descripcion
        )
        {
            return ValidarCuit(
                valor,
                descripcion
            );
        }

        private static long ValidarCuit(
            string valor,
            string descripcion
        )
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                LanzarError(
                    $"Debe informar el CUIT de la {descripcion}."
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
                    $"El CUIT de la {descripcion} contiene " +
                    "caracteres inválidos."
                );
            }

            string cuitNormalizado =
                new string(
                    valor
                        .Where(char.IsDigit)
                        .ToArray()
                );

            long numeroCuit = 0;

            if (
                cuitNormalizado.Length != 11 ||
                !long.TryParse(
                    cuitNormalizado,
                    out numeroCuit
                )
            )
            {
                LanzarError(
                    $"El CUIT de la {descripcion} debe contener " +
                    "11 números."
                );
            }

            try
            {
                new Cuit(
                    numeroCuit
                );
            }
            catch (SumariosDomainException)
            {
                LanzarError(
                    $"El CUIT de la {descripcion} no es válido."
                );
            }

            return numeroCuit;
        }

        private static void ValidarIdentificador(
            int valor,
            string nombreCampo
        )
        {
            if (valor <= 0)
            {
                LanzarError(
                    $"Debe seleccionar {nombreCampo}."
                );
            }
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

        private static void ValidarTextoOpcional(
            string valor,
            string nombreCampo,
            int longitudMaxima
        )
        {
            if (
                !string.IsNullOrWhiteSpace(valor) &&
                valor.Trim().Length > longitudMaxima
            )
            {
                LanzarError(
                    $"El campo {nombreCampo} supera la longitud permitida."
                );
            }
        }

        private static void ValidarNumeroObligatorio(
            string valor,
            string nombreCampo,
            int longitudMaxima
        )
        {
            ValidarTextoObligatorio(
                valor,
                nombreCampo,
                longitudMaxima
            );

            if (!valor.Trim().All(char.IsDigit))
            {
                LanzarError(
                    $"El campo {nombreCampo} debe contener " +
                    "solamente números."
                );
            }
        }

        private static void ValidarCorreo(
            string correo
        )
        {
            ValidarTextoObligatorio(
                correo,
                "correo electrónico",
                254
            );

            try
            {
                Email.FromAddress(
                    correo.Trim()
                );
            }
            catch (ArgumentException)
            {
                LanzarError(
                    "El correo electrónico no tiene un formato válido."
                );
            }
        }

        private static void ValidarTelefono(
            string telefono
        )
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                return;
            }

            ValidarTextoOpcional(
                telefono,
                "teléfono",
                30
            );

            bool contieneCaracterInvalido =
                telefono.Any(
                    caracter =>
                        !char.IsDigit(caracter) &&
                        caracter != '+' &&
                        caracter != '-' &&
                        caracter != '(' &&
                        caracter != ')' &&
                        !char.IsWhiteSpace(caracter)
                );

            string soloNumeros =
                new string(
                    telefono
                        .Where(char.IsDigit)
                        .ToArray()
                );

            if (
                contieneCaracterInvalido ||
                soloNumeros.Length < 8 ||
                soloNumeros.Length > 15
            )
            {
                LanzarError(
                    "El teléfono debe contener entre 8 y 15 números."
                );
            }
        }

        private static void LanzarError(
            string mensaje
        )
        {
            throw new InvalidException(
                mensaje,
                "Revise los datos ingresados."
            );
        }
    }
}