import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import MfaChallengePage from './MfaChallengePage';
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

function renderMfaChallenge(state: unknown, search?: string) {
  return render(
    <MemoryRouter
      initialEntries={[{ pathname: '/login/mfa', search, state }]}
    >
      <AuthProvider>
        <Routes>
          <Route path='/login' element={<div>Tela de login</div>} />
          <Route path='/login/mfa' element={<MfaChallengePage />} />
          <Route path='/admin' element={<div>Visão Geral</div>} />
        </Routes>
      </AuthProvider>
    </MemoryRouter>
  );
}

// jsdom não permite espionar window.location.assign diretamente — substitui
// o objeto location inteiro por um mock local, restaurado depois de cada
// teste (mesmo padrão de LoginForm.test.tsx).
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

describe('MfaChallengePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    getMock.mockRejectedValue({
      isAxiosError: true,
      response: { status: 401, data: {} },
    });
  });

  it('redirects back to /login when there is no challengeToken (e.g. direct reload)', async () => {
    renderMfaChallenge(null);

    await waitFor(() => {
      expect(screen.getByText('Tela de login')).toBeInTheDocument();
    });
  });

  it('completes the login and navigates home on a valid code', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: {
        status: 'ok',
        identity: {
          userId: '1',
          institutionId: '1',
          institutionName: 'ILPI Teste',
          displayName: 'Fulana',
          email: 'fulana@example.com',
          roles: [],
          organizationalResponsibilities: [],
          effectivePermissions: [],
        },
        remainingRecoveryCodes: null,
      },
    });

    renderMfaChallenge({ challengeToken: 'challenge-abc' });

    await user.type(screen.getByRole('textbox'), '123456');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => {
      expect(screen.getByText('Visão Geral')).toBeInTheDocument();
    });
    expect(postMock).toHaveBeenCalledWith('Auth/login/mfa', {
      challengeToken: 'challenge-abc',
      code: '123456',
    });
  });

  // §7.4 — o returnTo cruzado sobrevive à etapa de MFA e dispara navegação
  // de página inteira (não navigate()) só depois de confirmado o código.
  it('does a full-page navigation to a validated cross-app returnTo after confirming the code', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: {
        status: 'ok',
        identity: {
          userId: '1',
          institutionId: '1',
          institutionName: 'ILPI Teste',
          displayName: 'Fulana',
          email: 'fulana@example.com',
          roles: [],
          organizationalResponsibilities: [],
          effectivePermissions: [],
        },
        remainingRecoveryCodes: null,
      },
    });
    const assignSpy = mockLocationAssign();

    renderMfaChallenge({ challengeToken: 'challenge-abc' }, '?returnTo=%2Fcare%2Fresidents');

    await user.type(screen.getByRole('textbox'), '123456');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => {
      expect(assignSpy).toHaveBeenCalledWith('/care/residents');
    });
    expect(screen.queryByText('Visão Geral')).not.toBeInTheDocument();

    restoreLocation();
  });

  it('falls back to the default destination when returnTo is unsafe', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: {
        status: 'ok',
        identity: {
          userId: '1',
          institutionId: '1',
          institutionName: 'ILPI Teste',
          displayName: 'Fulana',
          email: 'fulana@example.com',
          roles: [],
          organizationalResponsibilities: [],
          effectivePermissions: [],
        },
        remainingRecoveryCodes: null,
      },
    });
    const assignSpy = mockLocationAssign();

    renderMfaChallenge({ challengeToken: 'challenge-abc' }, '?returnTo=https%3A%2F%2Fevil.com');

    await user.type(screen.getByRole('textbox'), '123456');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => {
      expect(screen.getByText('Visão Geral')).toBeInTheDocument();
    });
    expect(assignSpy).not.toHaveBeenCalled();

    restoreLocation();
  });

  it('shows an alert with the backend message on an invalid code', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: { status: 'mfa_required', challengeToken: 'challenge-abc' },
    });

    renderMfaChallenge({ challengeToken: 'challenge-abc' });

    await user.type(screen.getByRole('textbox'), '000000');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => {
      expect(screen.getByText('Código inválido.')).toBeInTheDocument();
    });
  });
});
