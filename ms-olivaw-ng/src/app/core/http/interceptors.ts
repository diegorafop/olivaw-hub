import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, defer, finalize, throwError } from 'rxjs';
import { RuntimeConfig } from '../config/runtime-config';
import { AutenticacaoStore } from '../auth/autenticacao.store';
import { HttpState } from './http-state';
import { LOCAL_ERROR, SKIP_AUTH, SKIP_LOADING } from './http-context';
import { apiErrorMessage } from './api-error';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const store = inject(AutenticacaoStore);
  if (!inject(RuntimeConfig).isApiUrl(request.url) || request.context.get(SKIP_AUTH))
    return next(request);
  const token = store.tokenValido();
  return next(
    token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request,
  );
};
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const state = inject(HttpState);
  const auth = inject(AutenticacaoStore);
  const isApi = inject(RuntimeConfig).isApiUrl(request.url);
  // Só invalida a sessão que originou a requisição, evitando respostas antigas após novo login.
  const token = auth.autenticacao()?.accessToken;
  return next(request).pipe(
    catchError((error: unknown) => {
      if (
        isApi &&
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        token &&
        auth.autenticacao()?.accessToken === token &&
        !request.context.get(SKIP_AUTH)
      )
        auth.limpar();
      if (isApi && !request.context.get(LOCAL_ERROR)) state.error.set(apiErrorMessage(error));
      return throwError(() => error);
    }),
  );
};
export const loadingInterceptor: HttpInterceptorFn = (request, next) => {
  const state = inject(HttpState);
  if (request.context.get(SKIP_LOADING)) return next(request);
  return defer(() => {
    state.start();
    return next(request).pipe(finalize(() => state.finish()));
  });
};
