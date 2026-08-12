import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { axe } from 'jest-axe';
import ProfilePage from './ProfilePage';
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

describe('ProfilePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders the restored identity', async () => {
    getMock.mockResolvedValueOnce({
      data: {
        userId: '1',
        institutionId: '1',
        institutionName: 'ILPI Jardim das Flores',
        displayName: 'Fulana de Tal',
        email: 'fulana@example.com',
        roles: ['Administrador'],
        organizationalResponsibilities: [],
        effectivePermissions: [],
      },
    });

    render(
      <AuthProvider>
        <ProfilePage />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByText('Fulana de Tal')).toBeInTheDocument();
    });
    expect(screen.getByText('fulana@example.com')).toBeInTheDocument();
    expect(screen.getByText('ILPI Jardim das Flores')).toBeInTheDocument();
    expect(screen.getByText('Administrador')).toBeInTheDocument();
  });

  it('renders nothing while there is no restored identity', () => {
    getMock.mockRejectedValue({ isAxiosError: true, response: { status: 401, data: {} } });

    const { container } = render(
      <AuthProvider>
        <ProfilePage />
      </AuthProvider>
    );

    expect(container).toBeEmptyDOMElement();
  });

  it('has no detectable accessibility violations', async () => {
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

    const { container } = render(
      <AuthProvider>
        <ProfilePage />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByText('Fulana')).toBeInTheDocument();
    });
    expect(await axe(container)).toHaveNoViolations();
  });
});
