export const emailValid = (value: string): boolean =>
  /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim());
export const whatsappDigits = (value: string): string => value.replace(/\D/g, '');
export const whatsappValid = (value: string): boolean =>
  /^[1-9][0-9]9[0-9]{8}$/.test(whatsappDigits(value));
export function contactValid(email: string, whatsapp: string): boolean {
  if (!email.trim() && !whatsapp) return false;
  if (email.trim() && !emailValid(email)) return false;
  if (whatsapp && !whatsappValid(whatsapp)) return false;
  return true;
}
export function formatWhatsapp(value: string): string {
  const digits = whatsappDigits(value).slice(0, 11);
  if (digits.length > 7) return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7)}`;
  if (digits.length > 2) return `(${digits.slice(0, 2)}) ${digits.slice(2)}`;
  return digits;
}
