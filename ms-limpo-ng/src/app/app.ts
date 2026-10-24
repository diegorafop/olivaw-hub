import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatButtonModule } from '@angular/material/button';
import { HttpState } from './core/http/http-state';
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MatProgressBarModule, MatButtonModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  readonly http = inject(HttpState);
}
