import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, useLocation } from 'react-router-dom';
import Sidebar from './index';

vi.mock('@/hooks/useAuth', () => ({ default: () => ({ hasPermission: () => true }) }));

function CurrentPath() {
  return <output>{useLocation().pathname}</output>;
}

describe('Sidebar', () => {
  it('expands and navigates to the registrations catalogue', async () => {
    const user = userEvent.setup();
    render(<MemoryRouter initialEntries={['/']}><Sidebar /><CurrentPath /></MemoryRouter>);

    await user.click(screen.getByRole('button', { name: 'Expandir menu' }));
    await user.click(screen.getByRole('button', { name: 'Cadastros' }));

    expect(screen.getByText('/registrations')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Recolher menu' })).toBeInTheDocument();
  });
});
