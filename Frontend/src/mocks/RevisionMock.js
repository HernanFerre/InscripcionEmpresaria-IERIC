export const SECCIONES_REVISION_MOCK = [
  {
    id: "estado",
    titulo: "Estado de la solicitud",
    estado: "Pendiente de rectificación",
    descripcion: "Realice las correcciones solicitadas por IERIC para continuar con el proceso.",
  },
  {
    id: "firma",
    titulo: "Firma de solicitud",
    estado: "Pendiente de firma",
    descripcion: "Seleccione el método y realice la firma del documento para continuar con el proceso.",
  },
  {
    id: "pago",
    titulo: "Pago de solicitud de inscripción empresaria",
    estado: "Pendiente de pago",
    descripcion: "La solicitud se encuentra aprobada. Realice el pago para finalizar el proceso.",
  },
];

export const CORRECCIONES_REVISION_MOCK = [
  {
    id: "domicilio-empresa",
    descripcion: "Corregir el domicilio de la empresa.",
    textoEnlace: "Ir a Empresa",
    destino: "empresa",
  },
  {
    id: "firma-representante",
    descripcion: "Agregar documentación respaldatoria a la firma del representante Perez, Juan Carlos.",
    textoEnlace: "Ir a Documentación",
    destino: "documentacion",
  },
];

export const TIPOS_FIRMA_REVISION_MOCK = [
  {
    id: "digital",
    titulo: "Firma digital",
    descripcion: "Permite firmar el documento en línea a través del sistema oficial de firma digital.",
    aviso: "Asegúrese de contar con su certificado digital homologado activo antes de continuar.",
    textoBoton: "Ir a FirmAr",
  },
  {
    id: "fisica",
    titulo: "Firma física",
    descripcion: "Descargue el documento, fírmelo y vuelva a cargar.",
    aviso:
      "Recuerde que se le solicitará presentar la documentación con firma hológrafa de forma presencial en la representación correspondiente.",
    textoBoton: "Continuar",
  },
];
