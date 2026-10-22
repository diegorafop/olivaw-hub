import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map } from 'rxjs';
import { DOCUMENT } from '@angular/common';
import { Component, inject, computed, input, output, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
@Component({
  selector: 'app-admin-layout',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss',
})
export class AdminLayout {
  private readonly document = inject(DOCUMENT);
  focusContent(event: Event): void {
    event.preventDefault();
    this.document.getElementById('operation-content')?.focus();
  }
  private readonly router = inject(Router);
  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects),
    ),
    { initialValue: this.router.url },
  );
  readonly section = computed(() =>
    this.currentUrl().includes('/admin/customers')
      ? 'Clientes'
      : this.currentUrl().includes('/admin/leads')
        ? 'Leads'
        : 'Visão geral',
  );
  readonly isRoot = input(true);
  readonly collapsed = signal(false);
  readonly operator = input.required<string>();
  readonly initials = input('');
  readonly pending = input(false);
  readonly signOut = output<void>();
}
