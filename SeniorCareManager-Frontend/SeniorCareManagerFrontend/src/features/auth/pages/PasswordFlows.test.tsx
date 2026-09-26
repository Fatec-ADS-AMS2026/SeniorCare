import { describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import ResetPasswordPage from './ResetPasswordPage';
import ChangePasswordPage from './ChangePasswordPage';

const { resetPassword, changePassword } = vi.hoisted(() => ({
  resetPassword: vi.fn(),
  changePassword: vi.fn(),
}));
vi.mock('../services/authService', () => ({ default: { resetPassword, changePassword } }));
vi.mock('@/hooks/useAuth', () => ({ default: () => ({ identity: { email: 'ana@example.com' } }) }));

describe('password journeys', () => {
  it('resets a password from a recovery link', async () => {
    resetPassword.mockResolvedValueOnce({ success: true });
    const user = userEvent.setup();
    render(<MemoryRouter initialEntries={['/reset?email=ana@example.com&token=token']}><ResetPasswordPage /></MemoryRouter>);

    await user.type(screen.getByLabelText(/Nova senha/), 'SenhaSegura1!');
    await user.click(screen.getByRole('button', { name: 'Redefinir senha' }));

    await waitFor(() => expect(resetPassword).toHaveBeenCalledWith({
      email: 'ana@example.com', token: 'token', newPassword: 'SenhaSegura1!',
    }));
    expect(screen.getByText('Senha redefinida com sucesso.')).toBeInTheDocument();
  });

  it('changes the current password and clears secret fields', async () => {
    changePassword.mockResolvedValueOnce({ success: true });
    const user = userEvent.setup();
    render(<MemoryRouter><ChangePasswordPage /></MemoryRouter>);

    await user.type(screen.getByLabelText(/Senha atual/), 'Antiga1!');
    await user.type(screen.getByLabelText(/Nova senha/), 'Nova1!');
    await user.click(screen.getByRole('button', { name: 'Alterar senha' }));

    await waitFor(() => expect(changePassword).toHaveBeenCalledWith({
      email: 'ana@example.com', currentPassword: 'Antiga1!', newPassword: 'Nova1!',
    }));
    expect(screen.getByText('Senha alterada com sucesso.')).toBeInTheDocument();
    expect(screen.getByLabelText(/Senha atual/)).toHaveValue('');
  });
});
