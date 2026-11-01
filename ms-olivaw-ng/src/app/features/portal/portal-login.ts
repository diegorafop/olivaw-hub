import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, switchMap, tap, of, type Observable } from 'rxjs';
import { PortalApi } from './portal-api';
import { inventoryError } from '../admin/inventory/inventory-api';
@Component({
  selector: 'app-portal-login',
  imports: [ReactiveFormsModule, RouterLink],
  template: ` <main id="portal-content" tabindex="-1">
    <header>
      <div>
        <p class="eyebrow">OLIVAW / MEU NEGÓCIO</p>
        <h1>{{ invitation() ? 'Acesso por convite' : 'Entrar no Meu negócio' }}</h1>
        <p>Acompanhe seus sistemas e converse com a equipe.</p>
      </div>
    </header>
    @if (error()) {
      <p role="alert">{{ error() }}</p>
    }
    @if (invitation(); as info) {
      <p>Convite para {{ info.business }} destinado a {{ info.email }}.</p>
      <p>
        {{
          info.existingAccount
            ? 'Entre na conta destinatária para aceitar o novo vínculo.'
            : 'Defina uma senha de 12 a 128 caracteres para ativar seu acesso.'
        }}
      </p>
    }
    <form [formGroup]="form" (ngSubmit)="submit()" aria-label="Acesso do cliente" class="editor">
      <section class="surface">
        <label
          >E-mail<input
            type="email"
            formControlName="email"
            maxlength="254"
            autocomplete="username" /></label
        ><label
          >Senha<input
            type="password"
            formControlName="password"
            maxlength="128"
            [attr.autocomplete]="
              invitation() && !invitation()?.existingAccount ? 'new-password' : 'current-password'
            " /></label
        ><button class="primary" type="submit" [disabled]="pending()">
          {{ pending() ? 'Aguarde…' : invitation() ? 'Aceitar e entrar' : 'Entrar' }}
        </button>
      </section>
    </form>
    <p>
      Esqueceu a senha ou precisa de acesso? Solicite à equipe responsável. Contas dos aplicativos
      dos clientes são independentes deste portal.
    </p>
    <a routerLink="/">Voltar à Olivaw</a>
  </main>`,
  styleUrl: '../admin/inventory/inventory.scss',
})
export class PortalLogin {
  private readonly api = inject(PortalApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroy = inject(DestroyRef);
  readonly error = signal('');
  readonly pending = signal(false);
  readonly invitation = signal<{
    email: string;
    business: string;
    existingAccount: boolean;
    expiresAt: string;
  } | null>(null);
  readonly form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(254)],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(128)],
    }),
  });
  constructor() {
    const fragment = this.route.snapshot.fragment ?? '';
    if (fragment) {
      this.api.pendingInvitation.set(fragment);
      void this.router.navigate([], {
        relativeTo: this.route,
        fragment: undefined,
        replaceUrl: true,
      });
    }
    const secret = this.api.pendingInvitation();
    if (secret)
      this.api
        .invitation(secret)
        .pipe(takeUntilDestroyed(this.destroy))
        .subscribe({
          next: (info) => {
            this.invitation.set(info);
            this.form.controls.email.setValue(info.email);
            this.form.controls.email.disable();
          },
          error: (e) => {
            this.error.set(inventoryError(e));
            this.api.pendingInvitation.set('');
          },
        });
  }
  submit() {
    if (this.pending()) return;
    this.form.markAllAsTouched();
    this.error.set('');
    if (this.form.invalid) {
      this.error.set('Confira seu e-mail e senha.');
      return;
    }
    const { email, password } = this.form.getRawValue();
    const info = this.invitation();
    if (info && !info.existingAccount && (password.length < 12 || !password.trim())) {
      this.error.set('Use uma senha de 12 a 128 caracteres.');
      return;
    }
    this.pending.set(true);
    this.form.disable();
    const acceptFirst: Observable<unknown> =
      info && !info.existingAccount
        ? this.api.accept(this.api.pendingInvitation(), password).pipe(
            tap(() => {
              this.api.pendingInvitation.set('');
              this.invitation.set(null);
            }),
          )
        : of(null);
    acceptFirst
      .pipe(
        switchMap(() => this.api.login(email, password)),
        switchMap((session) => {
          this.api.session.set(session);
          if (info?.existingAccount) return this.api.accept(this.api.pendingInvitation(), '');
          return of(null);
        }),
        takeUntilDestroyed(this.destroy),
        finalize(() => {
          this.pending.set(false);
          this.form.enable();
          if (this.invitation()) this.form.controls.email.disable();
        }),
      )
      .subscribe({
        next: () => {
          this.api.pendingInvitation.set('');
          this.form.reset();
          void this.router.navigate(['/portal']);
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
}
