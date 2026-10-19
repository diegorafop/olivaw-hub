import { Component } from '@angular/core';
@Component({
  selector: 'app-studio-empty',
  template:
    '<div class="empty-canvas"><span aria-hidden="true">＋</span><h2>Todo começo tem espaço.</h2><p>Escolha uma peça.<br />Seu site começa a nascer aqui.</p><small>Uma ideia de cada vez.</small></div>',
  styles: `
    .empty-canvas {
      min-height: 440px;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      background: radial-gradient(#d5dfdc 1px, transparent 1px);
      background-size: 20px 20px;
      color: #48615c;
      padding: 24px;
    }
    .empty-canvas > span {
      font-size: 3rem;
      color: #78968a;
    }
    .empty-canvas h2 {
      font-size: 1.5rem;
    }
    .empty-canvas p {
      line-height: 1.8;
    }
    .empty-canvas small {
      margin-top: 24px;
    }
  `,
})
export class StudioEmpty {}
