import type { CanDeactivateFn } from '@angular/router';
import type { ServiceEditor } from './service-editor';
export const serviceDraftGuard: CanDeactivateFn<ServiceEditor> = (component) =>
  component.canLeave();
