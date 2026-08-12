import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import LoginForm from './index';
import { AuthProvider } from '@/contexts/AuthContext';

const getMock = vi.fn();
const postMock = vi.fn();

vi.mock('@/features/api', () => ({
  api: {
    get: (...args: unknown[]) => getMock(...args),
    post: (...args: unknown[]) => postMock(...args),
    put: vi.fn(),
    delete: vi.fn(),
    patch: vi.fn(),
  },
  registerUnauthorizedHandler: vi.fn(),
}));

function renderLoginForm(initialEntry: string | { pathname: string; search?: string; state?: unknown } = '/login') {
  return render(
    <MemoryRouter initialEntries={[initialEntry]}>
      <AuthProvider>
        <Routes>
          <Route path='/login' element={<LoginForm />} />
          <Route path='/admin' element={<div>Visão Geral</div>} />
          <Route path='/login/mfa' element={<div>Tela de desafio MFA</div>} />
          <Route path='/religion' element={<div>Religiões</div>} />
        </Routes>
      </AuthProvider>
    </MemoryRouter>
  );
}

// jsdom não permite espionar window.location.assign diretamente
// (Object.defineProperty de Location não é configurável em todo ambiente) —
// mesma limitação já contornada em SeniorPortal-Frontend, substitui o objeto
// location inteiro por um mock local, restaurado depois de cada teste.
const originalLocation = window.location;

function mockLocationAssign() {
  const assign = vi.fn();
  Object.defineProperty(window, 'location', {
    value: { ...originalLocation, assign },
    writable: true,
    configurable: true,
  });
  return assign;
}

function restoreLocation() {
  Object.defineProperty(window, 'location', {
    value: originalLocation,
    writable: true,
    configurable: true,
  });
}

const okLoginResponse = {
  data: {
    status: 'ok',
    identity: {
      userId: '1',
      institutionId: '1',
      institutionName: 'ILPI Teste',
      displayName: 'Fulana de Tal',
      email: 'fulana@example.com',
      roles: [],
      organizationalResponsibilities: [],
      effectivePermissions: [],
    },
  },
};

describe('LoginForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // /auth/me na restauração inicial do AuthProvider — sem sessão prévia.
    getMock.mockRejectedValue({
      isAxiosError: true,
      response: { status: 401, data: {} },
    });
  });

  it('logs in and navigates home on a successful "ok" login', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: {
        status: 'ok',
        identity: {
          userId: '1',
          institutionId: '1',
          institutionName: 'ILPI Teste',
          displayName: 'Fulana de Tal',
          email: 'fulana@example.com',
          roles: [],
          organizationalResponsibilities: [],
          effectivePermissions: [],
        },
      },
    });

    renderLoginForm();

    await user.type(screen.getByPlaceholderText('Digite seu email'), 'fulana@example.com');
    await user.type(screen.getByPlaceholderText('Digite sua senha'), 'senha-correta');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => {
      expect(screen.getByText('Visão Geral')).toBeInTheDocument();
    });
    expect(postMock).toHaveBeenCalledWith('Auth/login', {
      email: 'fulana@example.com',
      password: 'senha-correta',
    });
  });

  it('shows the real backend message on invalid credentials (401 with body)', async () => {
    const user = userEvent.setup();
    postMock.mockRejectedValueOnce({
      isAxiosError: true,
      response: { status: 401, data: { message: 'Credenciais inválidas.' } },
    });

    renderLoginForm();

    await user.type(screen.getByPlaceholderText('Digite seu email'), 'fulana@example.com');
    await user.type(screen.getByPlaceholderText('Digite sua senha'), 'senha-errada');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => {
      expect(screen.getByText('Credenciais inválidas.')).toBeInTheDocument();
    });
  });

  // §6.4 — returnTo cruzado (portal → care) exige navegação de página
  // inteira: portal/care/stock são bundles/roteadores React Router
  // separados (design.md decisão 1), `navigate()` nunca alcança uma rota
  // fora do router deste próprio app. Espiona `window.location.assign` em
  // vez de registrar uma rota `/stock/products` falsa que não existe no
  // AppRoutes.tsx real (isso mascararia exatamente o bug que esse
  // mecanismo existe pra evitar).
  it('does a full-page navigation to a validated cross-app returnTo destination on success', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce(okLoginResponse);
    const assignSpy = mockLocationAssign();

    renderLoginForm({ pathname: '/login', search: '?returnTo=%2Fstock%2Fproducts' });

    await user.type(screen.getByPlaceholderText('Digite seu email'), 'fulana@example.com');
    await user.type(screen.getByPlaceholderText('Digite sua senha'), 'senha-correta');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => {
      expect(assignSpy).toHaveBeenCalledWith('/stock/products');
    });
    // Não tenta navegação SPA nenhuma pro destino cruzado.
    expect(screen.queryByText('Visão Geral')).not.toBeInTheDocument();

    restoreLocation();
  });

  it('falls back to the default destination (SPA navigate) when returnTo is unsafe', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce(okLoginResponse);
    const assignSpy = mockLocationAssign();

    renderLoginForm({ pathname: '/login', search: '?returnTo=https%3A%2F%2Fevil.com' });

    await user.type(screen.getByPlaceholderText('Digite seu email'), 'fulana@example.com');
    await user.type(screen.getByPlaceholderText('Digite sua senha'), 'senha-correta');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => {
      expect(screen.getByText('Visão Geral')).toBeInTheDocument();
    });
    expect(assignSpy).not.toHaveBeenCalled();

    restoreLocation();
  });

  it('prioritizes the internal preserved deep link (location.state.from) over returnTo', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce(okLoginResponse);
    const assignSpy = mockLocationAssign();

    renderLoginForm({
      pathname: '/login',
      search: '?returnTo=%2Fstock%2Fproducts',
      state: { from: { pathname: '/religion' } },
    });

    await user.type(screen.getByPlaceholderText('Digite seu email'), 'fulana@example.com');
    await user.type(screen.getByPlaceholderText('Digite sua senha'), 'senha-correta');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => {
      expect(screen.getByText('Religiões')).toBeInTheDocument();
    });
    // from vence — nunca chega a considerar o returnTo cruzado.
    expect(assignSpy).not.toHaveBeenCalled();

    restoreLocation();
  });

  it('navigates to the MFA challenge screen when the backend requires it', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: { status: 'mfa_required', challengeToken: 'challenge-abc' },
    });

    renderLoginForm();

    await user.type(screen.getByPlaceholderText('Digite seu email'), 'fulana@example.com');
    await user.type(screen.getByPlaceholderText('Digite sua senha'), 'senha-correta');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => {
      expect(screen.getByText('Tela de desafio MFA')).toBeInTheDocument();
    });
  });
});
