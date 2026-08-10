export const CONFIGURACION_DOCUMENTOS_MOCK = {
  formatosAceptados: [".pdf"],
  tiposMimeAceptados: ["application/pdf"],
  tamanoMaximoMb: 10,
};

export const SECCIONES_DOCUMENTACION_MOCK = [
  {
    id: "solicitud-inscripcion",
    titulo: "Documentación requerida para la solicitud de inscripción",
    abiertaInicialmente: true,
    documentos: [
      {
        id: "contrato-social",
        titulo: "Contrato social",
        obligatorio: true,
        archivo: null,
      },
      {
        id: "acta-directorio",
        titulo: "Acta Directorio",
        obligatorio: true,
        archivo: null,
      },
      {
        id: "constancia-domicilio",
        titulo: "Constancia domicilio",
        obligatorio: true,
        archivo: null,
      },
    ],
  },
  {
    id: "nomina-construccion",
    titulo: "Documentación requerida para la gestión de nómina de obreros de la construcción",
    abiertaInicialmente: true,
    documentos: [
      {
        id: "nomina-sanchez-patricia",
        titulo: "Nómina: Sanchez Patricia",
        obligatorio: true,
        archivo: null,
      },
    ],
  },
  {
    id: "representantes-apoderados",
    titulo: "Documentación requerida para la gestión de representantes/apoderados",
    abiertaInicialmente: false,
    documentos: [],
  },
];
