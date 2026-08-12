import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { axe } from 'jest-axe';
import SecurityPage from './SecurityPage';

const postMock = vi.fn();

vi.mock('@/features/api', () => ({
  api: {
    get: vi.fn(),
    post: (...args: unknown[]) => postMock(...args),
    put: vi.fn(),
  },
  registerUnauthorizedHandler: vi.fn(),
}));

describe('SecurityPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('shows the backend error message on an incorrect current password', async () => {
    const user = userEvent.setup();
    postMock.mockRejectedValueOnce({
      isAxiosError: true,
      response: { status: 401, data: { message: 'Senha atual incorreta.' } },
    });

    render(<SecurityPage />);

    await user.type(screen.getByLabelText('Senha atual'), 'senha-errada');
    await user.click(screen.getByRole('button', { name: 'Gerar novos códigos' }));

    await waitFor(() => {
      expect(screen.getByText('Senha atual incorreta.')).toBeInTheDocument();
    });
  });

  it('shows new recovery codes on success', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: { recoveryCodes: ['aaaa-1111', 'bbbb-2222'] },
    });

    render(<SecurityPage />);

    await user.type(screen.getByLabelText('Senha atual'), 'senha-correta');
    await user.click(screen.getByRole('button', { name: 'Gerar novos códigos' }));

    await waitFor(() => {
      expect(screen.getByText('aaaa-1111')).toBeInTheDocument();
    });
    expect(screen.getByText('bbbb-2222')).toBeInTheDocument();
    expect(postMock).toHaveBeenCalledWith('Auth/mfa/recovery-codes/regenerate', {
      currentPassword: 'senha-correta',
    });
  });

  it('has no detectable accessibility violations', async () => {
    const { container } = render(<SecurityPage />);
    expect(await axe(container)).toHaveNoViolations();
  });
});
