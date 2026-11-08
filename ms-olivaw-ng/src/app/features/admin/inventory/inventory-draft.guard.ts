import type { CanDeactivateFn } from '@angular/router';
export const inventoryDraftGuard: CanDeactivateFn<{ canLeave(): boolean }> = (component) =>
  component.canLeave();
