import { inject } from '@angular/core';
import type { CanDeactivateFn } from '@angular/router';
import { AutenticacaoStore } from '../../../core/auth/autenticacao.store';
import type { LeadProposalPage } from './lead-proposal';
export const leadProposalGuard: CanDeactivateFn<LeadProposalPage> = (component) =>
  !inject(AutenticacaoStore).tokenValido() || component.canLeave();
