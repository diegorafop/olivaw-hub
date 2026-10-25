export function cnpjDigits(value: string): string {
  return value.replace(/\D/g, '').slice(0, 14);
}
export function formatCnpj(value: string): string {
  return cnpjDigits(value)
    .replace(/^(\d{2})(\d)/, '$1.$2')
    .replace(/^(\d{2}\.\d{3})(\d)/, '$1.$2')
    .replace(/^(\d{2}\.\d{3}\.\d{3})(\d)/, '$1/$2')
    .replace(/(\/\d{4})(\d)/, '$1-$2');
}
