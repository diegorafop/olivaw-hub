import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    title: 'LTMJ Eventos',
    loadComponent: () => import('./features/public/public-page').then((m) => m.PublicPage),
  },
  {
    path: 'login',
    title: 'Entrar | LTMJ Eventos',
    loadComponent: () => import('./features/login/login-page').then((m) => m.LoginPage),
  },
  {
    path: 'admin',
    canMatch: [authGuard],
    children: [
      {
        path: '',
        title: 'Administração | LTMJ Eventos',
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
