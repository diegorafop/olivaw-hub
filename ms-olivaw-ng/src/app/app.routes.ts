import { inventoryDraftGuard } from './features/admin/inventory/inventory-draft.guard';
import { serviceDraftGuard } from './features/admin/services/service-draft.guard';
import { customerDraftGuard } from './features/admin/customers/customer-draft.guard';
import { Routes } from '@angular/router';
import { operationGuard, rootGuard, operationHomeGuard } from './core/auth/auth.guard';
export const routes: Routes = [
  {
    path: 'portal/chamados/novo',
    canMatch: [operationGuard, rootGuard],
    data: { portalSection: 'new' },
    title: 'Olivaw - Portal',
    loadComponent: () =>
      import('./features/portal-home-preview/portal-home-preview').then((m) => m.PortalHomePreview),
  },
  {
    path: 'portal/chamados/:itemId',
    canMatch: [operationGuard, rootGuard],
    data: { portalSection: 'request' },
    title: 'Olivaw - Portal',
    loadComponent: () =>
      import('./features/portal-home-preview/portal-home-preview').then((m) => m.PortalHomePreview),
  },
  {
    path: 'portal/chamados',
    canMatch: [operationGuard, rootGuard],
    data: { portalSection: 'requests' },
    title: 'Olivaw - Portal',
    loadComponent: () =>
      import('./features/portal-home-preview/portal-home-preview').then((m) => m.PortalHomePreview),
  },
  {
    path: 'portal/melhorias/:itemId',
    canMatch: [operationGuard, rootGuard],
    data: { portalSection: 'improvement' },
    title: 'Olivaw - Portal',
    loadComponent: () =>
      import('./features/portal-home-preview/portal-home-preview').then((m) => m.PortalHomePreview),
  },
  {
    path: 'portal/melhorias',
    canMatch: [operationGuard, rootGuard],
    data: { portalSection: 'improvements' },
    title: 'Olivaw - Portal',
    loadComponent: () =>
      import('./features/portal-home-preview/portal-home-preview').then((m) => m.PortalHomePreview),
  },
  {
    path: 'portal/plano',
    canMatch: [operationGuard, rootGuard],
    data: { portalSection: 'plan' },
    title: 'Olivaw - Portal',
    loadComponent: () =>
      import('./features/portal-home-preview/portal-home-preview').then((m) => m.PortalHomePreview),
  },
  {
    path: 'portal/indicador',
    canMatch: [operationGuard, rootGuard],
    data: { portalSection: 'indicator' },
    title: 'Olivaw - Portal',
    loadComponent: () =>
      import('./features/portal-home-preview/portal-home-preview').then((m) => m.PortalHomePreview),
  },
  {
    path: 'portal/tecnico',
    canMatch: [operationGuard, rootGuard],
    data: { portalSection: 'technical' },
    title: 'Olivaw - Portal',
    loadComponent: () =>
      import('./features/portal-home-preview/portal-home-preview').then((m) => m.PortalHomePreview),
  },
  {
    path: 'portal',
    canMatch: [operationGuard, rootGuard],
    title: 'Olivaw - Prévia do Portal',
    loadComponent: () =>
      import('./features/portal-home-preview/portal-home-preview').then((m) => m.PortalHomePreview),
  },
  {
    path: 'meu-negocio/entrar',
    title: 'Olivaw - Entrar no Meu negócio',
    loadComponent: () =>
      import('./features/portal/portal-login').then((module) => module.PortalLogin),
  },
  {
    path: 'meu-negocio',
    canDeactivate: [inventoryDraftGuard],
    title: 'Olivaw - Meu negócio',
    loadComponent: () =>
      import('./features/portal/portal-page').then((module) => module.PortalPage),
  },
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
    loadComponent: () => import('./features/admin/admin-frame').then((m) => m.AdminFrame),
    children: [
      {
        path: 'agents',
        canMatch: [rootGuard],
        title: 'Olivaw - Agents',
        loadComponent: () =>
          import('./features/admin/agents/agents-page').then((m) => m.AgentsPage),
      },
      {
        path: 'agents/baley',
        canMatch: [rootGuard],
        title: 'Olivaw - Baley',
        loadComponent: () =>
          import('./features/admin/agents/baley-reports').then((m) => m.BaleyReports),
      },
      {
        path: 'agents/baley/:reportId',
        canMatch: [rootGuard],
        title: 'Olivaw - Relatório Baley',
        loadComponent: () =>
          import('./features/admin/agents/report-detail').then((m) => m.ReportDetail),
      },
      {
        path: 'deliveries',
        canMatch: [rootGuard],
        canDeactivate: [inventoryDraftGuard],
        title: 'Olivaw - Entregas',
        data: { operation: 'deliveries' },
        loadComponent: () =>
          import('./features/admin/operations/operations-page').then(
            (module) => module.OperationsPage,
          ),
      },
      {
        path: 'portal-access',
        canMatch: [rootGuard],
        canDeactivate: [inventoryDraftGuard],
        title: 'Olivaw - Acessos do portal',
        loadComponent: () =>
          import('./features/admin/portal-access/portal-access-page').then(
            (module) => module.PortalAccessPage,
          ),
      },
      {
        path: 'portal-preview',
        canDeactivate: [inventoryDraftGuard],
        canMatch: [rootGuard],
        title: 'Olivaw - Prévia do cliente',
        data: { portalPreview: true },
        loadComponent: () =>
          import('./features/portal/portal-page').then((module) => module.PortalPage),
      },
      {
        path: 'configuration',
        canMatch: [rootGuard],
        title: 'Olivaw - Configuração',
        loadComponent: () =>
          import('./features/admin/configuration/configuration-page').then(
            (m) => m.ConfigurationPage,
          ),
      },
      {
        path: 'health',
        canMatch: [rootGuard],
        canDeactivate: [inventoryDraftGuard],
        title: 'Olivaw - Saúde',
        data: { operation: 'health' },
        loadComponent: () =>
          import('./features/admin/operations/operations-page').then((m) => m.OperationsPage),
      },
      {
        path: 'incidents',
        canMatch: [rootGuard],
        canDeactivate: [inventoryDraftGuard],
        title: 'Olivaw - Incidentes',
        data: { operation: 'incidents' },
        loadComponent: () =>
          import('./features/admin/operations/operations-page').then((m) => m.OperationsPage),
      },
      {
        path: 'usage',
        canMatch: [rootGuard],
        canDeactivate: [inventoryDraftGuard],
        title: 'Olivaw - Consumo',
        data: { operation: 'usage' },
        loadComponent: () =>
          import('./features/admin/operations/operations-page').then((m) => m.OperationsPage),
      },
      {
        path: 'care',
        canMatch: [rootGuard],
        canDeactivate: [inventoryDraftGuard],
        title: 'Olivaw - Cuidados',
        data: { operation: 'care' },
        loadComponent: () =>
          import('./features/admin/operations/operations-page').then((m) => m.OperationsPage),
      },
      {
        path: 'support',
        canMatch: [rootGuard],
        canDeactivate: [inventoryDraftGuard],
        title: 'Olivaw - Atendimento',
        data: { operation: 'support' },
        loadComponent: () =>
          import('./features/admin/operations/operations-page').then((m) => m.OperationsPage),
      },
      {
        path: 'finance',
        canMatch: [rootGuard],
        title: 'Olivaw - Financeiro',
        loadComponent: () =>
          import('./features/admin/finance/finance-dashboard').then((m) => m.FinanceDashboard),
      },
      {
        path: 'expenses',
        canMatch: [rootGuard],
        title: 'Olivaw - Despesas',
        loadComponent: () =>
          import('./features/admin/expenses/expenses-page').then((m) => m.ExpensesPage),
      },
      {
        path: 'plans/catalog',
        canMatch: [rootGuard],
        title: 'Olivaw - Proposta dos planos',
        loadComponent: () =>
          import('./features/admin/plans/plan-catalog').then((m) => m.PlanCatalog),
      },
      {
        path: 'plans',
        canMatch: [rootGuard],
        canDeactivate: [inventoryDraftGuard],
        title: 'Olivaw - Planos',
        loadComponent: () => import('./features/admin/plans/plans-page').then((m) => m.PlansPage),
      },

      {
        path: 'resources',
        canMatch: [rootGuard],
        title: 'Olivaw - Recursos',
        loadComponent: () =>
          import('./features/admin/inventory/resources-page').then((m) => m.ResourcesPage),
      },
      {
        path: 'resources/new',
        canMatch: [rootGuard],
        canDeactivate: [inventoryDraftGuard],
        title: 'Olivaw - Novo recurso',
        loadComponent: () =>
          import('./features/admin/inventory/resource-editor').then((m) => m.ResourceEditor),
      },
      {
        path: 'resources/:id',
        canMatch: [rootGuard],
        canDeactivate: [inventoryDraftGuard],
        title: 'Olivaw - Recurso',
        loadComponent: () =>
          import('./features/admin/inventory/resource-editor').then((m) => m.ResourceEditor),
      },
      {
        path: 'deployments/new',
        canMatch: [rootGuard],
        canDeactivate: [inventoryDraftGuard],
        title: 'Olivaw - Novo ambiente',
        loadComponent: () =>
          import('./features/admin/inventory/deployment-editor').then((m) => m.DeploymentEditor),
      },
      {
        path: 'deployments/:id',
        canMatch: [rootGuard],
        canDeactivate: [inventoryDraftGuard],
        title: 'Olivaw - Implantação',
        loadComponent: () =>
          import('./features/admin/inventory/deployment-editor').then((m) => m.DeploymentEditor),
      },

      {
        path: 'services',
        canMatch: [rootGuard],
        title: 'Olivaw - Serviços',
        loadComponent: () =>
          import('./features/admin/services/services-page').then((m) => m.ServicesPage),
      },
      {
        path: 'services/new',
        canMatch: [rootGuard],
        canDeactivate: [serviceDraftGuard],
        title: 'Olivaw - Novo serviço',
        loadComponent: () =>
          import('./features/admin/services/service-editor').then((m) => m.ServiceEditor),
      },
      {
        path: 'services/:id',
        canMatch: [rootGuard],
        canDeactivate: [serviceDraftGuard],
        title: 'Olivaw - Serviço',
        loadComponent: () =>
          import('./features/admin/services/service-editor').then((m) => m.ServiceEditor),
      },
      {
        path: 'customers',
        canMatch: [rootGuard],
        title: 'Olivaw - Clientes',
        loadComponent: () =>
          import('./features/admin/customers/customers-page').then((m) => m.CustomersPage),
      },
      {
        path: 'customers/new',
        canMatch: [rootGuard],
        title: 'Olivaw - Novo cliente',
        canDeactivate: [customerDraftGuard],
        loadComponent: () =>
          import('./features/admin/customers/customer-editor').then((m) => m.CustomerEditor),
      },
      {
        path: 'customers/:id',
        canMatch: [rootGuard],
        title: 'Olivaw - Cliente',
        canDeactivate: [customerDraftGuard],
        loadComponent: () =>
          import('./features/admin/customers/customer-editor').then((m) => m.CustomerEditor),
      },
      {
        path: 'leads',
        title: 'Olivaw - Leads',
        loadComponent: () => import('./features/admin/leads/leads-page').then((m) => m.LeadsPage),
      },
      {
        path: 'leads/:id',
        title: 'Olivaw - Lead',
        loadComponent: () => import('./features/admin/leads/leads-page').then((m) => m.LeadsPage),
      },
      {
        path: '',
        canMatch: [operationHomeGuard],
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
