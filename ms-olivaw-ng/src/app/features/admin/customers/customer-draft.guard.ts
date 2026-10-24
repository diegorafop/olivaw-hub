import { inject } from '@angular/core';
import { AutenticacaoStore } from '../../../core/auth/autenticacao.store';
import type { CanDeactivateFn } from '@angular/router';
import type { CustomerEditor } from './customer-editor';
export const customerDraftGuard: CanDeactivateFn<CustomerEditor> = (component) =>
  !inject(AutenticacaoStore).tokenValido() || component.canLeave();
