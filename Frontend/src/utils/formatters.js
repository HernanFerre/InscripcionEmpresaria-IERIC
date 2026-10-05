export function normalizarCuit(value) {
  return String(value ?? "").replace(/\D/g, "");
}

export function esCuitValido(value) {
  const cuitNormalizado = normalizarCuit(value);

  if (cuitNormalizado.length !== 11) {
    return false;
  }

  const multiplicadores = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];

  const suma = multiplicadores.reduce((acumulado, multiplicador, indice) => acumulado + Number(cuitNormalizado[indice]) * multiplicador, 0);

  const resto = suma % 11;

  const digitoVerificador = resto === 0 ? 0 : resto === 1 ? 9 : 11 - resto;

  return digitoVerificador === Number(cuitNormalizado[10]);
}

export function formatCuit(value) {
  const numeric = normalizarCuit(value);

  const limited = numeric.slice(0, 11);

  if (limited.length <= 2) {
    return limited;
  }

  if (limited.length <= 10) {
    return `${limited.slice(0, 2)}-${limited.slice(2)}`;
  }

  return `${limited.slice(0, 2)}-${limited.slice(2, 10)}-${limited.slice(10)}`;
}
