import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import AppRoutes from './AppRoutes';

vi.mock('@/hooks/useAuth', () => ({
  default: () => ({ status: 'authenticated', hasPermission: () => true, identity: { email: 'ana@example.com' } }),
}));
vi.mock('@/features/api', () => ({
  api: { get: vi.fn().mockResolvedValue({ data: { items: [] } }), post: vi.fn(), put: vi.fn(), delete: vi.fn(), patch: vi.fn() },
  registerUnauthorizedHandler: vi.fn(),
}));

describe('application routes', () => {
  it('renders the configured public shell', async () => {
    render(<AppRoutes />);
    expect(await screen.findByAltText('logo do sistema')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Portal' })).toBeInTheDocument();
  });
});
