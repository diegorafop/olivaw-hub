import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PortfolioService } from './portfolio.service';
import { authInterceptor, loadingInterceptor } from '../../core/http/interceptors';
import { HttpState } from '../../core/http/http-state';
import { AutenticacaoStore } from '../../core/auth/autenticacao.store';

describe('API do portfólio', () => {
  let api: PortfolioService;
  let requests: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor, loadingInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    api = TestBed.inject(PortfolioService);
    requests = TestBed.inject(HttpTestingController);
    TestBed.inject(AutenticacaoStore).iniciar({
      accessToken: 'token',
      expiresAt: new Date(Date.now() + 60000).toISOString(),
      usuario: {
        userProfileId: '1',
        name: 'Equipe',
        isRoot: false,
        permissions: ['portfolio.manage'],
      },
    });
  });
  afterEach(() => requests.verify());
  it('envia várias fotos em multipart com JWT e loading global', async () => {
    const files = [
      new File(['a'], 'a.jpg', { type: 'image/jpeg' }),
      new File(['b'], 'b.png', { type: 'image/png' }),
    ];
    const pending = api.upload('evento', files);
    const request = requests.expectOne('http://localhost:5137/portfolio/events/evento/photos');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.getAll('files')).toEqual(files);
    expect(request.request.headers.get('Authorization')).toBe('Bearer token');
    expect(request.request.headers.has('Content-Type')).toBe(false);
    expect(TestBed.inject(HttpState).loading()).toBe(true);
    request.flush({ count: 2 });
    await pending;
    expect(TestBed.inject(HttpState).loading()).toBe(false);
  });
  it('define visibilidade explicitamente com PATCH', async () => {
    const pending = api.visibility('evento', false);
    const request = requests.expectOne('http://localhost:5137/portfolio/events/evento/visibility');
    expect(request.request.method).toBe('PATCH');
    expect(request.request.body).toEqual({ isVisible: false });
    request.flush({});
    await pending;
  });
  it('exclui pelo identificador do evento', async () => {
    const pending = api.delete('evento');
    const request = requests.expectOne('http://localhost:5137/portfolio/events/evento');
    expect(request.request.method).toBe('DELETE');
    request.flush({});
    await pending;
  });
});
