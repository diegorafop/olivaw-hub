import { DOCUMENT } from '@angular/common';
import { inject, Injectable, signal } from '@angular/core';
import { emailValid, whatsappValid, contactValid, formatWhatsapp } from './contact-details';
import { LeadGateway } from './project-folder/lead-gateway';
import type { LeadSnapshot } from './project-folder/project-draft';

@Injectable({ providedIn: 'root' })
export class ContactInquiry {
  private readonly document = inject(DOCUMENT);
  private readonly gateway = inject(LeadGateway);
  readonly email = signal('');
  readonly message = signal('');
  readonly whatsapp = signal('');
  readonly touched = signal({ email: false, whatsapp: false, message: false });
  readonly sending = signal(false);
  readonly sent = signal(false);
  readonly feedback = signal('');
  private id = '';
  private key = '';
  private startedAt = '';
  private revision = 0;
  private fingerprint = '';
  readonly emailValid = () => emailValid(this.email());
  readonly whatsappValid = () => whatsappValid(this.whatsapp());
  readonly contactsMissing = () => !this.email().trim() && !this.whatsapp();
  readonly ready = () =>
    contactValid(this.email(), this.whatsapp()) && Boolean(this.message().trim());
  touch(field: 'email' | 'whatsapp' | 'message'): void {
    this.touched.update((value) => ({ ...value, [field]: true }));
  }
  emailError(): boolean {
    return this.touched().email && Boolean(this.email().trim()) && !this.emailValid();
  }
  whatsappError(): boolean {
    return this.touched().whatsapp && Boolean(this.whatsapp()) && !this.whatsappValid();
  }
  contactError(): boolean {
    return (this.touched().email || this.touched().whatsapp) && this.contactsMissing();
  }
  messageError(): boolean {
    return this.touched().message && !this.message().trim();
  }
  private whatsappDigits(): string {
    return this.whatsapp().replace(/\D/g, '');
  }
  private currentFingerprint(): string {
    return JSON.stringify([this.email().trim(), this.whatsappDigits(), this.message().trim()]);
  }
  editWhatsapp(value: string): void {
    this.whatsapp.set(formatWhatsapp(value));
    this.sent.set(false);
    this.feedback.set('');
  }
  editEmail(value: string): void {
    this.email.set(value);
    this.sent.set(false);
    this.feedback.set('');
  }
  editMessage(value: string): void {
    this.message.set(value);
    this.sent.set(false);
    this.feedback.set('');
  }
  async send(): Promise<void> {
    if (this.sending() || this.sent()) return;
    if (!this.ready()) {
      this.touched.set({ email: true, whatsapp: true, message: true });
      this.feedback.set('Informe um e-mail ou WhatsApp válido e escreva sua mensagem.');
      return;
    }
    const crypto = this.document.defaultView?.crypto;
    if (!crypto) {
      this.feedback.set(
        'Não foi possível preparar o envio neste navegador. Sua mensagem continua aqui.',
      );
      return;
    }
    if (!this.id) {
      this.id = crypto.randomUUID();
      this.key = Array.from(crypto.getRandomValues(new Uint8Array(32)), (byte) =>
        byte.toString(16).padStart(2, '0'),
      ).join('');
      this.startedAt = new Date().toISOString();
    }
    const fingerprint = this.currentFingerprint();
    if (fingerprint !== this.fingerprint) {
      this.revision++;
      this.fingerprint = fingerprint;
    }
    const snapshot: LeadSnapshot = {
      schemaVersion: 1,
      revision: this.revision,
      startedAt: this.startedAt,
      entry: 'discovery',
      stage: 'submitted',
      project: {
        source: 'discovery',
        title: 'Quero conversar com a Olivaw',
        choices: [],
        context: 'Contato direto pela página inicial.',
        blockIds: [],
      },
      excluded: [],
      requirements: this.message().trim(),
      business: '',
      email: this.email().trim(),
      whatsapp: this.whatsappDigits(),
      plan: null,
      events: [],
    };
    this.sending.set(true);
    this.feedback.set('Enviando sua mensagem…');
    try {
      await this.gateway.save(this.id, this.key, snapshot);
      const unchanged = fingerprint === this.currentFingerprint();
      this.sent.set(unchanged);
      this.feedback.set(
        unchanged
          ? 'Mensagem recebida pela Olivaw. A equipe poderá responder pelo contato informado.'
          : 'Mensagem recebida. Você fez novas alterações; envie novamente para atualizá-la.',
      );
    } catch {
      this.feedback.set(
        'Não foi possível enviar agora. Sua mensagem continua aqui; tente novamente em instantes.',
      );
    } finally {
      this.sending.set(false);
    }
  }
}
