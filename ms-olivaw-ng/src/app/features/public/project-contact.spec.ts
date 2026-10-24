import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ProjectDraft } from './project-folder/project-draft';
import { Router, provideRouter } from '@angular/router';
import { ProjectContact } from './project-contact';

describe('Descoberta de projeto', () => {
  async function setup() {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      imports: [ProjectContact],
      providers: [provideRouter([]), provideHttpClient()],
    });
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
    component.back();
    component.back();
    component.back();
    component.back();
    component.choosePath(component.paths[2]);
    await fixture.whenStable();
    expect(component.purpose()).toBeNull();
    expect(component.context()).toBeNull();
    expect(fixture.nativeElement.textContent).toContain(
      'O que você gostaria de cuidar com essa ferramenta?',
    );
  });

  it('leva prioridade e contexto para a pasta sem enviar dados', async () => {
    const fixture = await setup();
    const component = fixture.componentInstance;
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const path = component.paths[1];
    component.choosePath(path);
    component.chooseAnswer(path.choices[2]);
    component.chooseAnswer(path.contexts[0]);
    component.openProject();
    const draft = TestBed.inject(ProjectDraft);
    expect(draft.seed()?.choices).toEqual([path.choices[2].label, path.contexts[0].label]);
    expect(navigate).toHaveBeenCalledWith(['/projeto']);
  });
});
