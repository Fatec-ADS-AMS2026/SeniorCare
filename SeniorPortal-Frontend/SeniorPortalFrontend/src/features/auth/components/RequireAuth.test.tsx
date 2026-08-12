import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import RequireAuth from './RequireAuth';
import { AuthProvider } from '@/contexts/AuthContext';

const getMock = vi.fn();

vi.mock('@/features/api', () => ({
  api: {
    get: (...args: unknown[]) => getMock(...args),
    post: vi.fn(),
    put: vi.fn(),
  },
  registerUnauthorizedHandler: vi.fn(),
}));

function renderAt(initialPath: string) {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <AuthProvider>
        <Routes>
          <Route path='/login' element={<div>Tela de login</div>} />
          <Route
            path='/'
            element={
              <RequireAuth>
                <div>Conteúdo protegido</div>
              </RequireAuth>
            }
          />
        </Routes>
      </AuthProvider>
    </MemoryRouter>
  );
}

describe('RequireAuth', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('redirects to /login (with returnTo) when there is no valid session', async () => {
    getMock.mockRejectedValueOnce({
      isAxiosError: true,
      response: { status: 401, data: {} },
    });

    renderAt('/');

    await waitFor(() => {
      expect(screen.getByText('Tela de login')).toBeInTheDocument();
    });
  });

  it('renders the protected content when authenticated with a valid institution', async () => {
    getMock.mockResolvedValueOnce({
      data: {
        userId: '1',
        institutionId: '1',
        institutionName: 'ILPI Teste',
        displayName: 'Fulana',
        email: 'fulana@example.com',
        roles: [],
        organizationalResponsibilities: [],
        effectivePermissions: [],
      },
    });

    renderAt('/');

    await waitFor(() => {
      expect(screen.getByText('Conteúdo protegido')).toBeInTheDocument();
    });
  });

  // §4.4 — spec.md "Contexto institucional divergente": sessão autenticada mas
  // sem instituição explícita nunca deve renderizar o catálogo.
  it('blocks rendering and shows an invalid-context message when institutionId is missing', async () => {
    getMock.mockResolvedValueOnce({
      data: {
        userId: '1',
        institutionId: '',
        institutionName: '',
        displayName: 'Fulana',
        email: 'fulana@example.com',
        roles: [],
        organizationalResponsibilities: [],
        effectivePermissions: [],
      },
    });

    renderAt('/');

    await waitFor(() => {
      expect(screen.getByText('Contexto institucional inválido')).toBeInTheDocument();
    });
    expect(screen.queryByText('Conteúdo protegido')).not.toBeInTheDocument();
    expect(screen.queryByText('Tela de login')).not.toBeInTheDocument();
  });
});
