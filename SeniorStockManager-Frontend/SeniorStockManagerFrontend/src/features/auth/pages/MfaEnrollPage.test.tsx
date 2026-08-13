import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import MfaEnrollPage from './MfaEnrollPage';
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

function renderMfaEnroll(state: unknown, search?: string) {
  return render(
    <MemoryRouter initialEntries={[{ pathname: '/mfa/enroll', search, state }]}>
      <AuthProvider>
        <Routes>
          <Route path='/login' element={<div>Tela de login</div>} />
          <Route path='/mfa/enroll' element={<MfaEnrollPage />} />
          <Route path='/admin' element={<div>Visão Geral</div>} />
        </Routes>
      </AuthProvider>
    </MemoryRouter>
  );
}

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

describe('MfaEnrollPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    getMock.mockRejectedValue({
      isAxiosError: true,
      response: { status: 401, data: {} },
    });
  });

  it('redirects back to /login without a challengeToken and no session', async () => {
    renderMfaEnroll(null);

    await waitFor(() => {
      expect(screen.getByText('Tela de login')).toBeInTheDocument();
    });
  });

  it('starts enrollment and shows recovery codes on a valid confirmation code', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: { authenticatorKey: 'ABC123', otpAuthUri: 'otpauth://totp/SeniorCare' },
    });
    postMock.mockResolvedValueOnce({
      data: {
        recoveryCodes: ['aaaa-1111', 'bbbb-2222'],
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

    renderMfaEnroll({ challengeToken: 'challenge-abc' });

    await waitFor(() => {
      expect(screen.getByText('otpauth://totp/SeniorCare')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Código de confirmação', { exact: false }), '123456');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => {
      expect(screen.getByText('aaaa-1111')).toBeInTheDocument();
    });
    expect(screen.getByText('bbbb-2222')).toBeInTheDocument();
  });

  // §7.4 — "Concluir" honra o returnTo cruzado pendente via navegação de
  // página inteira, não navigate().
  it('does a full-page navigation to a validated cross-app returnTo when finishing enrollment', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: { authenticatorKey: 'ABC123', otpAuthUri: 'otpauth://totp/SeniorCare' },
    });
    postMock.mockResolvedValueOnce({
      data: {
        recoveryCodes: ['aaaa-1111'],
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
    const assignSpy = mockLocationAssign();

    renderMfaEnroll({ challengeToken: 'challenge-abc' }, '?returnTo=%2Fcare%2Fresidents');

    await waitFor(() => {
      expect(screen.getByText('otpauth://totp/SeniorCare')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Código de confirmação', { exact: false }), '123456');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => {
      expect(screen.getByText('aaaa-1111')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Concluir' }));

    await waitFor(() => {
      expect(assignSpy).toHaveBeenCalledWith('/care/residents');
    });
    expect(screen.queryByText('Visão Geral')).not.toBeInTheDocument();

    restoreLocation();
  });
});
