import {
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import {
  PortalApi,
  type PortalService,
  type PortalRequest,
  type PortalMessage,
} from './portal-api';
import { inventoryError } from '../admin/inventory/inventory-api';
export const requestTypes: Record<string, string> = {
  help: 'Preciso de ajuda',
  question: 'Tenho uma dúvida',
  improvement: 'Quero uma melhoria',
  storage: 'Quero mais armazenamento',
  capacity: 'Quero mais capacidade',
  plan: 'Quero rever meu plano',
};
export const requestStates: Record<string, string> = {
  received: 'Recebido',
  analysis: 'Em análise',
  waiting: 'Aguardando sua resposta',
  planned: 'Planejado',
  working: 'Em execução',
  validation: 'Em validação',
  delivered: 'Entregue',
  closed: 'Encerrado',
  cancelled: 'Encerrado sem execução',
};
@Component({
  selector: 'app-portal-requests',
  imports: [ReactiveFormsModule, DatePipe, RouterLink],
  templateUrl: './portal-requests.html',
  styleUrl: '../admin/inventory/inventory.scss',
})
export class PortalRequests {
  readonly customer = input.required<string>();
  readonly preview = input(false);
  readonly canRequest = input(false);
  readonly services = input<PortalService[]>([]);
  private readonly api = inject(PortalApi);
  private readonly destroy = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly query = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap,
  });
  readonly detailId = computed(() => {
    const id = this.query().get('request') ?? '';
    return id === 'new' ? '' : id;
  });
  readonly creating = computed(() => this.query().get('request') === 'new');
  readonly states = requestStates;
  readonly typeOptions = Object.entries(requestTypes);
  readonly requests = signal<PortalRequest[]>([]);
  readonly selected = signal<PortalRequest | null>(null);
  readonly messages = signal<PortalMessage[]>([]);
  readonly nextRequest = signal<number | null>(null);
  readonly nextMessage = signal<number | null>(null);
  readonly loading = signal(false);
  readonly sending = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  private requestKey = crypto.randomUUID();
  private messageKey = crypto.randomUUID();
  readonly form = new FormGroup({
    title: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(120)],
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(3000)],
    }),
    requestType: new FormControl('question', { nonNullable: true }),
    serviceExternalId: new FormControl('', { nonNullable: true }),
    desiredStorageGB: new FormControl<number | null>(null, [
      Validators.min(0.001),
      Validators.max(1000000000),
    ]),
    desiredMemoryMiB: new FormControl<number | null>(null, [
      Validators.min(0.001),
      Validators.max(1000000000),
    ]),
    desiredPlan: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(120)],
    }),
  });
  readonly reply = new FormGroup({
    body: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(3000)],
    }),
    reopen: new FormControl(false, { nonNullable: true }),
  });
  constructor() {
    effect(() => {
      const company = this.customer();
      this.detailId();
      this.creating();
      const preview = this.preview();
      untracked(() => {
        this.selected.set(null);
        this.messages.set([]);
        this.load(0, company, preview);
      });
    });
  }
  canLeave() {
    if (this.sending()) return false;
    if (!this.form.dirty && !this.reply.dirty) return true;
    return window.confirm('Há texto não enviado. Deseja sair?');
  }
  load(beforeId = 0, company = this.customer(), preview = this.preview()) {
    this.loading.set(true);
    this.api
      .requests(company, preview, beforeId, this.detailId())
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (data) => {
          if (company !== this.customer()) return;
          this.requests.set(data.items);
          this.nextRequest.set(data.nextBeforeId);
          if (this.detailId()) {
            const detail = data.items.find((x) => x.externalId === this.detailId());
            if (detail) {
              this.selected.set(detail);
              this.loadMessages();
            } else this.error.set('Solicitação não encontrada para esta empresa.');
          }
        },
        error: (e) => this.handleError(e),
      });
  }
  loadMessages(beforeId = 0) {
    const request = this.selected();
    if (!request) return;
    this.api
      .messages(this.customer(), request.externalId, this.preview(), beforeId)
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (data) => {
          if (this.selected()?.externalId !== request.externalId) return;
          this.messages.set(data.items);
          this.nextMessage.set(data.nextBeforeId);
        },
        error: (e) => this.handleError(e),
      });
  }
  send() {
    if (this.sending() || this.preview() || !this.canRequest()) return;
    this.form.markAllAsTouched();
    this.error.set('');
    this.success.set('');
    if (this.form.invalid) {
      this.error.set('Confira o título, a descrição e as capacidades informadas.');
      return;
    }
    const values = this.form.getRawValue();
    const body = {
      ...values,
      customerExternalId: this.customer(),
      serviceExternalId: values.serviceExternalId || null,
      requestKey: this.requestKey,
    };
    this.sending.set(true);
    this.form.disable();
    this.api
      .createRequest(body)
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => {
          this.sending.set(false);
          this.form.enable();
        }),
      )
      .subscribe({
        next: (result) => {
          this.success.set('Pedido registrado: ' + result.protocol);
          this.form.reset({
            title: '',
            description: '',
            requestType: 'question',
            serviceExternalId: '',
            desiredStorageGB: null,
            desiredMemoryMiB: null,
            desiredPlan: '',
          });
          this.requestKey = crypto.randomUUID();
          void this.router.navigate([], {
            relativeTo: this.route,
            queryParams: {
              section: 'requests',
              customer: this.customer(),
              request: result.externalId,
            },
            queryParamsHandling: 'merge',
          });
        },
        error: (e) => this.handleError(e),
      });
  }
  sendReply() {
    const selected = this.selected();
    if (!selected || this.sending() || this.preview() || !this.canRequest()) return;
    this.reply.markAllAsTouched();
    this.error.set('');
    if (this.reply.invalid) {
      this.error.set('Escreva sua mensagem.');
      return;
    }
    this.sending.set(true);
    this.reply.disable();
    this.api
      .createMessage({
        ...this.reply.getRawValue(),
        customerExternalId: this.customer(),
        requestExternalId: selected.externalId,
        messageKey: this.messageKey,
        revision: selected.revision,
      })
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => {
          this.sending.set(false);
          this.reply.enable();
        }),
      )
      .subscribe({
        next: () => {
          this.reply.reset({ body: '', reopen: false });
          this.messageKey = crypto.randomUUID();
          this.success.set('Mensagem registrada.');
          this.loadMessages();
          this.load();
        },
        error: (e) => this.handleError(e),
      });
  }
  private handleError(error: unknown) {
    this.error.set(inventoryError(error));
    if (!this.preview() && (error as { status?: number }).status === 401) {
      this.api.session.set(null);
      void this.router.navigate(['/meu-negocio/entrar']);
    }
  }
}
