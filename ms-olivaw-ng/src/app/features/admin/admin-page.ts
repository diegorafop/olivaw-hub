import { CustomerOverview } from './customers/customer-overview';
import { Component, inject } from '@angular/core';
import { AdminAreaCard } from './admin-area-card';
import { LeadOverview } from './leads/lead-overview';
import { AutenticacaoStore } from '../../core/auth/autenticacao.store';
@Component({
  selector: 'app-admin-page',
  imports: [AdminAreaCard, LeadOverview, CustomerOverview],
  templateUrl: './admin-page.html',
  styleUrl: './admin-page.scss',
})
export class AdminPage {
  readonly store = inject(AutenticacaoStore);
  readonly areas = [
    {
      number: '02',
      title: 'Sistemas',
      status: 'Monitoramento não conectado',
      text: 'Saúde e recursos aparecerão aqui com as primeiras fontes de dados.',
      icon: 'M3 4h18v12H3zM8 21h8M12 16v5M7 10h2l2-3 2 6 2-3h2',
    },
    {
      number: '03',
      title: 'Atendimento',
      status: 'Solicitações ainda não habilitadas',
      text: 'Conversas, dúvidas e melhorias acompanhadas do início à entrega.',
      icon: 'M21 11.5a8.4 8.4 0 0 1-.9 3.8 8.5 8.5 0 0 1-7.6 4.7 8.4 8.4 0 0 1-3.8-.9L3 21l1.9-5.7a8.4 8.4 0 0 1-.9-3.8 8.5 8.5 0 0 1 4.7-7.6 8.4 8.4 0 0 1 3.8-.9h.5a8.5 8.5 0 0 1 8 8v.5Z',
    },
  ];
}
