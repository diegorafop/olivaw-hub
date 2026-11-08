import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { HttpState } from './core/http/http-state';
describe('Feedback HTTP', () => {
  it('apresenta e permite fechar o erro global', async () => {
    TestBed.configureTestingModule({ imports: [App], providers: [provideRouter([])] });
    TestBed.inject(HttpState).error.set('Serviço indisponível.');
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const element: HTMLElement = fixture.nativeElement;
    expect(element.querySelector('[role="alert"]')?.textContent).toContain('Serviço indisponível.');
    element.querySelector('button')?.click();
    await fixture.whenStable();
    expect(element.querySelector('[role="alert"]')).toBeNull();
  });
});
