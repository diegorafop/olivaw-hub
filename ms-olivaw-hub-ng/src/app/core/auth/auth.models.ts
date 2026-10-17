export interface LoginRequest {
  login: string;
  senha: string;
}
export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
}
export interface AcessoAtual {
  usuarioId: string;
  nome: string;
  root: boolean;
  autorizacoes: string[];
}
export interface Sessao extends LoginResponse {
  usuario: AcessoAtual;
}
