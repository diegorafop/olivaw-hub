import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({
  selector: 'app-access-denied-page',
  imports: [RouterLink],
  template: `<section class="p-6">
    <h1>Acesso negado</h1>
    <a routerLink="/">Voltar ao início</a>
  </section>`,
})
export class AccessDeniedPage {}
