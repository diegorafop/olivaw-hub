import { Component, computed, effect, inject, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AdminLayout } from './admin-layout';
import { AuthService } from '../../core/auth/auth.service';
import { AutenticacaoStore } from '../../core/auth/autenticacao.store';
import { apiErrorMessage } from '../../core/http/api-error';
import { HttpState } from '../../core/http/http-state';
@Component({
  selector: 'app-admin-frame',
  imports: [AdminLayout, RouterOutlet],
  styles:
    ':host { display: block; background: #f7f9f6; color: #162c39; font-family: Arial, sans-serif; }',
  template: `<app-admin-layout
    [operator]="store.usuario()?.name ?? ''"
    [initials]="initials()"
    [pending]="pending()"
    (signOut)="logout()"
    ><router-outlet
  /></app-admin-layout>`,
})
export class AdminFrame {
  readonly store = inject(AutenticacaoStore);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly pending = signal(false);
  readonly initials = computed(() =>
    (this.store.usuario()?.name.trim().split(/\s+/) ?? [])
      .slice(0, 2)
      .map((name) => name[0])
      .join('')
      .toUpperCase(),
  );
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
