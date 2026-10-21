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
        novalidate
        (submit)="$event.preventDefault(); inquiry.send()"
        aria-label="Contato direto com a Olivaw"
      >
        <div class="form-intro">
          <h3>O que você gostaria de conversar?</h3>
          <p>Não precisa saber de tecnologia nem ter uma ideia pronta.</p>
        </div>
        <fieldset aria-describedby="contact-help contact-error">
          <legend>
            Como podemos responder? <span class="required" aria-hidden="true">*</span>
          </legend>
          <p id="contact-help" class="field-help">
            Preencha ao menos um contato: e-mail ou WhatsApp. Se preencher os dois, ambos precisam
            ser válidos.
          </p>
          <div class="contact-fields">
            <div>
              <label for="contact-email">Seu e-mail</label>
              <input
                id="contact-email"
                type="email"
                maxlength="254"
                autocomplete="email"
                placeholder="voce@seunegocio.com.br"
                [value]="inquiry.email()"
                [attr.aria-invalid]="inquiry.emailError() || inquiry.contactError()"
                aria-describedby="contact-email-error contact-help contact-error"
                (blur)="inquiry.touch('email')"
                (input)="inquiry.editEmail($any($event.target).value)"
              />
              <p id="contact-email-error" class="field-error" [hidden]="!inquiry.emailError()">
                Digite um e-mail válido, como voce@empresa.com.br.
              </p>
            </div>
            <div>
              <label for="contact-whatsapp">Seu WhatsApp com DDD</label>
              <input
                id="contact-whatsapp"
                type="tel"
                inputmode="tel"
                maxlength="15"
                autocomplete="tel-national"
                placeholder="(11) 99999-9999"
                [value]="inquiry.whatsapp()"
                [attr.aria-invalid]="inquiry.whatsappError() || inquiry.contactError()"
                aria-describedby="contact-whatsapp-error contact-help contact-error"
                (blur)="inquiry.touch('whatsapp')"
                (input)="inquiry.editWhatsapp($any($event.target).value)"
              />
              <p
                id="contact-whatsapp-error"
                class="field-error"
                [hidden]="!inquiry.whatsappError()"
              >
                Informe DDD e celular com 9 dígitos: (11) 99999-9999.
              </p>
            </div>
          </div>
          <p id="contact-error" class="field-error" [hidden]="!inquiry.contactError()">
            Preencha seu e-mail ou WhatsApp para podermos responder.
          </p>
        </fieldset>
        <label for="contact-message"
          >Sua mensagem <span class="required" aria-hidden="true">*</span></label
        ><textarea
          id="contact-message"
          rows="4"
          [attr.aria-invalid]="inquiry.messageError()"
          aria-describedby="contact-message-error"
          (blur)="inquiry.touch('message')"
          required
          maxlength="3000"
          placeholder="Conte o que está pensando ou pergunte o que quiser…"
          [value]="inquiry.message()"
          (input)="inquiry.editMessage($any($event.target).value)"
        ></textarea>
        <p id="contact-message-error" class="field-error" [hidden]="!inquiry.messageError()">
          Escreva uma mensagem para continuar.
        </p>
        <div class="form-footer">
          <div class="contact-reassurance">
            <img
              src="assets/brand/reactions/olivaw-message-welcome-v1.webp"
              alt=""
              width="80"
              height="80"
            />
            <p>Pode mandar sua ideia. A Olivaw entra em contato com você.</p>
          </div>
          <button type="submit" [disabled]="inquiry.sending() || inquiry.sent()">
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
