import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { AdminPage } from './admin-page';
import { PortfolioService, ManagedEvent } from '../portfolio/portfolio.service';
import { AutenticacaoStore } from '../../core/auth/autenticacao.store';
import { AuthService } from '../../core/auth/auth.service';

describe('Gestão do portfólio', () => {
  const event: ManagedEvent = {
    id: 'evento',
    title: 'Churrasco',
    description: 'Descrição',
    category: 'Confraternizações',
    isVisible: true,
    photos: [],
  };
  let api: {
    list: ReturnType<typeof vi.fn>;
    create: ReturnType<typeof vi.fn>;
    update: ReturnType<typeof vi.fn>;
    upload: ReturnType<typeof vi.fn>;
    visibility: ReturnType<typeof vi.fn>;
    delete: ReturnType<typeof vi.fn>;
  };
  let component: AdminPage;
  let confirmed: boolean;
  beforeEach(async () => {
    api = {
      list: vi.fn().mockResolvedValue({ items: [] }),
      create: vi.fn().mockResolvedValue({ id: 'novo' }),
      update: vi.fn().mockResolvedValue({}),
      upload: vi.fn().mockResolvedValue({}),
      visibility: vi.fn().mockResolvedValue({}),
      delete: vi.fn().mockResolvedValue({}),
    };
    confirmed = false;
    TestBed.configureTestingModule({
      imports: [AdminPage],
      providers: [
        provideRouter([]),
        { provide: PortfolioService, useValue: api },
        { provide: AuthService, useValue: { logout: vi.fn() } },
        { provide: MatDialog, useValue: { open: () => ({ afterClosed: () => of(confirmed) }) } },
      ],
    });
    TestBed.inject(AutenticacaoStore).iniciar({
      accessToken: 'token',
      expiresAt: new Date(Date.now() + 60000).toISOString(),
      usuario: { usuarioId: '1', nome: 'Equipe', root: true, autorizacoes: [] },
    });
    vi.stubGlobal(
      'URL',
      Object.assign(URL, {
        createObjectURL: vi.fn().mockReturnValue('blob:foto'),
        revokeObjectURL: vi.fn(),
      }),
    );
    component = TestBed.createComponent(AdminPage).componentInstance;
    await Promise.resolve();
    await Promise.resolve();
  });
  afterEach(() => vi.unstubAllGlobals());
  it('exige título, descrição e categoria válida', async () => {
    expect(component.form.invalid).toBe(true);
    await component.save();
    expect(api.create).not.toHaveBeenCalled();
    component.form.setValue({
      title: 'Churrasco',
      description: 'Descrição',
      category: 'Confraternizações',
    });
    expect(component.form.valid).toBe(true);
    for (const field of ['title', 'description', 'category'] as const) {
      const control = component.form.controls[field];
      const previous = control.value;
      control.setValue('');
      expect(component.form.invalid).toBe(true);
      component.form.patchValue({ [field]: previous });
    }
  });
  it('seleciona múltiplas fotos para o payload', async () => {
    const files = [
      new File(['a'], 'a.jpg', { type: 'image/jpeg' }),
      new File(['b'], 'b.jpg', { type: 'image/jpeg' }),
    ];
    component.selectFiles({ target: { files, value: '' } } as unknown as Event);
    expect(component.selectedPhotos().map((photo) => photo.file)).toEqual(files);
    component.form.setValue({
      title: 'Churrasco',
      description: 'Descrição',
      category: 'Confraternizações',
    });
    await component.save();
    expect(api.upload).toHaveBeenCalledWith('novo', files);
  });
  it('mantém rascunho e fotos para repetir upload com falha', async () => {
    api.upload.mockRejectedValue(new Error('Falha de rede'));
    component.form.setValue({ title: 'Evento', description: 'Descrição', category: 'Casamentos' });
    component.selectFiles({
      target: { files: [new File(['a'], 'a.jpg', { type: 'image/jpeg' })], value: '' },
    } as unknown as Event);
    await component.save();
    expect(component.editingId()).toBe('novo');
    expect(component.selectedPhotos()).toHaveLength(1);
    api.upload.mockResolvedValue({});
    await component.save();
    expect(api.create).toHaveBeenCalledTimes(1);
  });
  it('dispara atualização de visibilidade', async () => {
    await component.toggle(event, false);
    expect(api.visibility).toHaveBeenCalledWith('evento', false);
  });
  it('exclui somente após confirmação', async () => {
    await component.remove(event);
    expect(api.delete).not.toHaveBeenCalled();
    confirmed = true;
    await component.remove(event);
    expect(api.delete).toHaveBeenCalledWith('evento');
  });
});
