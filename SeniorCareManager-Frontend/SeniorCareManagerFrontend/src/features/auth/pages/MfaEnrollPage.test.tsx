import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
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
});
