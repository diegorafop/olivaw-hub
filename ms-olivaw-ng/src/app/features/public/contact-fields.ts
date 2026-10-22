import { Component, computed, input, output, signal } from '@angular/core';
import { emailValid, whatsappValid, formatWhatsapp } from './contact-details';

@Component({
  selector: 'app-contact-fields',
  template: `
    <fieldset
      [attr.aria-describedby]="prefix() + '-contact-help' + ' ' + prefix() + '-contact-error'"
    >
      <legend>Como podemos responder? <span class="required" aria-hidden="true">*</span></legend>
      <p [id]="prefix() + '-contact-help'" class="field-help">
        Preencha ao menos um contato: e-mail ou WhatsApp. Se preencher os dois, ambos precisam ser
        válidos.
      </p>
      <div class="contact-fields">
        <div>
          <label [attr.for]="prefix() + '-contact-email'">Seu e-mail</label>
          <input
            [id]="prefix() + '-contact-email'"
            type="email"
            maxlength="254"
            autocomplete="email"
            placeholder="voce@seunegocio.com.br"
            [value]="email()"
            [attr.aria-invalid]="emailError() || contactError()"
            [attr.aria-describedby]="
              prefix() +
              '-contact-email-error' +
              ' ' +
              prefix() +
              '-contact-help' +
              ' ' +
              prefix() +
              '-contact-error'
            "
            (blur)="emailTouched.set(true)"
            (input)="emailChange.emit($any($event.target).value)"
          />
          <p [id]="prefix() + '-contact-email-error'" class="field-error" [hidden]="!emailError()">
            Digite um e-mail válido, como voce@empresa.com.br.
          </p>
        </div>
        <div>
          <label [attr.for]="prefix() + '-contact-whatsapp'">Seu WhatsApp com DDD</label>
          <input
            [id]="prefix() + '-contact-whatsapp'"
            type="tel"
            inputmode="tel"
            maxlength="15"
            autocomplete="tel-national"
            placeholder="(11) 99999-9999"
            [value]="formattedWhatsapp()"
            [attr.aria-invalid]="whatsappError() || contactError()"
            [attr.aria-describedby]="
              prefix() +
              '-contact-whatsapp-error' +
              ' ' +
              prefix() +
              '-contact-help' +
              ' ' +
              prefix() +
              '-contact-error'
            "
            (blur)="whatsappTouched.set(true)"
            (input)="whatsappChange.emit(format($any($event.target).value))"
          />
          <p
            [id]="prefix() + '-contact-whatsapp-error'"
            class="field-error"
            [hidden]="!whatsappError()"
          >
            Informe DDD e celular com 9 dígitos: (11) 99999-9999.
          </p>
        </div>
      </div>
      <p [id]="prefix() + '-contact-error'" class="field-error" [hidden]="!contactError()">
        Preencha seu e-mail ou WhatsApp para podermos responder.
      </p>
    </fieldset>
  `,
  styleUrl: './contact-fields.scss',
})
export class ContactFields {
  readonly prefix = input.required<string>();
  readonly email = input('');
  readonly whatsapp = input('');
  readonly attempted = input(false);
  readonly emailChange = output<string>();
  readonly whatsappChange = output<string>();
  readonly emailTouched = signal(false);
  readonly whatsappTouched = signal(false);
  readonly formattedWhatsapp = computed(() => formatWhatsapp(this.whatsapp()));
  readonly format = formatWhatsapp;
  readonly emailError = computed(
    () =>
      (this.attempted() || this.emailTouched()) &&
      Boolean(this.email().trim()) &&
      !emailValid(this.email()),
  );
  readonly whatsappError = computed(
    () =>
      (this.attempted() || this.whatsappTouched()) &&
      Boolean(this.whatsapp()) &&
      !whatsappValid(this.whatsapp()),
  );
  readonly contactError = computed(
    () =>
      (this.attempted() || this.emailTouched() || this.whatsappTouched()) &&
      !this.email().trim() &&
      !this.whatsapp(),
  );
}
