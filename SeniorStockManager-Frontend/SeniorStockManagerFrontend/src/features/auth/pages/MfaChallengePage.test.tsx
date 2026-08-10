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

function renderMfaChallenge(state: unknown) {
  return render(
    <MemoryRouter
      initialEntries={[{ pathname: '/login/mfa', state }]}
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
