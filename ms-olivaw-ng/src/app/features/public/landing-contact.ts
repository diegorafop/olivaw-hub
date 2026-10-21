import { Component, inject, signal } from '@angular/core';
import { ContactInquiry } from './contact-inquiry';
import { ProjectContact } from './project-contact';

@Component({
  selector: 'app-landing-contact',
  imports: [ProjectContact],
  template: `
    <div class="contact-heading">
      <div>
        <p class="eyebrow">DO SEU JEITO, NO SEU TEMPO</p>
        <h2>Uma conversa já<br />é um bom começo.</h2>
      </div>
      <p>
        Já sabe o que precisa ou só quer tirar uma dúvida? Fale com a Olivaw. Se preferir, a gente
        ajuda a organizar sua ideia primeiro.
      </p>
    </div>
    <div class="contact-options" role="group" aria-label="Como você prefere começar?">
      <button
        type="button"
        [attr.aria-pressed]="mode() === 'direct'"
        aria-controls="direct-contact"
        (click)="mode.set('direct')"
      >
        <span aria-hidden="true">↗</span
        ><span
          ><strong>Quero conversar com a Olivaw</strong
          ><small>Deixe seu contato e uma mensagem. Sem precisar planejar um projeto.</small></span
        ><span aria-hidden="true">{{ mode() === 'direct' ? '●' : '○' }}</span>
      </button>
      <button
        type="button"
        [attr.aria-pressed]="mode() === 'guided'"
        aria-controls="guided-contact"
        (click)="mode.set('guided')"
      >
        <span aria-hidden="true">✳</span
        ><span
          ><strong>Quero organizar minha ideia</strong
          ><small>Explore possibilidades e prepare seu projeto com a gente.</small></span
        ><span aria-hidden="true">{{ mode() === 'guided' ? '●' : '○' }}</span>
      </button>
    </div>
    <div id="direct-contact" [hidden]="mode() !== 'direct'">
      <form
        (submit)="$event.preventDefault(); inquiry.send()"
        aria-label="Contato direto com a Olivaw"
      >
        <div class="form-intro">
          <h3>O que você gostaria de conversar?</h3>
          <p>Não precisa saber de tecnologia nem ter uma ideia pronta.</p>
        </div>
        <label for="contact-email">Seu e-mail para respondermos</label
        ><input
          id="contact-email"
          type="email"
          required
          maxlength="254"
          autocomplete="email"
          placeholder="voce@seunegocio.com.br"
          [value]="inquiry.email()"
          (input)="inquiry.editEmail($any($event.target).value)"
        />
        <label for="contact-message">Sua mensagem</label
        ><textarea
          id="contact-message"
          rows="4"
          required
          maxlength="3000"
          placeholder="Conte o que está pensando ou pergunte o que quiser…"
          [value]="inquiry.message()"
          (input)="inquiry.editMessage($any($event.target).value)"
        ></textarea>
        <div class="form-footer">
          <p>
            Usaremos o e-mail para responder à sua mensagem. Os dados só são enviados quando você
            confirmar abaixo.
          </p>
          <button
            type="submit"
            [disabled]="!inquiry.ready() || inquiry.sending() || inquiry.sent()"
          >
            {{
              inquiry.sending()
                ? 'Enviando…'
                : inquiry.sent()
                  ? 'Mensagem enviada ✓'
                  : 'Enviar mensagem'
            }}
            <span aria-hidden="true">↗</span>
          </button>
        </div>
        <p role="status" aria-live="polite" class="feedback">{{ inquiry.feedback() }}</p>
      </form>
    </div>
    <div id="guided-contact" [hidden]="mode() !== 'guided'"><app-project-contact /></div>
  `,
  styleUrl: './landing-contact.scss',
})
export class LandingContact {
  readonly inquiry = inject(ContactInquiry);
  readonly mode = signal<'direct' | 'guided'>('direct');
}
