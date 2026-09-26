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

describe('MfaEnrollPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    getMock.mockRejectedValue({ isAxiosError: true, response: { status: 401, data: {} } });
  });

  it('renders a QR code and preserves the manual authenticator key', async () => {
    postMock.mockResolvedValueOnce({
      data: {
        authenticatorKey: 'ABC123',
        otpAuthUri: 'otpauth://totp/SeniorCare?secret=ABC123',
      },
    });

    render(
      <MemoryRouter initialEntries={[{ pathname: '/mfa/enroll', state: { challengeToken: 'challenge' } }]}>
        <AuthProvider>
          <Routes>
            <Route path='/mfa/enroll' element={<MfaEnrollPage />} />
            <Route path='/login' element={<div>Login</div>} />
          </Routes>
        </AuthProvider>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole('img', { name: 'Código QR para configurar MFA' })).toHaveAttribute(
        'src',
        expect.stringMatching(/^data:image\/png;base64,/)
      );
    });
    expect(screen.getByText('ABC123')).toBeInTheDocument();
  });

  it('confirms the authenticator code and shows recovery codes once', async () => {
    postMock
      .mockResolvedValueOnce({
        data: { authenticatorKey: 'ABC123', otpAuthUri: 'otpauth://totp/SeniorCare?secret=ABC123' },
      })
      .mockResolvedValueOnce({
        data: { recoveryCodes: ['RECOVERY-1', 'RECOVERY-2'] },
      });
    const user = userEvent.setup();
    render(
      <MemoryRouter initialEntries={[{ pathname: '/mfa/enroll', state: { challengeToken: 'challenge' } }]}>
        <AuthProvider><Routes><Route path='/mfa/enroll' element={<MfaEnrollPage />} /></Routes></AuthProvider>
      </MemoryRouter>
    );

    await screen.findByText('ABC123');
    await user.type(screen.getByLabelText(/Código/), '123456');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    expect(await screen.findByText('Guarde seus códigos de recuperação')).toBeInTheDocument();
    expect(screen.getByText('RECOVERY-1')).toBeInTheDocument();
  });
});
