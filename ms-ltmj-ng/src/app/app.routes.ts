import { Routes } from '@angular/router';
import { authGuard, permissionGuard } from './core/auth/auth.guard';
export const routes: Routes = [
  {
    path: 'previa-portfolio',
    title: 'Prévia do portfólio | LTMJ Eventos',
    data: { portfolioPreview: true },
    loadComponent: () => import('./features/public/public-page').then((m) => m.PublicPage),
  },
  {
    path: '',
    pathMatch: 'full',
    title: 'LTMJ Eventos',
    loadComponent: () => import('./features/public/public-page').then((m) => m.PublicPage),
  },
  {
    path: 'gestao/entrar',
    title: 'Entrar | LTMJ Eventos',
    loadComponent: () => import('./features/login/login-page').then((m) => m.LoginPage),
  },
  {
    path: 'gestao',
    canMatch: [authGuard],
    children: [
      {
        path: '',
        title: 'Portfólio | LTMJ Eventos',
        canActivate: [permissionGuard],
        data: { permissao: 'portfolio.manage' },
        loadComponent: () => import('./features/admin/admin-page').then((m) => m.AdminPage),
      },
    ],
  },
  {
    path: 'acesso-negado',
    title: 'Acesso negado',
    loadComponent: () => import('./shared/access-denied-page').then((m) => m.AccessDeniedPage),
  },
  {
    path: '**',
    title: 'Página não encontrada',
    loadComponent: () => import('./shared/not-found-page').then((m) => m.NotFoundPage),
  },
];
