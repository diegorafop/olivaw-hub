import { RenderMode, ServerRoute } from '@angular/ssr';
export const serverRoutes: ServerRoute[] = [
  { path: '', renderMode: RenderMode.Prerender },
  { path: 'estudio', renderMode: RenderMode.Prerender },
  { path: '**', renderMode: RenderMode.Client },
];
