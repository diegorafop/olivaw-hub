import { Component, effect, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/auth/auth.service';
import { AutenticacaoStore } from '../../core/auth/autenticacao.store';
import { apiErrorMessage } from '../../core/http/api-error';
import { HttpState } from '../../core/http/http-state';
@Component({
  selector: 'app-admin-page',
  imports: [MatButtonModule],
  template: `<section class="mx-auto flex max-w-5xl flex-col gap-6 p-6 md:p-12">
    <h1 class="text-3xl font-bold">Administração</h1>
    <p>Olá, {{ store.usuario()?.name }}.</p>
    <p>Adicione aqui as funcionalidades administrativas do cliente.</p>
    <div>
      <button mat-button type="button" [disabled]="pending()" (click)="logout()">Sair</button>
    </div>
  </section>`,
})
export class AdminPage {
  readonly store = inject(AutenticacaoStore);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly pending = signal(false);
  private readonly http = inject(HttpState);
  constructor() {
    effect(() => {
      if (!this.store.autenticacao()) void this.router.navigateByUrl('/login');
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
