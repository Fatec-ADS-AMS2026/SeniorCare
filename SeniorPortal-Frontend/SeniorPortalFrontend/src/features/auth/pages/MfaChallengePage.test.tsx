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
  },
  registerUnauthorizedHandler: vi.fn(),
}));

function renderMfaChallenge(pathWithQuery: string, state: unknown) {
  const [pathname, search] = pathWithQuery.split('?');
  return render(
    <MemoryRouter initialEntries={[{ pathname, search: search ? `?${search}` : undefined, state }]}>
      <AuthProvider>
        <Routes>
          <Route path='/login' element={<div>Tela de login</div>} />
          <Route path='/login/mfa' element={<MfaChallengePage />} />
          <Route path='/' element={<div>Início</div>} />
          <Route path='/care/residents' element={<div>Residentes</div>} />
        </Routes>
      </AuthProvider>
    </MemoryRouter>
  );
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
    renderMfaChallenge('/login/mfa', null);

    await waitFor(() => {
      expect(screen.getByText('Tela de login')).toBeInTheDocument();
    });
  });

  it('completes the login and navigates to the fallback destination on a valid code', async () => {
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

    renderMfaChallenge('/login/mfa', { challengeToken: 'challenge-abc' });

    await user.type(screen.getByRole('textbox'), '123456');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => {
      expect(screen.getByText('Início')).toBeInTheDocument();
    });
    expect(postMock).toHaveBeenCalledWith('Auth/login/mfa', {
      challengeToken: 'challenge-abc',
      code: '123456',
    });
  });

  it('honors a validated returnTo destination on a valid code', async () => {
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
      },
    });

    renderMfaChallenge('/login/mfa?returnTo=%2Fcare%2Fresidents', {
      challengeToken: 'challenge-abc',
    });

    await user.type(screen.getByRole('textbox'), '123456');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => {
      expect(screen.getByText('Residentes')).toBeInTheDocument();
    });
  });

  it('shows an alert with the backend message on an invalid code', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: { status: 'mfa_required', challengeToken: 'challenge-abc' },
    });

    renderMfaChallenge('/login/mfa', { challengeToken: 'challenge-abc' });

    await user.type(screen.getByRole('textbox'), '000000');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => {
      expect(screen.getByText('Código inválido.')).toBeInTheDocument();
    });
  });
});
