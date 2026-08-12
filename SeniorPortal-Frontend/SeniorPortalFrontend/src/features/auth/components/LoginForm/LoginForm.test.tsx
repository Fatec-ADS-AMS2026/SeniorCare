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
  },
  registerUnauthorizedHandler: vi.fn(),
}));

function renderLoginForm(initialPath = '/login') {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <AuthProvider>
        <Routes>
          <Route path='/login' element={<LoginForm />} />
          <Route path='/' element={<div>Início</div>} />
          <Route path='/care/residents' element={<div>Residentes</div>} />
          <Route path='/login/mfa' element={<div>Tela de desafio MFA</div>} />
          <Route path='/mfa/enroll' element={<div>Tela de cadastro MFA</div>} />
        </Routes>
      </AuthProvider>
    </MemoryRouter>
  );
}

describe('LoginForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // GET /auth/me na restauração inicial do AuthProvider — sem sessão prévia.
    getMock.mockRejectedValue({
      isAxiosError: true,
      response: { status: 401, data: {} },
    });
  });

  it('logs in and navigates home (fallback) on a successful "ok" login', async () => {
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
      expect(screen.getByText('Início')).toBeInTheDocument();
    });
    expect(postMock).toHaveBeenCalledWith('Auth/login', {
      email: 'fulana@example.com',
      password: 'senha-correta',
    });
  });

  // §4.5 — returnTo só é honrado depois de validado contra a allowlist.
  it('navigates to a validated returnTo destination on success', async () => {
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

    renderLoginForm('/login?returnTo=%2Fcare%2Fresidents');

    await user.type(screen.getByPlaceholderText('Digite seu email'), 'fulana@example.com');
    await user.type(screen.getByPlaceholderText('Digite sua senha'), 'senha-correta');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => {
      expect(screen.getByText('Residentes')).toBeInTheDocument();
    });
  });

  it('falls back to home when returnTo is an unsafe destination', async () => {
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

    renderLoginForm('/login?returnTo=https%3A%2F%2Fevil.com');

    await user.type(screen.getByPlaceholderText('Digite seu email'), 'fulana@example.com');
    await user.type(screen.getByPlaceholderText('Digite sua senha'), 'senha-correta');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => {
      expect(screen.getByText('Início')).toBeInTheDocument();
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

  it('navigates to the MFA enrollment screen when the backend requires it', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: { status: 'mfa_enrollment_required', challengeToken: 'challenge-abc' },
    });

    renderLoginForm();

    await user.type(screen.getByPlaceholderText('Digite seu email'), 'fulana@example.com');
    await user.type(screen.getByPlaceholderText('Digite sua senha'), 'senha-correta');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => {
      expect(screen.getByText('Tela de cadastro MFA')).toBeInTheDocument();
    });
  });
});
