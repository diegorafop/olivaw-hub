import { SupportConversation } from '../support/support-conversation';
import { AutenticacaoStore } from '../../../core/auth/autenticacao.store';
import { Component, DestroyRef, inject, signal, viewChild } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../../core/http/http-context';
import { inventoryError } from '../inventory/inventory-api';
import { operationConfigs, type OperationField } from './operations-config';
interface Entry {
  externalId: string;
  revision?: number;
  [key: string]: string | number | null | undefined;
}
interface Choice {
  externalId: string;
  name?: string;
  business?: string;
}
interface EntryPage {
  items: Entry[];
  nextBeforeId: number | null;
  state?: string;
}
@Component({
  selector: 'app-operations-page',
  imports: [SupportConversation, RouterLink, ReactiveFormsModule, DatePipe],
  templateUrl: './operations-page.html',
  styleUrl: '../inventory/inventory.scss',
})
export class OperationsPage {
  private readonly auth = inject(AutenticacaoStore);
  private readonly http = inject(HttpClient);
  private readonly runtime = inject(RuntimeConfig);
  private readonly destroy = inject(DestroyRef);
  readonly config = operationConfigs[inject(ActivatedRoute).snapshot.data['operation'] as string];
  readonly options = { context: new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true) };
  readonly form = new FormGroup<Record<string, FormControl<string | number | null>>>({});
  readonly anchor = new FormControl('', { nonNullable: true, validators: [Validators.required] });
  readonly search = new FormControl('', { nonNullable: true });
  readonly choices = signal<Choice[]>([]);
  readonly sourceServices = signal<Choice[]>([]);
  readonly sourceRequests = signal<Choice[]>([]);
  readonly choicePage = signal(1);
  readonly choiceTotal = signal(0);
  readonly entries = signal<Entry[]>([]);
  readonly history = signal<Entry[]>([]);
  readonly selected = signal<Entry | null>(null);
  readonly next = signal<number | null>(null);
  readonly historyNext = signal<number | null>(null);
  readonly state = signal('');
  readonly pending = signal(false);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  private sampleKey = crypto.randomUUID();
  constructor() {
    for (const field of this.config.fields) {
      const validators = [];
      if (field.required) validators.push(Validators.required);
      if (field.max) validators.push(Validators.maxLength(field.max));
      if (field.type === 'number') validators.push(Validators.min(0));
      this.form.addControl(
        field.key,
        new FormControl<string | number | null>(this.initial(field), validators),
      );
    }
    this.anchor.valueChanges.pipe(takeUntilDestroyed(this.destroy)).subscribe((id) => {
      if (this.config.anchor === 'customerExternalId' && id) this.loadSources(id);
    });
    this.loadChoices();
    if (this.config.workflow) this.load();
  }
  private initial(field: OperationField): string | number | null {
    if (field.options) return Object.keys(field.options)[0];
    if (field.key === 'maxAgeMinutes') return 15;
    if (field.type === 'number') return null;
    return '';
  }
  private loadSources(customer: string) {
    const params = new HttpParams()
      .set('customerExternalId', customer)
      .set('page', 1)
      .set('pageSize', 100);
    this.http
      .get<{ items: Choice[]; total: number }>(this.runtime.endpoint('services'), {
        ...this.options,
        params,
      })
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (data) => {
          if (customer !== this.anchor.value) return;
          this.sourceServices.set(data.items);
          if (data.total > 100)
            this.error.set('A seleção mostra os primeiros 100 serviços desta empresa.');
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
    if (this.config.endpoint === 'delivery-notes')
      this.http
        .get<{
          items: (Choice & { title: string; protocol: string })[];
          nextBeforeId: number | null;
        }>(this.runtime.endpoint('support-requests'), {
          ...this.options,
          params: new HttpParams().set('customerExternalId', customer),
        })
        .pipe(takeUntilDestroyed(this.destroy))
        .subscribe({
          next: (data) => {
            if (customer !== this.anchor.value) return;
            this.sourceRequests.set(
              data.items.map((item) => ({
                externalId: item.externalId,
                name: item.protocol + ' · ' + item.title,
              })),
            );
          },
          error: (e) => this.error.set(inventoryError(e)),
        });
  }
  optionsFor(field: OperationField) {
    return Object.entries(field.options ?? {});
  }
  display(field: OperationField, value: string | number | null | undefined) {
    if (value == null || value === '') return 'Não informado';
    if (field.source) {
      const options = field.source === 'services' ? this.sourceServices() : this.sourceRequests();
      return options.find((item) => item.externalId === value)?.name ?? 'Vínculo registrado';
    }
    if (field.type === 'datetime-local') return new Date(String(value)).toLocaleString('pt-BR');
    return field.options?.[String(value)] ?? String(value);
  }
  loadChoices(page = 1) {
    const path = this.config.anchor === 'customerExternalId' ? 'customers' : 'resources';
    this.http
      .get<{ items: Choice[]; total: number }>(this.runtime.endpoint(path), {
        ...this.options,
        params: new HttpParams()
          .set('page', page)
          .set('pageSize', 20)
          .set('search', this.search.value),
      })
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (data) => {
          this.choices.set(data.items);
          this.choicePage.set(page);
          this.choiceTotal.set(data.total);
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  load(beforeId = 0) {
    if (this.loading() || this.pending()) return;
    if (!this.config.workflow && !this.anchor.value) {
      this.entries.set([]);
      this.state.set('');
      return;
    }
    let params = new HttpParams().set('beforeId', beforeId);
    if (this.anchor.value) params = params.set(this.config.anchor, this.anchor.value);
    this.loading.set(true);
    this.error.set('');
    this.http
      .get<EntryPage>(this.runtime.endpoint(this.config.endpoint), { ...this.options, params })
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (data) => {
          this.entries.set(data.items);
          this.next.set(data.nextBeforeId);
          this.state.set(data.state ?? '');
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  edit(entry: Entry) {
    if (this.form.dirty && !window.confirm('Descartar alterações não salvas?')) return;
    this.selected.set(entry);
    this.error.set('');
    this.anchor.setValue(String(entry[this.config.anchor]));
    this.anchor.disable();
    for (const field of this.config.fields) {
      let value = entry[field.key] ?? this.initial(field);
      if (field.type === 'datetime-local' && value) {
        const date = new Date(String(value));
        value = new Date(date.getTime() - date.getTimezoneOffset() * 60000)
          .toISOString()
          .slice(0, 16);
      }
      this.form.controls[field.key].setValue(value);
      if (field.source) this.form.controls[field.key].disable();
    }
    this.form.markAsPristine();
    this.loadHistory();
  }
  loadHistory(beforeId = 0) {
    const entry = this.selected();
    if (!entry) return;
    this.http
      .get<EntryPage>(
        this.runtime.endpoint(`${this.config.endpoint}/${entry.externalId}/history`),
        { ...this.options, params: new HttpParams().set('beforeId', beforeId) },
      )
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (data) => {
          if (this.selected()?.externalId !== entry.externalId) return;
          this.history.set(data.items);
          this.historyNext.set(data.nextBeforeId);
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  newEntry() {
    if (this.form.dirty && !window.confirm('Descartar alterações não salvas?')) return;
    this.selected.set(null);
    this.anchor.enable();
    this.history.set([]);
    this.historyNext.set(null);
    for (const field of this.config.fields)
      this.form.controls[field.key].reset(this.initial(field));
    this.sampleKey = crypto.randomUUID();
    this.form.markAsPristine();
  }
  readonly supportConversation = viewChild(SupportConversation);
  canLeave() {
    if (!this.auth.tokenValido()) return true;
    if (this.pending()) return false;
    if (this.supportConversation() && !this.supportConversation()!.canLeave()) return false;
    if (!this.form.dirty) return true;
    return window.confirm('Há alterações não salvas. Deseja sair?');
  }
  runHttp() {
    if (this.pending() || !this.anchor.value) return;
    this.pending.set(true);
    this.error.set('');
    this.http
      .post(
        this.runtime.endpoint('health-readings/http'),
        { resourceExternalId: this.anchor.value, sampleKey: this.sampleKey },
        this.options,
      )
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.pending.set(false)),
      )
      .subscribe({
        next: () => {
          this.sampleKey = crypto.randomUUID();
          this.pending.set(false);
          this.success.set('Verificação HTTP registrada.');
          this.load();
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  save() {
    if (this.pending() || this.loading()) return;
    this.form.markAllAsTouched();
    this.anchor.markAsTouched();
    this.error.set('');
    this.success.set('');
    if (this.form.invalid || this.anchor.invalid || !this.anchor.value) {
      this.error.set('Confira os campos obrigatórios.');
      return;
    }
    const body: Record<string, unknown> = {
      ...this.form.getRawValue(),
      [this.config.anchor]: this.anchor.value,
    };
    for (const field of this.config.fields) {
      if (field.source && !body[field.key]) body[field.key] = null;
      if (field.type === 'datetime-local') {
        const raw = body[field.key];
        if (!raw) {
          body[field.key] = null;
          continue;
        }
        const date = new Date(String(raw));
        if (!Number.isFinite(date.valueOf())) {
          this.error.set('Confira as datas informadas.');
          return;
        }
        body[field.key] = date.toISOString();
      }
    }
    if (!this.config.workflow) {
      const date = new Date(String(body['observedAt']));
      if (!Number.isFinite(date.valueOf())) {
        this.error.set('Informe o instante da observação.');
        return;
      }
      body['observedAt'] = date.toISOString();
      body['sampleKey'] = this.sampleKey;
    }
    const entry = this.selected();
    if (entry) body['revision'] = entry.revision;
    this.pending.set(true);
    this.form.disable();
    this.anchor.disable();
    const request = entry
      ? this.http.put(
          this.runtime.endpoint(`${this.config.endpoint}/${entry.externalId}`),
          body,
          this.options,
        )
      : this.http.post(this.runtime.endpoint(this.config.endpoint), body, this.options);
    request
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => {
          this.pending.set(false);
          this.form.enable();
          if (this.selected())
            for (const field of this.config.fields)
              if (field.source) this.form.controls[field.key].disable();
          if (!this.selected()) this.anchor.enable();
        }),
      )
      .subscribe({
        next: () => {
          this.form.markAsPristine();
          this.selected.set(null);
          this.anchor.enable();
          this.history.set([]);
          this.sampleKey = crypto.randomUUID();
          this.success.set('Registro salvo.');
          this.pending.set(false);
          this.load();
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
}
