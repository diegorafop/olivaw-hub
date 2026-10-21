import { Routes } from '@angular/router';
import { operationGuard } from './core/auth/auth.guard';
export const routes: Routes = [
  {
    path: 'projeto',
    title: 'Olivaw - Projeto',
    loadComponent: () =>
      import('./features/public/project-folder/project-folder').then((m) => m.ProjectFolder),
  },
  { path: 'seu-site', redirectTo: 'estudio', pathMatch: 'full' },
  {
    path: '',
    pathMatch: 'full',
    title: 'Olivaw Hub',
    loadComponent: () => import('./features/public/public-page').then((m) => m.PublicPage),
  },
  {
    path: 'estudio',
    title: 'Olivaw - Estúdio',
    loadComponent: () =>
      import('./features/public/site-studio/site-studio').then((m) => m.SiteStudio),
  },
  {
    path: 'login',
    title: 'Olivaw - Entrar',
    loadComponent: () => import('./features/login/login-page').then((m) => m.LoginPage),
  },
  {
    path: 'admin',
    canMatch: [operationGuard],
    children: [
      {
        path: '',
        title: 'Olivaw - Operação',
        loadComponent: () => import('./features/admin/admin-page').then((m) => m.AdminPage),
      },
    ],
  },
  {
    path: 'acesso-negado',
    title: 'Olivaw - Acesso negado',
    loadComponent: () => import('./shared/access-denied-page').then((m) => m.AccessDeniedPage),
  },
  {
    path: '**',
    title: 'Olivaw - Página não encontrada',
    loadComponent: () => import('./shared/not-found-page').then((m) => m.NotFoundPage),
  },
];
