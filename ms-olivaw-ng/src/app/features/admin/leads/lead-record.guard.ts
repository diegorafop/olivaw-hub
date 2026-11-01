import { inject } from '@angular/core';
import type { CanDeactivateFn } from '@angular/router';
import { AutenticacaoStore } from '../../../core/auth/autenticacao.store';
import type { LeadRecord } from './lead-record';
export const leadRecordGuard: CanDeactivateFn<LeadRecord> = (component) =>
  !inject(AutenticacaoStore).tokenValido() || component.canLeave();
