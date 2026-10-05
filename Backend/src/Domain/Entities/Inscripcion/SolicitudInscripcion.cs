using System;
using System.Collections.Generic;
using System.Linq;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.SeedWork;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Entities.Inscripcion
{
    public class SolicitudInscripcion : IAggregateRoot
    {
        private readonly List<EmpresaIntegrante>
            _empresasIntegrantes =
                new List<EmpresaIntegrante>();

        public long Id { get; private set; }

        public Guid UsuarioId { get; private set; }

        public Empresa EmpresaPrincipal { get; private set; }

        public string Comentarios { get; private set; }

        public IReadOnlyCollection<EmpresaIntegrante>
            EmpresasIntegrantes =>
                _empresasIntegrantes.AsReadOnly();

        private SolicitudInscripcion()
        {
        }

        private SolicitudInscripcion(
            Guid usuarioId,
            Empresa empresaPrincipal,
            string comentarios
        )
        {
            if (usuarioId == Guid.Empty)
            {
                throw new SumariosDomainException(
                    "La solicitud debe estar asociada a un usuario."
                );
            }

            if (empresaPrincipal == null)
            {
                throw new SumariosDomainException(
                    "La solicitud debe tener una empresa principal."
                );
            }

            if (!empresaPrincipal.TieneDatosCompletos)
            {
                throw new SumariosDomainException(
                    "La empresa principal no tiene completos " +
                    "los datos requeridos para la inscripción."
                );
            }

            UsuarioId = usuarioId;
            EmpresaPrincipal = empresaPrincipal;
            Comentarios = NormalizarComentarios(comentarios);
        }

        public static SolicitudInscripcion Crear(
            Guid usuarioId,
            Empresa empresaPrincipal,
            string comentarios = null
        )
        {
            return new SolicitudInscripcion(
                usuarioId,
                empresaPrincipal,
                comentarios
            );
        }

        public void AgregarEmpresaIntegrante(
            Empresa empresaIntegrante
        )
        {
            if (empresaIntegrante == null)
            {
                throw new SumariosDomainException(
                    "Debe indicar la empresa integrante."
                );
            }

            long cuitIntegrante =
                empresaIntegrante.Cuit.ToInt64();

            bool yaExiste = _empresasIntegrantes.Any(
                relacion =>
                    relacion
                        .CuitEmpresaIntegrante
                        .ToInt64() ==
                    cuitIntegrante
            );

            if (yaExiste)
            {
                throw new SumariosDomainException(
                    "La empresa integrante ya fue agregada " +
                    "a la solicitud."
                );
            }

            EmpresaIntegrante relacion =
                EmpresaIntegrante.Vincular(
                    EmpresaPrincipal,
                    empresaIntegrante
                );

            _empresasIntegrantes.Add(relacion);
        }

        public void QuitarEmpresaIntegrante(
            Cuit cuitEmpresaIntegrante
        )
        {
            if (cuitEmpresaIntegrante == null)
            {
                throw new SumariosDomainException(
                    "Debe indicar el CUIT de la empresa integrante."
                );
            }

            EmpresaIntegrante relacion =
                _empresasIntegrantes.FirstOrDefault(
                    empresa =>
                        empresa
                            .CuitEmpresaIntegrante
                            .ToInt64() ==
                        cuitEmpresaIntegrante.ToInt64()
                );

            if (relacion == null)
            {
                throw new SumariosDomainException(
                    "La empresa integrante indicada " +
                    "no pertenece a la solicitud."
                );
            }

            _empresasIntegrantes.Remove(relacion);
        }

        public void ActualizarComentarios(
            string comentarios
        )
        {
            Comentarios = NormalizarComentarios(comentarios);
        }

        public void AsignarId(long id)
        {
            if (id <= 0)
            {
                throw new SumariosDomainException(
                    "El identificador de la solicitud no es válido."
                );
            }

            if (Id != 0)
            {
                throw new SumariosDomainException(
                    "La solicitud ya tiene un identificador asignado."
                );
            }

            Id = id;
        }

        private static string NormalizarComentarios(
            string comentarios
        )
        {
            if (string.IsNullOrWhiteSpace(comentarios))
            {
                return null;
            }

            return comentarios.Trim();
        }
    }
}