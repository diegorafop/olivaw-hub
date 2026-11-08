import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
@Component({
  selector: 'app-public-page',
  imports: [RouterLink, MatButtonModule],
  template: `<section
    class="mx-auto flex min-h-screen max-w-5xl flex-col justify-center gap-6 p-6 md:p-12"
  >
    <p class="text-sm font-bold text-primary">Olivaw</p>
    <h1 class="text-4xl font-bold md:text-6xl">Seu próximo projeto começa aqui.</h1>
    <p class="max-w-2xl text-lg">
      Base pública pronta para receber a identidade, os conteúdos e os serviços da sua empresa.
    </p>
    <div><a mat-flat-button routerLink="/login">Acessar administração</a></div>
  </section>`,
})
export class PublicPage {}
