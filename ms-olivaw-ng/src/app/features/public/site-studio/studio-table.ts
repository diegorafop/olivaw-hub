import { Component, computed, signal } from '@angular/core';

@Component({
  selector: 'app-studio-table',
  template: `
    <p class="eyebrow">Sua planilha ganhou vida</p>
    <h3>Uma rotina mais organizada.</h3>
    <p>Experimente buscar e ordenar. Os dados são exemplos.</p>
    <label
      >Buscar produto
      <input
        type="search"
        placeholder="Ex.: Luminária"
        (input)="query.set($any($event.target).value)"
    /></label>
    <div class="table-scroll">
      <table>
        <thead>
          <tr>
            <th>
              <button type="button" (click)="ascending.set(!ascending())">
                Produto {{ ascending() ? '↑' : '↓' }}
              </button>
            </th>
            <th>Estoque</th>
            <th>Situação</th>
          </tr>
        </thead>
        <tbody>
          @for (row of filtered(); track row.name) {
            <tr animate.enter="row-enter">
              <td>{{ row.name }}</td>
              <td>{{ row.stock }}</td>
              <td>
                <span>{{ row.stock < 5 ? 'Repor em breve' : 'Disponível' }}</span>
              </td>
            </tr>
          } @empty {
            <tr>
              <td colspan="3">Nenhum produto encontrado.</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: `
    :host {
      display: block;
      color: #162c39;
    }
    h3 {
      font-size: 1.3rem;
      margin: 8px 0;
    }
    p {
      line-height: 1.6;
    }
    .eyebrow {
      font-size: 0.65rem;
      letter-spacing: 0.12em;
    }
    label {
      display: grid;
      gap: 8px;
      font-size: 0.8rem;
      margin: 16px 0 12px;
    }
    input {
      font: inherit;
      border: 1px solid #cbd5d4;
      border-radius: 10px;
      padding: 12px;
      background: white;
    }
    .table-scroll {
      overflow: auto;
    }
    table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.8rem;
      text-align: left;
    }
    th,
    td {
      padding: 11px 10px;
      border-bottom: 1px solid #e1e9e6;
    }
    th {
      background: #edf3f0;
    }
    button {
      font: inherit;
      color: inherit;
      border: 0;
      background: none;
      cursor: pointer;
      font-weight: 700;
    }
    td span {
      background: #edf3f0;
      border-radius: 20px;
      padding: 5px 8px;
      white-space: nowrap;
    }
    .row-enter {
      animation: arrive 0.35s ease-out;
    }
    @keyframes arrive {
      from {
        opacity: 0;
        transform: translateY(12px);
      }
      to {
        opacity: 1;
        transform: none;
      }
    }
    @media (prefers-reduced-motion: reduce) {
      .row-enter {
        animation: none;
      }
    }
  `,
})
export class StudioTable {
  readonly query = signal('');
  readonly ascending = signal(true);
  private readonly rows = [
    { name: 'Luminária Aurora', stock: 12 },
    { name: 'Cadeira Horizonte', stock: 3 },
    { name: 'Mesa Essencial', stock: 8 },
    { name: 'Vaso Origem', stock: 24 },
  ];
  readonly filtered = computed(() =>
    this.rows
      .filter((row) =>
        row.name.toLocaleLowerCase('pt-BR').includes(this.query().toLocaleLowerCase('pt-BR')),
      )
      .sort((a, b) => (this.ascending() ? 1 : -1) * a.name.localeCompare(b.name)),
  );
}
