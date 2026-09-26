import { describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import ActivateAccountPage from './ActivateAccountPage';
import RecoverAccountPage from './RecoverAccountPage';

const { activate, recover } = vi.hoisted(() => ({ activate: vi.fn(), recover: vi.fn() }));
vi.mock('../services/authService', () => ({ default: { activate, recover } }));

describe('account recovery journeys', () => {
  it('activates an account from the activation link', async () => {
    activate.mockResolvedValueOnce({ success: true });
    const user = userEvent.setup();
    render(<MemoryRouter initialEntries={['/activate?email=a@b.com&token=t']}><ActivateAccountPage /></MemoryRouter>);
    await user.type(screen.getByLabelText(/Defina sua senha/), 'SenhaSegura1!');
    await user.click(screen.getByRole('button', { name: 'Ativar conta' }));
    await waitFor(() => expect(screen.getByText('Conta ativada com sucesso.')).toBeInTheDocument());
  });

  it('returns the neutral recovery message', async () => {
    recover.mockResolvedValueOnce({ success: true, data: {} });
    const user = userEvent.setup();
    render(<MemoryRouter><RecoverAccountPage /></MemoryRouter>);
    await user.type(screen.getByLabelText(/E-mail/), 'a@b.com');
    await user.click(screen.getByRole('button', { name: 'Enviar instruções' }));
    await waitFor(() => expect(screen.getByText(/conta elegível/)).toBeInTheDocument());
  });
});
