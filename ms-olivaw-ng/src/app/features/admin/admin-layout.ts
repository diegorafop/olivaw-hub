import { DOCUMENT } from '@angular/common';
import { Component, inject, input, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({
  selector: 'app-admin-layout',
  imports: [RouterLink],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss',
})
export class AdminLayout {
  private readonly document = inject(DOCUMENT);
  focusContent(event: Event): void {
    event.preventDefault();
    this.document.getElementById('operation-content')?.focus();
  }
  readonly collapsed = signal(false);
  readonly operator = input.required<string>();
  readonly initials = input('');
  readonly pending = input(false);
  readonly signOut = output<void>();
}
