import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import RoleOverview from './RoleOverview';

const getMock = vi.fn();
const postMock = vi.fn();
const putMock = vi.fn();
const deleteMock = vi.fn();

vi.mock('@/features/api', () => ({
  api: {
    get: (...args: unknown[]) => getMock(...args),
    post: (...args: unknown[]) => postMock(...args),
    put: (...args: unknown[]) => putMock(...args),
    delete: (...args: unknown[]) => deleteMock(...args),
    patch: vi.fn(),
  },
  registerUnauthorizedHandler: vi.fn(),
}));

const existingRole = {
  id: '11111111-1111-1111-1111-111111111111',
  institutionId: '22222222-2222-2222-2222-222222222222',
  name: 'Cuidador',
  rowVersion: 3,
};

// BreadcrumbPageTitle usa useLocation por baixo — precisa de um Router mesmo sem
// nenhuma navegação real acontecer no teste.
function renderRoleOverview() {
  return render(
    <MemoryRouter>
      <RoleOverview />
    </MemoryRouter>
  );
}

describe('RoleOverview', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    getMock.mockResolvedValue({
      data: { items: [existingRole], page: 1, pageSize: 20, totalCount: 1 },
    });
  });

  it('creates a role', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: { ...existingRole, id: '33333333-3333-3333-3333-333333333333', name: 'Enfermeira' },
    });

    renderRoleOverview();

    await waitFor(() => expect(screen.getByText('Cuidador')).toBeInTheDocument());

    await user.click(screen.getByRole('button', { name: 'Adicionar' }));
    // Duas caixas de texto na tela ao mesmo tempo (busca + modal) — mira o campo
    // do formulário pelo atributo name em vez de getByRole('textbox').
    const nameInput = document.querySelector(
      'input[name="name"]'
    ) as HTMLInputElement;
    await user.type(nameInput, 'Enfermeira');
    await user.click(screen.getByRole('button', { name: 'Salvar' }));

    await waitFor(() => {
      expect(postMock).toHaveBeenCalledWith('AdminRole/', { name: 'Enfermeira' });
    });
  });

  it('shows a friendly message on a 409 (stale RowVersion) when editing', async () => {
    const user = userEvent.setup();
    putMock.mockRejectedValueOnce({
      isAxiosError: true,
      response: {
        status: 409,
        data: {
          type: 'https://seniorcare.dev/erros/conflito-concorrencia',
          title: 'Conflito de concorrência.',
          status: 409,
          detail: 'Releia o recurso (GET) para obter a versão atual antes de tentar novamente.',
        },
      },
    });

    renderRoleOverview();

    await waitFor(() => expect(screen.getByText('Cuidador')).toBeInTheDocument());

    await user.click(screen.getByRole('button', { name: 'Editar' }));
    const nameInput = document.querySelector(
      'input[name="name"]'
    ) as HTMLInputElement;
    await user.clear(nameInput);
    await user.type(nameInput, 'Cuidador Sênior');
    await user.click(screen.getByRole('button', { name: 'Salvar' }));

    await waitFor(() => {
      expect(
        screen.getByText(
          'Releia o recurso (GET) para obter a versão atual antes de tentar novamente.'
        )
      ).toBeInTheDocument();
    });
  });

  it('deletes a role', async () => {
    const user = userEvent.setup();
    deleteMock.mockResolvedValueOnce({ data: undefined });

    renderRoleOverview();

    await waitFor(() => expect(screen.getByText('Cuidador')).toBeInTheDocument());

    await user.click(screen.getByRole('button', { name: 'Excluir' }));
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => {
      expect(deleteMock).toHaveBeenCalledWith(`AdminRole/${existingRole.id}`);
    });
  });
});
