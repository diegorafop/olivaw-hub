import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/auth/auth.service';
import { apiErrorMessage } from '../../core/http/api-error';
@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule, RouterLink, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `<section class="mx-auto flex min-h-screen max-w-md flex-col justify-center gap-6 p-6">
    <a routerLink="/">Voltar ao site</a>
    <h1 class="text-3xl font-bold">Entrar na administração</h1>
    <form [formGroup]="form" (ngSubmit)="submit()" class="flex flex-col gap-4">
      <mat-form-field
        ><mat-label>Login</mat-label
        ><input matInput formControlName="login" autocomplete="username" required maxlength="100" />
        @if (form.controls.login.invalid) {
          <mat-error>Informe seu login.</mat-error>
        }
      </mat-form-field>
      <mat-form-field
        ><mat-label>Senha</mat-label
        ><input
          matInput
          type="password"
          formControlName="senha"
          autocomplete="current-password"
          required
          maxlength="128"
        />
        @if (form.controls.senha.invalid) {
          <mat-error>Informe sua senha.</mat-error>
        }
      </mat-form-field>
      @if (error()) {
        <p role="alert">{{ error() }}</p>
      }
      <button mat-flat-button type="submit" [disabled]="pending()">
        {{ pending() ? 'Entrando…' : 'Entrar' }}
      </button>
    </form>
  </section>`,
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly pending = signal(false);
  readonly error = signal<string | null>(null);
  readonly form = new FormGroup({
    login: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    senha: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(128)],
    }),
  });
  async submit(): Promise<void> {
    if (this.pending()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.auth.login(this.form.getRawValue());
      await this.router.navigateByUrl('/admin');
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.pending.set(false);
    }
  }
}
