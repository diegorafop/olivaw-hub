import { Component, computed, effect, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AdminLayout } from './admin-layout';
import { AdminAreaCard } from './admin-area-card';
import { AuthService } from '../../core/auth/auth.service';
import { AutenticacaoStore } from '../../core/auth/autenticacao.store';
import { apiErrorMessage } from '../../core/http/api-error';
import { HttpState } from '../../core/http/http-state';
@Component({
  selector: 'app-admin-page',
  imports: [AdminLayout, AdminAreaCard],
  templateUrl: './admin-page.html',
  styleUrl: './admin-page.scss',
})
export class AdminPage {
  readonly store = inject(AutenticacaoStore);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly pending = signal(false);
  readonly initials = computed(() => {
    const names = this.store.usuario()?.name.trim().split(/\s+/) ?? [];
    return names
      .slice(0, 2)
      .map((name) => name[0])
      .join('')
      .toUpperCase();
  });
  readonly areas = [
    {
      number: '01',
      title: 'Clientes',
      status: 'Cadastro ainda não disponível',
      text: 'As empresas e seus responsáveis terão um lugar próprio na operação.',
      icon: 'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M16 3a4 4 0 0 1 0 8M22 21v-2a4 4 0 0 0-3-3.87M13 7a4 4 0 1 1-8 0 4 4 0 0 1 8 0',
    },
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
  private readonly http = inject(HttpState);
  constructor() {
    effect(() => {
      if (!this.store.autenticacao()) {
        void this.router.navigateByUrl('/login');
        return;
      }
      if (!this.store.usuario()?.isRoot) void this.router.navigateByUrl('/acesso-negado');
    });
  }
  async logout(): Promise<void> {
    if (this.pending()) return;
    this.pending.set(true);
    try {
      await this.auth.logout();
    } catch (error) {
      this.http.error.set(
        `${apiErrorMessage(error)} A saída local foi concluída, mas a revogação no servidor não foi confirmada.`,
      );
    } finally {
      this.pending.set(false);
      await this.router.navigateByUrl('/login');
    }
  }
}
