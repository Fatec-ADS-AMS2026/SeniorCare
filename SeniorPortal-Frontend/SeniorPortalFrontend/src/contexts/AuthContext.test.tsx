import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { AuthProvider } from './AuthContext';
import useAuth from '@/hooks/useAuth';

// §4.6 — mock de @/features/api, capturando o callback que
// registerUnauthorizedHandler recebe pra exercitar a lógica de 401 sem testar o
// interceptor do axios em si (baixo risco, só repassa a chamada).
const getMock = vi.fn();
const postMock = vi.fn();
let capturedHandler: (() => void) | null = null;

vi.mock('@/features/api', () => ({
  api: {
    get: (...args: unknown[]) => getMock(...args),
    post: (...args: unknown[]) => postMock(...args),
    put: vi.fn(),
  },
  registerUnauthorizedHandler: vi.fn((handler: (() => void) | null) => {
    capturedHandler = handler;
  }),
}));

function Consumer() {
  const { status, identity, hasPermission } = useAuth();
  return (
    <div>
      <span data-testid='status'>{status}</span>
      <span data-testid='identity'>{identity?.displayName ?? 'none'}</span>
      <span data-testid='institution'>{identity?.institutionId ?? 'none'}</span>
      <span data-testid='has-module-care'>
        {hasPermission('Module', 'care') ? 'yes' : 'no'}
      </span>
    </div>
  );
}

const identity = {
  userId: '1',
  institutionId: '1',
  institutionName: 'ILPI Teste',
  displayName: 'Fulana de Tal',
  email: 'fulana@example.com',
  roles: ['Administrador'],
  organizationalResponsibilities: [],
  effectivePermissions: [{ resource: 'Module', action: 'care' }],
};

describe('AuthProvider', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    capturedHandler = null;
    Object.defineProperty(window, 'location', {
      value: { pathname: '/', assign: vi.fn() },
      writable: true,
    });
  });

  it('restores the session from GET /auth/me and exposes hasPermission against EffectivePermissions', async () => {
    getMock.mockResolvedValueOnce({ data: identity });

    render(
      <AuthProvider>
        <Consumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('status').textContent).toBe('authenticated');
    });

    expect(screen.getByTestId('identity').textContent).toBe('Fulana de Tal');
    expect(screen.getByTestId('institution').textContent).toBe('1');
    expect(screen.getByTestId('has-module-care').textContent).toBe('yes');
  });

  it('becomes anonymous when there is no valid session', async () => {
    getMock.mockRejectedValueOnce({
      isAxiosError: true,
      response: { status: 401, data: {} },
    });

    render(
      <AuthProvider>
        <Consumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('status').textContent).toBe('anonymous');
    });
    expect(screen.getByTestId('identity').textContent).toBe('none');
  });

  it('registers a 401 handler that clears the session and redirects, unless already on /login', async () => {
    getMock.mockResolvedValueOnce({ data: identity });

    render(
      <AuthProvider>
        <Consumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('status').textContent).toBe('authenticated');
    });
    expect(capturedHandler).toBeInstanceOf(Function);

    // §4.3 — expiração não renovável: um 401 vindo de qualquer chamada
    // protegida limpa TODO o contexto local (sem tentativa de renovação) e
    // redireciona pro /login.
    capturedHandler!();

    await waitFor(() => {
      expect(screen.getByTestId('status').textContent).toBe('anonymous');
    });
    expect(screen.getByTestId('identity').textContent).toBe('none');
    expect(window.location.assign).toHaveBeenCalledWith('/login');
  });

  it('does not redirect again if the 401 handler fires while already on /login (no loop)', async () => {
    Object.defineProperty(window, 'location', {
      value: { pathname: '/login', assign: vi.fn() },
      writable: true,
    });
    getMock.mockResolvedValueOnce({ data: identity });

    render(
      <AuthProvider>
        <Consumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('status').textContent).toBe('authenticated');
    });

    capturedHandler!();

    await waitFor(() => {
      expect(screen.getByTestId('status').textContent).toBe('anonymous');
    });
    expect(window.location.assign).not.toHaveBeenCalled();
  });
});
