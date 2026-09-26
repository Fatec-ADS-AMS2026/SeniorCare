import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, useLocation } from 'react-router-dom';
import { ThemeProvider } from '@/contexts/ThemeContext';
import AccessibilityPage from '@/pages/AccessibilityPage';
import Registrations from '@/pages/Registrations';
import Sidebar from '@/features/layout/components/Sidebar';
import Card from '@/components/Card';
import SearchBar from '@/components/SearchBar';

vi.mock('@/hooks/useAuth', () => ({ default: () => ({ hasPermission: () => true }) }));

function CurrentPath() {
  return <output>{useLocation().pathname}</output>;
}

describe('operational pages', () => {
  beforeEach(() => {
    vi.stubGlobal('localStorage', { getItem: () => null, setItem: vi.fn() });
  });

  it('persists accessibility controls through the theme provider', async () => {
    const user = userEvent.setup();
    render(<MemoryRouter><ThemeProvider><AccessibilityPage /></ThemeProvider></MemoryRouter>);
    await user.click(screen.getByRole('button', { name: 'Ativar alto contraste' }));
    await user.click(screen.getByRole('button', { name: 'Aumentar fonte' }));
    expect(document.documentElement.dataset.theme).toBe('high-contrast');
    expect(document.documentElement.style.getPropertyValue('--font-size')).toBe('18px');
  });

  it('shows all permitted stock catalogues', () => {
    render(<MemoryRouter><Registrations /></MemoryRouter>);
    expect(screen.getByText('Unidades de medidas cadastradas')).toBeInTheDocument();
    expect(screen.getByText('Fornecedores cadastrados')).toBeInTheDocument();
    expect(screen.getByText('Produtos cadastrados')).toBeInTheDocument();
  });

  it('expands the navigation and opens registrations', async () => {
    const user = userEvent.setup();
    render(<MemoryRouter initialEntries={['/']}><Sidebar /><CurrentPath /></MemoryRouter>);
    await user.click(screen.getByRole('button', { name: 'Expandir menu' }));
    await user.click(screen.getByRole('button', { name: 'Cadastros' }));
    expect(screen.getByText('/registrations')).toBeInTheDocument();
  });
  it('opens a catalogue card with the keyboard', async () => {
    const user = userEvent.setup();
    render(<MemoryRouter><Card text='Produtos' subText='Catálogo' icon={<span />} page='/products' /><CurrentPath /></MemoryRouter>);
    const card = screen.getByRole('button', { name: 'Produtos: Catálogo' });
    card.focus();
    await user.keyboard('{Enter}');
    expect(screen.getByText('/products')).toBeInTheDocument();
  });

  it('submits the entered catalogue search term', async () => {
    const user = userEvent.setup();
    const action = vi.fn();
    render(<SearchBar placeholder='Buscar' action={action} />);
    await user.type(screen.getByPlaceholderText('Buscar'), 'Ana');
    await user.click(screen.getByRole('button', { name: 'Buscar' }));
    expect(action).toHaveBeenCalledWith('Ana');
  });

});
