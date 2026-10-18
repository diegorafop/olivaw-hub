import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({
  selector: 'app-not-found-page',
  imports: [RouterLink],
  template: `<section class="p-6">
    <h1>Página não encontrada</h1>
    <a routerLink="/">Voltar ao início</a>
  </section>`,
})
export class NotFoundPage {}
