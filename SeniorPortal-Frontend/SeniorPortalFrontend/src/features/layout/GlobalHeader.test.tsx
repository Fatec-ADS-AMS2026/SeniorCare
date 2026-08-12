import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { axe } from 'jest-axe';
import GlobalHeader from './GlobalHeader';
import { AuthProvider } from '@/contexts/AuthContext';
import { RuntimeConfigProvider } from '@/contexts/RuntimeConfigContext';

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

function renderHeader() {
  vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 } as Response));
  return render(
    <MemoryRouter initialEntries={['/']}>
      <RuntimeConfigProvider>
        <AuthProvider>
          <GlobalHeader />
        </AuthProvider>
      </RuntimeConfigProvider>
    </MemoryRouter>
  );
}

describe('GlobalHeader', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    getMock.mockResolvedValue({
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
  });

  it('renders global navigation links and calls logout', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({ data: { message: 'ok' } });

    renderHeader();

    expect(await screen.findByRole('link', { name: 'Catálogo' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Perfil' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Segurança' })).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: /Sair/ }));

    await waitFor(() => {
      expect(postMock).toHaveBeenCalledWith('Auth/logout');
    });
  });

  it('has no detectable accessibility violations', async () => {
    const { container } = renderHeader();
    await screen.findByRole('link', { name: 'Catálogo' });

    expect(await axe(container)).toHaveNoViolations();
  });
});
