import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { AutenticacaoStore } from './autenticacao.store';

export const authGuard: CanMatchFn = () => {
  const auth = inject(AutenticacaoStore);
  return !!auth.tokenValido() || inject(Router).createUrlTree(['/login']);
};
// Uso futuro: canActivate: [permissionGuard], data: { permissao: 'eventos.editar' }.
export const permissionGuard: CanActivateFn = (route) => {
  const auth = inject(AutenticacaoStore);
  const router = inject(Router);
  if (!auth.tokenValido()) return router.createUrlTree(['/login']);
  const permission = route.data['permissao'];
  return typeof permission === 'string' && auth.permite(permission)
    ? true
    : router.createUrlTree(['/acesso-negado']);
};

// Cada funcionalidade mantém sua autorização no servidor.
export const operationGuard: CanMatchFn = () => {
  const auth = inject(AutenticacaoStore);
  const router = inject(Router);
  if (!auth.tokenValido()) return router.createUrlTree(['/login']);
  if (auth.permite('leads.manage')) return true;
  return router.createUrlTree(['/acesso-negado']);
};

export const rootGuard: CanMatchFn = () => {
  const auth = inject(AutenticacaoStore);
  if (auth.tokenValido() && auth.usuario()?.isRoot) return true;
  return inject(Router).createUrlTree(['/acesso-negado']);
};

export const operationHomeGuard: CanMatchFn = () => {
  const auth = inject(AutenticacaoStore);
  if (auth.usuario()?.isRoot) return true;
  return inject(Router).createUrlTree(['/admin/leads']);
};
