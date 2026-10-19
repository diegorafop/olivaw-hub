import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ProjectContact } from './project-contact';

describe('Descoberta de projeto', () => {
  async function setup() {
    TestBed.configureTestingModule({ imports: [ProjectContact], providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(ProjectContact);
    await fixture.whenStable();
    return fixture;
  }

  it('apresenta cinco caminhos sem pedir dados ou texto na entrada', async () => {
    const fixture = await setup();
    const element: HTMLElement = fixture.nativeElement;
    expect(element.querySelectorAll('.path-option').length).toBe(5);
    expect(element.querySelector('input, textarea')).toBeNull();
    expect(element.textContent).toContain('Ainda não sei por onde começar');
  });

  it('limpa prioridade e contexto ao trocar o caminho', async () => {
    const fixture = await setup();
    const component = fixture.componentInstance;
    component.choosePath(component.paths[1]);
    component.chooseAnswer(component.paths[1].choices[0]);
    component.chooseAnswer(component.paths[1].contexts[0]);
    component.prepareConversation();
    expect(component.form.controls.summary.value).toContain(
      'Copiar informações de um lugar para outro',
    );
    component.back();
    component.back();
    component.back();
    component.back();
    component.choosePath(component.paths[2]);
    await fixture.whenStable();
    expect(component.purpose()).toBeNull();
    expect(component.context()).toBeNull();
    expect(component.form.controls.summary.value).toBe('');
    expect(fixture.nativeElement.textContent).toContain(
      'O que você gostaria de cuidar com essa ferramenta?',
    );
  });

  it('preserva a revisão editada ao voltar da revisão para a sugestão', async () => {
    const fixture = await setup();
    const component = fixture.componentInstance;
    const path = component.paths[1];
    component.choosePath(path);
    component.chooseAnswer(path.choices[2]);
    component.chooseAnswer(path.contexts[3]);
    component.prepareConversation();
    component.form.controls.summary.setValue('Minha ideia revisada.');
    component.back();
    component.prepareConversation();
    await fixture.whenStable();
    expect(component.form.controls.summary.value).toBe('Minha ideia revisada.');
    expect(component.form.valid).toBe(true);
  });
});
