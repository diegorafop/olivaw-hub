import { HttpErrorResponse } from '@angular/common/http';
export function apiErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) return 'Não foi possível concluir a operação.';
  if (error.status === 0) return 'Não foi possível conectar à API.';
  if (error.status === 401) return 'Credenciais inválidas ou sessão expirada. Entre novamente.';
  if (error.status === 403) return 'Você não tem permissão para esta operação.';
  if (error.status === 413)
    return 'As fotos são grandes demais para este envio. O limite é 10 MB por foto. Reduza o tamanho das imagens ou envie menos fotos por vez.';
  if (error.status === 415) return 'O formato da foto não é aceito. Use JPEG, PNG ou WebP.';
  if (error.status === 429) return 'Muitas tentativas. Aguarde e tente novamente.';
  if (error.status >= 500) return 'Serviço indisponível. Tente novamente mais tarde.';
  const problem = error.error;
  if (problem && typeof problem === 'object') {
    if (problem.errors && typeof problem.errors === 'object') {
      const messages = Object.values(problem.errors)
        .flat()
        .filter((item): item is string => typeof item === 'string');
      if (messages.length) return messages.join(' ');
    }
    if (typeof problem.title === 'string') return problem.title;
  }
  return 'Não foi possível concluir a operação.';
}
