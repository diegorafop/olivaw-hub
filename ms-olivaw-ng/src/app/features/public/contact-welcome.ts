import { Component } from '@angular/core';
@Component({
  selector: 'app-contact-welcome',
  template: `<img
      src="assets/brand/reactions/olivaw-message-welcome-v1.webp"
      alt=""
      width="80"
      height="80"
    />
    <p>Pode mandar sua ideia. A Olivaw entra em contato com você.</p>`,
  styles: `
    :host {
      display: flex;
      align-items: center;
      gap: 14px;
      max-width: 440px;
    }
    img {
      flex: 0 0 80px;
      object-fit: contain;
    }
    p {
      font-size: 0.875rem;
      line-height: 1.65;
      margin: 0;
      color: #64766c;
    }
  `,
})
export class ContactWelcome {}
