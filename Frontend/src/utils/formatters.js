export function formatCuit(value) {
  // Solo números
  const numeric = value.replace(/\D/g, "");

  // Máximo 11 dígitos
  const limited = numeric.slice(0, 11);

  // Formato XX-XXXXXXXX-X
  if (limited.length <= 2) {
    return limited;
  }

  if (limited.length <= 10) {
    return `${limited.slice(0, 2)}-${limited.slice(2)}`;
  }

  return `${limited.slice(0, 2)}-${limited.slice(2, 10)}-${limited.slice(10)}`;
}
