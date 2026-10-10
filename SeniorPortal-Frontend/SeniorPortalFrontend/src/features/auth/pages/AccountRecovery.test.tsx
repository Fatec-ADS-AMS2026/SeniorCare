import { describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import ActivateAccountPage from './ActivateAccountPage';
import RecoverAccountPage from './RecoverAccountPage';
import ResetPasswordPage from './ResetPasswordPage';

const { activate, recover, resetPassword } = vi.hoisted(() => ({
  activate: vi.fn(),
  recover: vi.fn(),
  resetPassword: vi.fn(),
}));

vi.mock('../services/authService', () => ({ default: { activate, recover, resetPassword } }));

describe('account recovery journeys', () => {
  it('activates an account from an email link', async () => {
    activate.mockResolvedValueOnce({ success: true });
    const user = userEvent.setup();
    render(
      <MemoryRouter initialEntries={['/ativar-conta?email=admin@example.com&token=activation-token']}>
        <ActivateAccountPage />
      </MemoryRouter>
    );

    await user.type(screen.getByLabelText('Defina sua senha'), 'SenhaSegura1!');
    await user.click(screen.getByRole('button', { name: 'Ativar conta' }));

    await waitFor(() =>
      expect(activate).toHaveBeenCalledWith({
        email: 'admin@example.com',
        token: 'activation-token',
        newPassword: 'SenhaSegura1!',
      })
    );
    expect(screen.getByText('Conta ativada com sucesso.')).toBeInTheDocument();
  });

  it('resets a password from an email link', async () => {
    resetPassword.mockResolvedValueOnce({ success: true });
    const user = userEvent.setup();
    render(
      <MemoryRouter initialEntries={['/redefinir-senha?email=admin@example.com&token=recovery-token']}>
        <ResetPasswordPage />
      </MemoryRouter>
    );

    await user.type(screen.getByLabelText('Nova senha'), 'OutraSenhaSegura1!');
    await user.click(screen.getByRole('button', { name: 'Redefinir senha' }));

    await waitFor(() =>
      expect(resetPassword).toHaveBeenCalledWith({
        email: 'admin@example.com',
        token: 'recovery-token',
        newPassword: 'OutraSenhaSegura1!',
      })
    );
    expect(screen.getByText('Senha redefinida com sucesso.')).toBeInTheDocument();
  });

  it('shows a neutral recovery response', async () => {
    recover.mockResolvedValueOnce({ success: true, data: {} });
    const user = userEvent.setup();
    render(
      <MemoryRouter>
        <RecoverAccountPage />
      </MemoryRouter>
    );

    await user.type(screen.getByLabelText('E-mail'), 'admin@example.com');
    await user.click(screen.getByRole('button', { name: 'Enviar instruções' }));

    await waitFor(() =>
      expect(screen.getByText(/Se o e-mail informado tiver uma conta elegível/)).toBeInTheDocument()
    );
  });
});
