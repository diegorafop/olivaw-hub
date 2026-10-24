import { Component, DestroyRef, effect, inject, input, signal, untracked } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../../core/http/http-context';
import { inventoryError } from '../inventory/inventory-api';
interface Message {
  externalId: string;
  body: string;
  authorName: string;
  isPublic: boolean;
  isStaff: boolean;
  createdAt: string;
}
@Component({
  selector: 'app-support-conversation',
  imports: [ReactiveFormsModule, DatePipe],
  template: ` <section class="surface">
    <h2>Conversa e notas do atendimento</h2>
    @if (error()) {
      <p role="alert">{{ error() }}</p>
    }
    @if (success()) {
      <p role="status">{{ success() }}</p>
    }
    @for (message of messages(); track message.externalId) {
      <article>
        <p>
          <strong>{{ message.authorName }}</strong> ·
          {{ message.isPublic ? 'Visível ao cliente' : 'Nota interna' }} ·
          {{ message.createdAt | date: 'dd/MM/yyyy HH:mm' }}
        </p>
        <p style="white-space: pre-line">{{ message.body }}</p>
      </article>
    } @empty {
      <p>Nenhuma mensagem registrada.</p>
    }
    @if (next(); as cursor) {
      <button type="button" (click)="load(cursor)">Mensagens anteriores</button>
    }
    <form [formGroup]="form" (ngSubmit)="send()" aria-label="Mensagem da equipe">
      <label>Mensagem<textarea formControlName="body" maxlength="3000" rows="4"></textarea></label
      ><label
        ><input type="checkbox" formControlName="isPublic" /> Visível ao cliente no portal</label
      >
      <p>
        Notas internas não aparecem na conversa do cliente. Uma mensagem pública fica registrada no
        portal; não envia e-mail ou WhatsApp.
      </p>
      <button type="submit" [disabled]="pending()">
        {{ form.controls.isPublic.value ? 'Registrar resposta pública' : 'Registrar nota interna' }}
      </button>
    </form>
  </section>`,
  styleUrl: '../inventory/inventory.scss',
})
export class SupportConversation {
  readonly requestId = input.required<string>();
  private readonly http = inject(HttpClient);
  private readonly runtime = inject(RuntimeConfig);
  private readonly destroy = inject(DestroyRef);
  private readonly options = {
    context: new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true),
  };
  readonly messages = signal<Message[]>([]);
  readonly next = signal<number | null>(null);
  readonly error = signal('');
  readonly success = signal('');
  readonly pending = signal(false);
  readonly form = new FormGroup({
    body: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(3000)],
    }),
    isPublic: new FormControl(false, { nonNullable: true }),
  });
  private key = crypto.randomUUID();
  constructor() {
    effect(() => {
      const id = this.requestId();
      untracked(() => this.load(0, id));
    });
  }
  canLeave() {
    if (this.pending()) return false;
    if (!this.form.dirty) return true;
    return window.confirm('Há uma mensagem não enviada. Deseja sair?');
  }
  load(beforeId = 0, id = this.requestId()) {
    this.http
      .get<{ items: Message[]; nextBeforeId: number | null }>(
        this.runtime.endpoint('support-requests/messages'),
        {
          ...this.options,
          params: new HttpParams().set('requestExternalId', id).set('beforeId', beforeId),
        },
      )
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (response) => {
          if (id !== this.requestId()) return;
          this.messages.set(response.items);
          this.next.set(response.nextBeforeId);
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  send() {
    if (this.pending()) return;
    this.form.markAllAsTouched();
    this.error.set('');
    this.success.set('');
    if (this.form.invalid) {
      this.error.set('Escreva uma mensagem de até 3.000 caracteres.');
      return;
    }
    const body = {
      ...this.form.getRawValue(),
      requestExternalId: this.requestId(),
      messageKey: this.key,
    };
    this.pending.set(true);
    this.form.disable();
    this.http
      .post(this.runtime.endpoint('support-requests/messages'), body, this.options)
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => {
          this.pending.set(false);
          this.form.enable();
        }),
      )
      .subscribe({
        next: () => {
          this.form.reset({ body: '', isPublic: false });
          this.key = crypto.randomUUID();
          this.success.set('Mensagem registrada.');
          this.load();
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
}
