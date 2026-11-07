import { Component, HostListener, inject, signal, viewChild } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { LeadsApi, type LeadDetailData } from './leads-api';
import { LeadDetail } from './lead-detail';
import { LeadFollowUp } from './lead-follow-up';
import { LEAD_STATUSES } from './lead-status';
import {
  emptyQualification,
  approachSuggestion,
  webUrl,
  PRESENCE_LABELS,
  PRIORITY_LABELS,
  CONFIDENCE_LABELS,
} from './lead-qualification-model';

@Component({
  selector: 'app-lead-record',
  imports: [DatePipe, RouterLink, ReactiveFormsModule, LeadDetail, LeadFollowUp],
  templateUrl: './lead-record.html',
  styleUrl: './lead-record.scss',
})
export class LeadRecord {
  private readonly api = inject(LeadsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly followUp = viewChild(LeadFollowUp);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly queryParams = this.route.snapshot.queryParams;
  readonly lead = signal<LeadDetailData | null>(null);
  readonly loading = signal(true);
  readonly busy = signal(false);
  readonly loadError = signal('');
  readonly message = signal('');
  readonly error = signal('');
  readonly conflict = signal(false);
  readonly statuses = LEAD_STATUSES;
  readonly presenceOptions = Object.entries(PRESENCE_LABELS);
  readonly priorityOptions = Object.entries(PRIORITY_LABELS);
  readonly confidenceOptions = Object.entries(CONFIDENCE_LABELS);
  readonly safeUrl = webUrl;
  whatsappLink(value: string) {
    return /^[1-9][0-9]9[0-9]{8}$/.test(value) ? `https://wa.me/55${value}` : null;
  }
  private readonly text = (max: number) =>
    new FormControl('', { nonNullable: true, validators: [Validators.maxLength(max)] });
  readonly form = new FormGroup({
    segment: this.text(100),
    region: this.text(120),
    websiteUrl: this.text(2048),
    phone: this.text(40),
    whatsapp: new FormControl('', {
      nonNullable: true,
      validators: [Validators.pattern(/^[1-9][0-9]9[0-9]{8}$/)],
    }),
    contactFormUrl: this.text(2048),
    sourceUrl: this.text(2048),
    presence: new FormControl('unknown', { nonNullable: true }),
    priority: new FormControl('unassessed', { nonNullable: true }),
    confidence: new FormControl('pending', { nonNullable: true }),
    tags: this.text(730),
    portfolioReference: this.text(120),
    facts: this.text(1500),
    positives: this.text(1500),
    observations: this.text(1500),
    questions: this.text(1500),
    approach: this.text(3000),
    approachStatus: new FormControl('draft', { nonNullable: true }),
  });
  constructor() {
    void this.load();
  }
  hasChanges() {
    return this.form.dirty || !!this.followUp()?.form.dirty;
  }
  canLeave() {
    if (this.busy() || this.followUp()?.pending()) return false;
    return !this.hasChanges() || window.confirm('Descartar alterações não salvas da ficha?');
  }
  @HostListener('window:beforeunload', ['$event'])
  beforeUnload(event: BeforeUnloadEvent) {
    if (this.hasChanges() || this.busy()) event.preventDefault();
  }
  async reload() {
    if (this.canLeave()) await this.load();
  }
  async load() {
    this.loading.set(true);
    this.loadError.set('');
    try {
      const data = await firstValueFrom(this.api.detail(this.id));
      this.lead.set(data);
      const qualification = { ...emptyQualification(), ...data.snapshot?.qualification };
      this.form.reset({ ...qualification, tags: qualification.tags.join(', ') });
      this.conflict.set(false);
      this.error.set('');
      this.message.set('');
    } catch {
      this.loadError.set('Não foi possível abrir a ficha. Tente novamente.');
    } finally {
      this.loading.set(false);
    }
  }
  private qualification() {
    const value = this.form.getRawValue();
    return {
      ...this.lead()?.snapshot?.qualification,
      ...value,
      schemaVersion: 1,
      tags: value.tags
        .split(',')
        .map((tag) => tag.trim())
        .filter(Boolean),
    };
  }
  suggest() {
    if (
      this.form.controls.approach.value &&
      !window.confirm('Substituir o rascunho de abordagem atual?')
    )
      return;
    const draft = this.lead()?.snapshot;
    this.form.controls.approach.setValue(
      approachSuggestion(draft?.business || '', this.qualification()),
    );
    this.form.controls.approachStatus.setValue('draft');
    this.form.markAsDirty();
    this.message.set(
      'Sugestão preparada a partir dos campos desta ficha. Revise os fatos e personalize antes de usar. Nada foi enviado.',
    );
  }
  async copy() {
    try {
      await navigator.clipboard.writeText(this.form.controls.approach.value);
      this.message.set('Texto copiado. Nenhum contato ou envio foi registrado.');
    } catch {
      this.error.set('Não foi possível copiar. Selecione o texto do rascunho e copie manualmente.');
    }
  }
  async save() {
    const data = this.lead();
    if (!data?.snapshot || this.busy() || this.conflict()) return;
    this.message.set('');
    this.error.set('');
    const qualification = this.qualification();
    if (
      this.form.invalid ||
      qualification.tags.length > 12 ||
      qualification.tags.some((tag) => tag.length > 60)
    ) {
      this.error.set(
        'Confira os campos: WhatsApp com DDD e 11 dígitos, textos dentro dos limites e até 12 tags de 60 caracteres.',
      );
      this.form.markAllAsTouched();
      return;
    }
    if (
      [qualification.websiteUrl, qualification.sourceUrl, qualification.contactFormUrl].some(
        (url) => url && !webUrl(url),
      )
    ) {
      this.error.set('Use links completos com http:// ou https://, sem credenciais.');
      return;
    }
    this.busy.set(true);
    try {
      const result = await firstValueFrom(
        this.api.updateQualification(this.id, data.snapshot.revision, qualification),
      );
      this.lead.set({
        ...data,
        snapshot: {
          ...data.snapshot,
          revision: result.revision,
          qualification,
          qualificationUpdatedAt: result.updatedAt,
        },
      });
      if (JSON.stringify(qualification) === JSON.stringify(this.qualification()))
        this.form.markAsPristine();
      this.message.set('Qualificação e rascunho salvos. Nenhuma mensagem foi enviada.');
    } catch (error) {
      this.conflict.set(error instanceof HttpErrorResponse && error.status === 409);
      this.error.set(
        this.conflict()
          ? 'A ficha mudou em outra sessão. Copie seu rascunho antes de recarregar e comparar as informações.'
          : 'Não foi possível confirmar o salvamento. Seus campos foram mantidos; confira a ficha antes de repetir.',
      );
    } finally {
      this.busy.set(false);
    }
  }
  async followUpSaved() {
    // Follow-up has its own revision. Updating it must not discard the qualification draft.
    const data = await firstValueFrom(this.api.detail(this.id)).catch(() => null);
    if (data)
      this.lead.update((current) =>
        current
          ? {
              ...current,
              followUpStatus: data.followUpStatus,
              followUpRevision: data.followUpRevision,
              followUpUpdatedAt: data.followUpUpdatedAt,
            }
          : current,
      );
    else
      this.error.set(
        'Acompanhamento salvo, mas não foi possível atualizar a leitura. Recarregue antes de salvar outra alteração.',
      );
  }
}
