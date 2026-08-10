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

function renderLoginForm() {
  return render(
    <MemoryRouter initialEntries={['/login']}>
      <AuthProvider>
        <Routes>
          <Route path='/login' element={<LoginForm />} />
          <Route path='/admin' element={<div>Visão Geral</div>} />
          <Route path='/login/mfa' element={<div>Tela de desafio MFA</div>} />
        </Routes>
      </AuthProvider>
    </MemoryRouter>
  );
}

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
