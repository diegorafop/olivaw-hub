export interface LoginRequest {
  login: string;
  password: string;
}
export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
}
export interface AcessoAtual {
  userProfileId: string;
  name: string;
  isRoot: boolean;
  permissions: string[];
}
export interface Sessao extends LoginResponse {
  usuario: AcessoAtual;
}
