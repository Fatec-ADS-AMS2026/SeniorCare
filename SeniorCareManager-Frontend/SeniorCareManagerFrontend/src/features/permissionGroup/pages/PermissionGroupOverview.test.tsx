import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import PermissionGroupOverview from './PermissionGroupOverview';

const getMock = vi.fn();
const postMock = vi.fn();

vi.mock('@/features/api', () => ({
  api: {
    get: (...args: unknown[]) => getMock(...args),
    post: (...args: unknown[]) => postMock(...args),
    put: vi.fn(),
    delete: vi.fn(),
    patch: vi.fn(),
  },
  registerUnauthorizedHandler: vi.fn(),
}));

function renderOverview() {
  return render(
    <MemoryRouter>
      <PermissionGroupOverview />
    </MemoryRouter>
  );
}

describe('PermissionGroupOverview', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    getMock.mockResolvedValue({
      data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
    });
  });

  it('surfaces a 422 (ValidationProblemDetails) field error from the backend', async () => {
    const user = userEvent.setup();
    postMock.mockRejectedValueOnce({
      isAxiosError: true,
      response: {
        status: 400,
        data: {
          type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
          title: 'One or more validation errors occurred.',
          status: 400,
          errors: { Name: ['The Name field is required.'] },
        },
      },
    });

    renderOverview();

    await waitFor(() => expect(getMock).toHaveBeenCalled());

    await user.click(screen.getByRole('button', { name: 'Adicionar' }));
    const nameInput = document.querySelector(
      'input[name="name"]'
    ) as HTMLInputElement;
    await user.type(nameInput, 'Grupo Teste');
    await user.click(screen.getByRole('button', { name: 'Salvar' }));

    await waitFor(() => {
      expect(
        screen.getByText('One or more validation errors occurred.')
      ).toBeInTheDocument();
    });
  });
});
