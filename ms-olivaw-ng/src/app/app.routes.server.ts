import { RenderMode, ServerRoute } from '@angular/ssr';
export const serverRoutes: ServerRoute[] = [
  { path: '', renderMode: RenderMode.Prerender },
  { path: 'estudio', renderMode: RenderMode.Prerender },
  { path: 'planos', renderMode: RenderMode.Prerender },
  {
    path: 'planos/:planId',
    renderMode: RenderMode.Prerender,
    getPrerenderParams: async () =>
      ['bronze', 'prata', 'ouro', 'diamante'].map((planId) => ({ planId })),
  },
  { path: '**', renderMode: RenderMode.Client },
];
