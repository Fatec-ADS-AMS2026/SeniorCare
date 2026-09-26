import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ThemeProvider } from '@/contexts/ThemeContext';
import AccessibilityPage from '@/pages/AccessibilityPage';
import AdminOverview from '@/pages/Admin/AdminOverview';
import Registrations from '@/pages/Registrations';
import InstitutionSecurityPage from '@/features/institutionSecurity/pages/InstitutionSecurityPage';
import UserSessionOverview from '@/features/userSession/pages/UserSessionOverview';
import Card from '@/components/Card';
import { AppLayout } from '@/features/layouts';
import SearchBar from '@/components/SearchBar';

const { getMock, putMock } = vi.hoisted(() => ({ getMock: vi.fn(), putMock: vi.fn() }));

vi.mock('@/features/api', () => ({
  api: { get: getMock, put: putMock, post: vi.fn(), delete: vi.fn(), patch: vi.fn() },
  registerUnauthorizedHandler: vi.fn(),
}));
vi.mock('@/hooks/useAuth', () => ({ default: () => ({ hasPermission: () => true }) }));

const policy = {
  lockoutDurationMinutes: 15,
  maxFailedAttempts: 5,
  accessTokenDurationMinutes: 30,
  refreshTokenDurationDays: 7,
  mfaRequiredForAllUsers: false,
};

describe('operational page journeys', () => {
  beforeEach(() => {
    vi.stubGlobal('localStorage', {
      getItem: () => null,
      setItem: vi.fn(),
    });
  });

  it('updates institution security after reauthentication', async () => {
    getMock.mockResolvedValueOnce({ data: policy });
    putMock.mockResolvedValueOnce({ data: { ...policy, maxFailedAttempts: 6 } });
    const user = userEvent.setup();
    render(<MemoryRouter><InstitutionSecurityPage /></MemoryRouter>);

    await screen.findByDisplayValue('5');
    await user.clear(screen.getByLabelText(/Máximo de tentativas/));
    await user.type(screen.getByLabelText(/Máximo de tentativas/), '6');
    await user.click(screen.getByRole('button', { name: 'Salvar' }));
    await user.type(screen.getByLabelText(/Sua senha atual/), 'SenhaSegura1!');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => expect(putMock).toHaveBeenCalledWith(
      'AdminInstitutionSecurity/',
      expect.objectContaining({ maxFailedAttempts: 6, currentPassword: 'SenhaSegura1!' })
    ));
    expect(await screen.findByText('Parâmetros de segurança atualizados com sucesso!')).toBeInTheDocument();
  });

  it('revokes every session for the selected user after reauthentication', async () => {
    getMock.mockResolvedValue({ data: { items: [{
      id: 'session-1', userId: 'user-1', ipAddress: '127.0.0.1', userAgent: 'Browser',
      createdAtUtc: '2025-01-01T00:00:00Z', lastSeenAtUtc: '2025-01-01T00:00:00Z',
    }] } });
    putMock.mockResolvedValueOnce({ data: undefined });
    const user = userEvent.setup();
    render(
      <MemoryRouter initialEntries={['/users/user-1/sessions']}>
        <Routes><Route path='/users/:userId/sessions' element={<UserSessionOverview />} /></Routes>
      </MemoryRouter>
    );

    await screen.findByText('127.0.0.1');
    await user.click(screen.getByRole('button', { name: 'Revogar todas' }));
    await user.type(screen.getByLabelText(/Sua senha atual/), 'SenhaSegura1!');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => expect(putMock).toHaveBeenCalledWith(
      'AdminUserSession/revoke-all?userId=user-1',
      { currentPassword: 'SenhaSegura1!' }
    ));
    expect(await screen.findByText('Sessão(ões) revogada(s) com sucesso.')).toBeInTheDocument();
  });

  it('persists accessibility controls through the theme provider', async () => {
    const user = userEvent.setup();
    render(<MemoryRouter><ThemeProvider><AccessibilityPage /></ThemeProvider></MemoryRouter>);

    await user.click(screen.getByRole('button', { name: 'Ativar alto contraste' }));
    await user.click(screen.getByRole('button', { name: 'Aumentar fonte' }));

    expect(document.documentElement.dataset.theme).toBe('high-contrast');
    expect(document.documentElement.style.getPropertyValue('--font-size')).toBe('18px');
  });

  it('shows every permitted administrative and registration destination', () => {
    render(
      <MemoryRouter>
        <AdminOverview />
        <Registrations />
      </MemoryRouter>
    );

    expect(screen.getByRole('button', { name: /Usuários:/ })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Plano de Saúde:/ })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Cargo:/ })).toBeInTheDocument();
  });

  it('opens a catalogue card with the keyboard', async () => {
    const user = userEvent.setup();
    render(<MemoryRouter><Card text='Cadastros' subText='Catálogo' icon={<span />} page='/registrations' /></MemoryRouter>);
    const card = screen.getByRole('button', { name: 'Cadastros: Catálogo' });
    card.focus();
    await user.keyboard('{Enter}');
  });

  it('opens a catalogue card with the mouse', async () => {
    const user = userEvent.setup();
    render(<MemoryRouter><Card text='Cadastros' subText='Catálogo' icon={<span />} page='/registrations' /></MemoryRouter>);
    await user.click(screen.getByRole('button', { name: 'Cadastros: Catálogo' }));
  });

  it('renders the authenticated application layout around its outlet', () => {
    render(
      <MemoryRouter>
        <Routes><Route element={<AppLayout />}><Route index element={<p>Conteúdo</p>} /></Route></Routes>
      </MemoryRouter>
    );
    expect(screen.getByText('Conteúdo')).toBeInTheDocument();
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
