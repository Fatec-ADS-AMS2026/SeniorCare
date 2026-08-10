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
    delete: vi.fn(),
    patch: vi.fn(),
  },
  registerUnauthorizedHandler: vi.fn(),
}));

// A rota protegida precisa existir em `routes` (RequireAuth faz matchPath contra
// @/routes/routes de verdade) — /carrier já é uma rota real com
// requiredPermission: {resource:'Carrier', action:'read'} (§10.6).
function renderAt(initialPath: string) {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <AuthProvider>
        <Routes>
          <Route path='/login' element={<div>Tela de login</div>} />
          <Route
            path='/carrier'
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

  it('redirects to /login when there is no valid session', async () => {
    getMock.mockRejectedValueOnce({
      isAxiosError: true,
      response: { status: 401, data: {} },
    });

    renderAt('/carrier');

    await waitFor(() => {
      expect(screen.getByText('Tela de login')).toBeInTheDocument();
    });
  });

  it('renders the protected content when authenticated with the required permission', async () => {
    getMock.mockResolvedValueOnce({
      data: {
        userId: '1',
        institutionId: '1',
        institutionName: 'ILPI Teste',
        displayName: 'Fulana',
        email: 'fulana@example.com',
        roles: [],
        organizationalResponsibilities: [],
        effectivePermissions: [{ resource: 'Carrier', action: 'read' }],
      },
    });

    renderAt('/carrier');

    await waitFor(() => {
      expect(screen.getByText('Conteúdo protegido')).toBeInTheDocument();
    });
  });

  it('shows "Acesso negado" inline (no redirect) when authenticated without the required permission', async () => {
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

    renderAt('/carrier');

    await waitFor(() => {
      expect(screen.getByText('Acesso negado')).toBeInTheDocument();
    });
    expect(screen.queryByText('Tela de login')).not.toBeInTheDocument();
    expect(screen.queryByText('Conteúdo protegido')).not.toBeInTheDocument();
  });
});
