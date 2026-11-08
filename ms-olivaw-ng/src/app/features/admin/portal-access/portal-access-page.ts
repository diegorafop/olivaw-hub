import { Component, DestroyRef, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../../core/http/http-context';
import { AutenticacaoStore } from '../../../core/auth/autenticacao.store';
import { inventoryError } from '../inventory/inventory-api';
interface Choice {
  externalId: string;
  name?: string;
  business?: string;
}
interface Member {
  externalId: string;
  business: string;
  name: string;
  email: string;
  role: string;
  isActive: boolean;
  revision: number;
}
interface Link {
  externalId: string;
  serviceExternalId: string;
  serviceName: string;
  healthResourceExternalId: string | null;
  label: string;
  publicUrl: string;
  isActive: boolean;
  revision: number;
}
@Component({
  selector: 'app-portal-access-page',
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  templateUrl: './portal-access-page.html',
  styleUrl: '../inventory/inventory.scss',
})
export class PortalAccessPage {
  private readonly http = inject(HttpClient);
  private readonly runtime = inject(RuntimeConfig);
  private readonly destroy = inject(DestroyRef);
  private readonly auth = inject(AutenticacaoStore);
  readonly options = { context: new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true) };
  readonly customer = new FormControl('', { nonNullable: true, validators: [Validators.required] });
  readonly search = new FormControl('', { nonNullable: true });
  readonly customers = signal<Choice[]>([]);
  readonly customerPage = signal(1);
  readonly customerTotal = signal(0);
  readonly services = signal<Choice[]>([]);
  readonly resources = signal<Choice[]>([]);
  readonly members = signal<Member[]>([]);
  readonly links = signal<Link[]>([]);
  readonly nextMember = signal<number | null>(null);
  readonly nextLink = signal<number | null>(null);
  readonly configured = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  readonly pending = signal(false);
  readonly invitation = signal<{ url: string; expiresAt: string } | null>(null);
  readonly audit = signal<
    {
      externalId: string;
      action: string;
      details: string;
      actorName: string;
      actorAuthority: string;
      createdAt: string;
    }[]
  >([]);
  readonly nextAudit = signal<number | null>(null);
  readonly recoveryMember = signal<Member | null>(null);
  readonly recoveryReason = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(500)],
  });
  readonly roleLabels: Record<string, string> = {
    viewer: 'Consulta',
    requester: 'Consulta e solicitações',
    manager: 'Gestão e cobranças',
  };
  readonly actionLabels: Record<string, string> = {
    invited: 'Convite gerado',
    accepted: 'Convite aceito',
    revoked: 'Acesso revogado',
    reset: 'Recuperação de acesso',
  };
  readonly selectedLink = signal<Link | null>(null);
  readonly invitationForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(120)],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(254)],
    }),
    role: new FormControl('requester', { nonNullable: true }),
  });
  readonly linkForm = new FormGroup({
    serviceExternalId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    healthResourceExternalId: new FormControl('', { nonNullable: true }),
    label: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(120)],
    }),
    publicUrl: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(300)] }),
    isActive: new FormControl(true, { nonNullable: true }),
  });
  constructor() {
    this.loadCustomers();
  }
  canLeave() {
    if (!this.auth.tokenValido()) return true;
    if (this.pending()) return false;
    if (!this.invitationForm.dirty && !this.linkForm.dirty && !this.recoveryReason.dirty)
      return true;
    return window.confirm('Há alterações não salvas. Deseja sair?');
  }
  loadCustomers(page = 1) {
    this.http
      .get<{ items: Choice[]; total: number }>(this.runtime.endpoint('customers'), {
        ...this.options,
        params: new HttpParams()
          .set('page', page)
          .set('pageSize', 20)
          .set('search', this.search.value),
      })
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (data) => {
          this.customers.set(data.items);
          this.customerPage.set(page);
          this.customerTotal.set(data.total);
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  private previousCustomer = '';
  chooseCustomer() {
    if (this.pending()) return;
    if (
      (this.invitationForm.dirty || this.linkForm.dirty || this.recoveryReason.dirty) &&
      !window.confirm('Descartar alterações não salvas antes de trocar a empresa?')
    ) {
      this.customer.setValue(this.previousCustomer);
      return;
    }
    this.previousCustomer = this.customer.value;
    this.recoveryMember.set(null);
    this.recoveryReason.reset('');
    this.invitation.set(null);
    this.selectedLink.set(null);
    this.invitationForm.reset({ name: '', email: '', role: 'requester' });
    this.linkForm.reset({
      serviceExternalId: '',
      healthResourceExternalId: '',
      label: '',
      publicUrl: '',
      isActive: true,
    });
    this.linkForm.controls.serviceExternalId.enable();
    this.loadMembers();
    this.loadLinks();
    this.loadAudit();
    const params = new HttpParams()
      .set('customerExternalId', this.customer.value)
      .set('page', 1)
      .set('pageSize', 100);
    for (const [endpoint, target] of [
      ['services', this.services],
      ['resources', this.resources],
    ] as const)
      this.http
        .get<{ items: Choice[]; total: number }>(this.runtime.endpoint(endpoint), {
          ...this.options,
          params,
        })
        .pipe(takeUntilDestroyed(this.destroy))
        .subscribe({
          next: (data) => {
            target.set(data.items);
            if (data.total > 100)
              this.error.set(
                'Mais de 100 opções nesta empresa. Refine os cadastros antes de publicar o serviço.',
              );
          },
          error: (e) => this.error.set(inventoryError(e)),
        });
  }
  loadAudit(beforeId = 0) {
    if (!this.customer.value) return;
    this.http
      .get<{
        items: {
          externalId: string;
          action: string;
          details: string;
          actorName: string;
          actorAuthority: string;
          createdAt: string;
        }[];
        nextBeforeId: number | null;
      }>(this.runtime.endpoint('portal-access/audit'), {
        ...this.options,
        params: new HttpParams()
          .set('customerExternalId', this.customer.value)
          .set('beforeId', beforeId),
      })
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (data) => {
          this.audit.set(data.items);
          this.nextAudit.set(data.nextBeforeId);
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  selectRecovery(member: Member) {
    this.recoveryMember.set(member);
    this.recoveryReason.reset('');
  }
  recover() {
    const member = this.recoveryMember();
    if (!member || this.pending()) return;
    this.recoveryReason.markAsTouched();
    if (this.recoveryReason.invalid) {
      this.error.set('Informe o motivo da recuperação de acesso.');
      return;
    }
    this.busy(true);
    this.error.set('');
    this.invitation.set(null);
    this.http
      .post<{ token: string; expiresAt: string }>(
        this.runtime.endpoint('portal-access/' + member.externalId + '/recovery'),
        { revision: member.revision, reason: this.recoveryReason.value },
        this.options,
      )
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.busy(false)),
      )
      .subscribe({
        next: (data) => {
          const url = new URL('/portal/entrar', window.location.origin);
          url.hash = data.token;
          this.invitation.set({ url: url.toString(), expiresAt: data.expiresAt });
          this.recoveryMember.set(null);
          this.recoveryReason.markAsPristine();
          this.success.set(
            'Sessões anteriores invalidadas. Compartilhe o novo convite somente com o destinatário.',
          );
          this.loadMembers();
          this.loadAudit();
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  loadMembers(beforeId = 0) {
    if (!this.customer.value) return;
    this.http
      .get<{ items: Member[]; authorityConfigured: boolean; nextBeforeId: number | null }>(
        this.runtime.endpoint('portal-access'),
        {
          ...this.options,
          params: new HttpParams()
            .set('customerExternalId', this.customer.value)
            .set('beforeId', beforeId),
        },
      )
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (data) => {
          this.members.set(data.items);
          this.configured.set(data.authorityConfigured);
          this.nextMember.set(data.nextBeforeId);
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  loadLinks(beforeId = 0) {
    if (!this.customer.value) return;
    this.http
      .get<{ items: Link[]; nextBeforeId: number | null }>(
        this.runtime.endpoint('portal-access/services'),
        {
          ...this.options,
          params: new HttpParams()
            .set('customerExternalId', this.customer.value)
            .set('beforeId', beforeId),
        },
      )
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (data) => {
          this.links.set(data.items);
          this.nextLink.set(data.nextBeforeId);
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  private busy(value: boolean) {
    this.pending.set(value);
    if (value) {
      this.recoveryReason.disable();
      this.customer.disable();
      this.invitationForm.disable();
      this.linkForm.disable();
      return;
    }
    this.recoveryReason.enable();
    this.customer.enable();
    this.invitationForm.enable();
    this.linkForm.enable();
    if (this.selectedLink()) this.linkForm.controls.serviceExternalId.disable();
  }
  invite() {
    if (this.pending()) return;
    this.invitationForm.markAllAsTouched();
    this.error.set('');
    if (this.invitationForm.invalid || !this.customer.value) {
      this.error.set('Selecione a empresa e confira o destinatário.');
      return;
    }
    const body = { ...this.invitationForm.getRawValue(), customerExternalId: this.customer.value };
    this.busy(true);
    this.invitation.set(null);
    this.http
      .post<{ token: string; expiresAt: string }>(
        this.runtime.endpoint('portal-access/invitations'),
        body,
        this.options,
      )
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.busy(false)),
      )
      .subscribe({
        next: (data) => {
          const url = new URL('/portal/entrar', window.location.origin);
          url.hash = data.token;
          this.invitation.set({ url: url.toString(), expiresAt: data.expiresAt });
          this.invitationForm.markAsPristine();
          this.loadAudit();
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  revoke(member: Member) {
    if (
      this.pending() ||
      !window.confirm('Revogar este vínculo? O acesso às demais empresas permanece independente.')
    )
      return;
    this.busy(true);
    this.http
      .put(
        this.runtime.endpoint('portal-access/' + member.externalId),
        { revision: member.revision },
        this.options,
      )
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.busy(false)),
      )
      .subscribe({
        next: () => {
          this.loadMembers();
          this.loadAudit();
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  editLink(link: Link) {
    if (this.linkForm.dirty && !window.confirm('Descartar a edição não salva?')) return;
    this.selectedLink.set(link);
    this.linkForm.reset({ ...link, healthResourceExternalId: link.healthResourceExternalId ?? '' });
    this.linkForm.controls.serviceExternalId.disable();
  }
  saveLink() {
    if (this.pending()) return;
    this.linkForm.markAllAsTouched();
    this.error.set('');
    if (this.linkForm.invalid || !this.customer.value) {
      this.error.set('Confira o serviço e o nome público.');
      return;
    }
    const values = this.linkForm.getRawValue();
    const body = {
      ...values,
      healthResourceExternalId: values.healthResourceExternalId || null,
      customerExternalId: this.customer.value,
      revision: this.selectedLink()?.revision,
    };
    const request = this.selectedLink()
      ? this.http.put(
          this.runtime.endpoint('portal-access/services/' + this.selectedLink()!.externalId),
          body,
          this.options,
        )
      : this.http.post(this.runtime.endpoint('portal-access/services'), body, this.options);
    this.busy(true);
    request
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.busy(false)),
      )
      .subscribe({
        next: () => {
          this.linkForm.markAsPristine();
          this.selectedLink.set(null);
          this.linkForm.reset({
            serviceExternalId: '',
            healthResourceExternalId: '',
            label: '',
            publicUrl: '',
            isActive: true,
          });
          this.loadLinks();
          this.success.set('Configuração pública registrada.');
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
}
