import { DOCUMENT, isPlatformBrowser } from '@angular/common';
import { DestroyRef, effect, inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { contactValid, whatsappDigits } from '../contact-details';
import { LeadGateway } from './lead-gateway';

export interface ProjectSeed {
  source: 'studio' | 'discovery';
  title: string;
  choices: readonly string[];
  context: string;
  presetId?: string;
  blockIds?: readonly string[];
}
export interface JourneyEvent {
  name: string;
  value: string;
  at: string;
}
export interface LeadSnapshot {
  schemaVersion: 1;
  revision: number;
  startedAt: string;
  entry: 'studio' | 'discovery';
  stage: 'exploring' | 'project' | 'submitted';
  project: ProjectSeed | null;
  excluded: readonly string[];
  requirements: string;
  business: string;
  email: string;
  whatsapp?: string;
  plan: string | null;
  events: readonly JourneyEvent[];
}
interface StoredDraft {
  id: string;
  key: string;
  snapshot: LeadSnapshot;
  journeyStep?: number;
}

@Injectable({ providedIn: 'root' })
export class ProjectDraft {
  private readonly document = inject(DOCUMENT);
  private readonly browser = isPlatformBrowser(inject(PLATFORM_ID));
  private readonly gateway = inject(LeadGateway);
  private readonly storageKey = 'olivaw.lead.v1';
  readonly journeyStep = signal(0);
  readonly seed = signal<ProjectSeed | null>(null);
  readonly requirements = signal('');
  readonly business = signal('');
  readonly email = signal('');
  readonly whatsapp = signal('');
  readonly contactAttempted = signal(false);
  readonly excluded = signal<readonly string[]>([]);
  readonly plan = signal<string | null>(null);
  readonly stage = signal<LeadSnapshot['stage']>('exploring');
  readonly events = signal<readonly JourneyEvent[]>([]);
  readonly sending = signal(false);
  readonly sent = signal(false);
  readonly deliveryMessage = signal('');
  private id = '';
  private key = '';
  private startedAt = '';
  private entry: LeadSnapshot['entry'] = 'studio';
  private revision = 0;
  private fingerprint = '';
  private pending?: ReturnType<typeof setTimeout>;

  constructor() {
    if (this.browser) this.restore();
    effect(() => {
      this.snapshot();
      this.journeyStep();
      if (!this.browser || !this.id) return;
      clearTimeout(this.pending);
      this.pending = setTimeout(() => this.store(this.snapshot()), 400);
    });
    const flush = () => this.store(this.snapshot());
    if (this.browser) this.document.defaultView?.addEventListener('pagehide', flush);
    inject(DestroyRef).onDestroy(() => {
      clearTimeout(this.pending);
      this.document.defaultView?.removeEventListener('pagehide', flush);
    });
  }
  enter(source: LeadSnapshot['entry']): void {
    if (!this.browser || this.id) return;
    const crypto = this.document.defaultView?.crypto;
    if (!crypto) return;
    this.id = crypto.randomUUID();
    this.key = Array.from(crypto.getRandomValues(new Uint8Array(32)), (byte) =>
      byte.toString(16).padStart(2, '0'),
    ).join('');
    this.startedAt = new Date().toISOString();
    this.entry = source;
    this.record('entered', source);
  }
  record(name: string, value: string): void {
    this.events.update((items) => [
      ...items.slice(-39),
      { name, value: value.slice(0, 160), at: new Date().toISOString() },
    ]);
  }
  start(seed: ProjectSeed): void {
    this.enter(seed.source);
    this.seed.set(seed);
    this.excluded.set([]);
    this.stage.set('project');
    this.record('project-opened', seed.title);
  }
  snapshot(): LeadSnapshot {
    return {
      schemaVersion: 1,
      revision: this.revision,
      startedAt: this.startedAt,
      entry: this.entry,
      stage: this.stage(),
      project: this.seed(),
      excluded: this.excluded(),
      requirements: this.requirements(),
      business: this.business(),
      email: this.email(),
      ...(this.whatsapp() ? { whatsapp: whatsappDigits(this.whatsapp()) } : {}),
      plan: this.plan(),
      events: this.events(),
    };
  }
  editEmail(value: string): void {
    this.email.set(value);
    this.sent.set(false);
    this.deliveryMessage.set('');
  }
  editWhatsapp(value: string): void {
    this.whatsapp.set(whatsappDigits(value).slice(0, 11));
    this.sent.set(false);
    this.deliveryMessage.set('');
  }
  async submit(): Promise<void> {
    if (this.sending()) return;
    this.contactAttempted.set(true);
    if (!contactValid(this.email(), this.whatsapp())) {
      this.deliveryMessage.set(
        'Informe um e-mail ou WhatsApp válido para a equipe conversar com você.',
      );
      return;
    }
    this.enter(this.seed()?.source ?? 'discovery');
    if (!this.id) {
      this.deliveryMessage.set(
        'Não foi possível preparar o envio neste navegador. Você pode copiar o resumo.',
      );
      return;
    }
    if (!this.seed()) {
      this.seed.set({
        source: 'discovery',
        title: 'Quero explorar minha ideia',
        choices: [],
        context: '',
      });
    }
    this.stage.set('submitted');
    this.sending.set(true);
    this.sent.set(false);
    this.deliveryMessage.set('Enviando sua ideia…');
    const snapshot = this.store(this.snapshot());
    try {
      await this.gateway.save(this.id, this.key, snapshot);
      const unchanged =
        JSON.stringify({ ...this.snapshot(), revision: 0 }) ===
        JSON.stringify({ ...snapshot, revision: 0 });
      this.sent.set(unchanged);
      this.deliveryMessage.set(
        unchanged
          ? 'Ideia recebida pela Olivaw. A equipe poderá entrar em contato para entender os requisitos e planejar o primeiro passo.'
          : 'A versão enviada foi recebida. Você fez novas alterações enquanto ela era enviada; envie novamente para atualizar a pasta.',
      );
    } catch {
      this.deliveryMessage.set(
        'Não foi possível enviar agora. Sua ideia continua nesta aba. Tente novamente ou copie o resumo para guardar.',
      );
    } finally {
      this.sending.set(false);
    }
  }
  private store(data: LeadSnapshot): LeadSnapshot {
    const fingerprint = JSON.stringify({ ...data, revision: 0 });
    if (fingerprint !== this.fingerprint) {
      this.revision++;
      this.fingerprint = fingerprint;
      this.sent.set(false);
    }
    const snapshot = { ...data, revision: this.revision };
    if (this.browser && this.id) {
      try {
        this.document.defaultView?.sessionStorage.setItem(
          this.storageKey,
          JSON.stringify({ id: this.id, key: this.key, snapshot, journeyStep: this.journeyStep() }),
        );
      } catch {
        /* In-memory draft remains available when storage is blocked. */
      }
    }
    return snapshot;
  }
  private restore(): void {
    try {
      const raw = this.document.defaultView?.sessionStorage.getItem(this.storageKey);
      if (!raw || raw.length > 40000) return;
      const draft = JSON.parse(raw) as StoredDraft;
      const data = draft.snapshot;
      if (
        data?.schemaVersion !== 1 ||
        !/^[a-f0-9-]{36}$/i.test(draft.id) ||
        !/^[a-f0-9]{64}$/.test(draft.key) ||
        !Number.isInteger(data.revision) ||
        data.revision < 0 ||
        !['studio', 'discovery'].includes(data.entry) ||
        !['exploring', 'project', 'submitted'].includes(data.stage) ||
        !Number.isFinite(Date.parse(data.startedAt)) ||
        (data.plan !== null && !['Bronze', 'Prata', 'Ouro', 'Diamante'].includes(data.plan)) ||
        (data.project !== null &&
          (typeof data.project?.title !== 'string' ||
            !Array.isArray(data.project?.choices) ||
            !data.project.choices.every((item) => typeof item === 'string') ||
            typeof data.project?.context !== 'string' ||
            !['studio', 'discovery'].includes(data.project?.source) ||
            (data.project.blockIds !== undefined &&
              (!Array.isArray(data.project.blockIds) ||
                !data.project.blockIds.every((item) => typeof item === 'string'))))) ||
        !Array.isArray(data.events) ||
        !data.events.every(
          (item) =>
            typeof item?.name === 'string' &&
            typeof item?.value === 'string' &&
            Number.isFinite(Date.parse(item?.at)),
        ) ||
        !Array.isArray(data.excluded) ||
        !data.excluded.every((item) => typeof item === 'string') ||
        typeof data.requirements !== 'string' ||
        typeof data.business !== 'string' ||
        typeof data.email !== 'string' ||
        (data.whatsapp !== undefined &&
          (typeof data.whatsapp !== 'string' || !/^[0-9]{0,11}$/.test(data.whatsapp)))
      )
        return;
      this.journeyStep.set(
        Number.isInteger(draft.journeyStep) && draft.journeyStep! >= 0 && draft.journeyStep! <= 4
          ? draft.journeyStep!
          : 0,
      );
      this.id = draft.id;
      this.key = draft.key;
      this.startedAt = data.startedAt;
      this.entry = data.entry;
      this.revision = data.revision;
      this.seed.set(data.project);
      this.requirements.set(data.requirements);
      this.business.set(data.business);
      this.email.set(data.email);
      this.whatsapp.set(data.whatsapp ?? '');
      this.excluded.set(data.excluded);
      this.plan.set(data.plan);
      this.stage.set(data.stage);
      this.events.set(data.events.slice(-40));
      this.fingerprint = JSON.stringify({ ...data, revision: 0 });
    } catch {
      /* Ignore unavailable storage and stale drafts. */
    }
  }
}
