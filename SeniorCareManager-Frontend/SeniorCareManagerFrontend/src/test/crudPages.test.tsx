import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import HealthInsurancePlanOverview from '@/features/healthInsurancePlan/pages/HealthInsurancePlanOverview';
import PositionOverview from '@/features/position/pages/PositionOverview';
import OrganizationalRoleOverview from '@/features/organizationalRole/pages/OrganizationalRoleOverview';
import AdminUserOverview from '@/features/adminUser/pages/AdminUserOverview';
import AccessPolicyOverview from '@/features/accessPolicy/pages/AccessPolicyOverview';
import OrganizationalRoleAssignmentOverview from '@/features/organizationalRoleAssignment/pages/OrganizationalRoleAssignmentOverview';
import ReligionOverview from '@/features/religion/pages/ReligionOverview';

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

const pages = [
  ['planos de saúde', 'Adicionar', HealthInsurancePlanOverview],
  ['cargos', 'Adicionar', PositionOverview],
  ['papéis organizacionais', 'Adicionar', OrganizationalRoleOverview],
  ['usuários administrativos', 'Adicionar', AdminUserOverview],
  ['políticas de acesso', 'Criar política', AccessPolicyOverview],
  ['atribuições organizacionais', 'Atribuir', OrganizationalRoleAssignmentOverview],
] as const;

describe('páginas CRUD assistenciais', () => {
  beforeEach(() => {
    getMock.mockResolvedValue({
      data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
    });
  });

  it.each(pages)('renders the empty %s catalogue and exposes its create action', async (_, action, Page) => {
    render(
      <MemoryRouter>
        <Page />
      </MemoryRouter>
    );

    await waitFor(() => expect(getMock).toHaveBeenCalled());
    expect(screen.getByRole('button', { name: action })).toBeEnabled();
  });

  it.each([
    ['cargo', PositionOverview, { id: 0, name: 'Enfermeiro' }],
    ['papel organizacional', OrganizationalRoleOverview, { id: 'role-1', name: 'Coordenador' }],
  ] as const)(
    'creates a %s through its public form',
    async (_, Page, model) => {
      const user = userEvent.setup();
      postMock.mockResolvedValueOnce({ data: model });

      render(
        <MemoryRouter>
          <Page />
        </MemoryRouter>
      );

      await waitFor(() => expect(getMock).toHaveBeenCalled());
      await user.click(screen.getByRole('button', { name: 'Adicionar' }));
      await user.type(document.querySelector('input[name="name"]')!, model.name);
      await user.click(screen.getByRole('button', { name: 'Salvar' }));

      await waitFor(() => expect(postMock).toHaveBeenCalled());
    }
  );

  it('creates a health plan through its public form', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: { id: 1, name: 'Saúde Municipal', abbreviation: 'SMS', type: 0 },
    });
    render(<MemoryRouter><HealthInsurancePlanOverview /></MemoryRouter>);

    await waitFor(() => expect(getMock).toHaveBeenCalled());
    await user.click(screen.getByRole('button', { name: 'Adicionar' }));
    await user.type(document.querySelector('input[name="name"]')!, 'Saúde Municipal');
    await user.type(document.querySelector('input[name="abbreviation"]')!, 'SMS');
    await user.selectOptions(document.querySelector('select[name="type"]')!, '1');
    await user.click(screen.getByRole('button', { name: 'Salvar' }));

    await waitFor(() => expect(postMock).toHaveBeenCalled());
  });

  it('blocks a duplicate health plan name before sending it to the API', async () => {
    getMock.mockResolvedValue({
      data: { items: [{ id: 1, name: 'Saúde Municipal', abbreviation: 'SMS', type: 0 }], page: 1, pageSize: 20, totalCount: 1 },
    });
    const user = userEvent.setup();
    render(<MemoryRouter><HealthInsurancePlanOverview /></MemoryRouter>);

    await screen.findByText('Saúde Municipal');
    await user.click(screen.getByRole('button', { name: 'Adicionar' }));
    await user.type(document.querySelector('input[name="name"]')!, 'Saúde Municipal');
    await user.type(document.querySelector('input[name="abbreviation"]')!, 'SM');
    postMock.mockClear();
    await user.click(screen.getByRole('button', { name: 'Salvar' }));

    expect(await screen.findByText('Já existe um Plano de Saúde com esse nome.')).toBeInTheDocument();
    expect(postMock).not.toHaveBeenCalled();
  });

  it('filters listed health plans by name', async () => {
    getMock.mockResolvedValue({
      data: {
        items: [
          { id: 1, name: 'Saúde Municipal', abbreviation: 'SMS', type: 0 },
          { id: 2, name: 'Saúde Estadual', abbreviation: 'SES', type: 1 },
        ],
        page: 1,
        pageSize: 20,
        totalCount: 2,
      },
    });
    const user = userEvent.setup();
    render(<MemoryRouter><HealthInsurancePlanOverview /></MemoryRouter>);

    await screen.findByText('Saúde Municipal');
    await user.type(screen.getByPlaceholderText('Buscar plano de saúde...'), 'Municipal');
    await user.click(screen.getByRole('button', { name: 'Buscar' }));

    expect(screen.getByText('Saúde Municipal')).toBeInTheDocument();
    expect(screen.queryByText('Saúde Estadual')).not.toBeInTheDocument();
  });

  it('activates a drafted access policy', async () => {
    getMock.mockResolvedValue({
      data: { items: [{ id: 'policy-1', resource: 'Resident', action: 'Read', version: 1, effect: 1, state: 1 }] },
    });
    putMock.mockResolvedValueOnce({ data: {} });
    const user = userEvent.setup();
    render(<MemoryRouter><AccessPolicyOverview /></MemoryRouter>);

    await screen.findByText('Resident.Read');
    await user.click(screen.getByTitle('Ativar'));

    await waitFor(() => expect(putMock).toHaveBeenCalledWith('AdminAccessPolicy/policy-1/activate'));
    expect(await screen.findByText('Política ativada com sucesso!')).toBeInTheDocument();
  });

  it('ends an organizational role assignment', async () => {
    getMock.mockImplementation((url: string) => {
      if (url === 'AdminOrganizationalRoleAssignment/') {
        return Promise.resolve({ data: { items: [{ id: 'assignment-1', userId: 'user-1', organizationalRoleId: 'role-1', scopeType: 1, validFrom: '2025-01-01' }] } });
      }
      if (url === 'AdminUser/') return Promise.resolve({ data: { items: [{ id: 'user-1', displayName: 'Ana' }] } });
      return Promise.resolve({ data: { items: [{ id: 'role-1', name: 'Coordenador' }] } });
    });
    putMock.mockResolvedValueOnce({ data: {} });
    const user = userEvent.setup();
    render(<MemoryRouter><OrganizationalRoleAssignmentOverview /></MemoryRouter>);

    await screen.findByText('Ana');
    await user.click(screen.getByTitle('Encerrar atribuição'));
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => expect(putMock).toHaveBeenCalledWith('AdminOrganizationalRoleAssignment/assignment-1/end'));
    expect(await screen.findByText('Atribuição encerrada com sucesso!')).toBeInTheDocument();
  });

  it('resends activation for a provisioned local account', async () => {
    getMock.mockResolvedValue({
      data: { items: [{ id: 'user-1', displayName: 'Ana', email: 'ana@example.com', accountState: 1, identityOrigin: 1 }] },
    });
    postMock.mockResolvedValueOnce({ data: { emailSent: true } });
    const user = userEvent.setup();
    render(<MemoryRouter><AdminUserOverview /></MemoryRouter>);

    await screen.findByText('ana@example.com');
    await user.click(screen.getByTitle('Reenviar ativação'));

    await waitFor(() => expect(postMock).toHaveBeenCalledWith('AdminUser/user-1/resend-activation'));
    expect(await screen.findByText('Um novo link de ativação foi enviado por e-mail. O link anterior foi invalidado.')).toBeInTheDocument();
  });

  it('edits a listed position through its public form', async () => {
    getMock.mockResolvedValue({ data: { items: [{ id: 1, name: 'Enfermeiro' }], page: 1, pageSize: 20, totalCount: 1 } });
    putMock.mockResolvedValueOnce({ data: { id: 1, name: 'Enfermeira' } });
    const user = userEvent.setup();
    render(<MemoryRouter><PositionOverview /></MemoryRouter>);

    await screen.findByText('Enfermeiro');
    await user.click(screen.getByTitle('Editar'));
    await user.clear(document.querySelector('input[name="name"]')!);
    await user.type(document.querySelector('input[name="name"]')!, 'Enfermeira');
    await user.click(screen.getByRole('button', { name: 'Salvar' }));

    await waitFor(() => expect(putMock).toHaveBeenCalled());
    expect(await screen.findByText('Cargo "Enfermeira" atualizado com sucesso!')).toBeInTheDocument();
  });

  it('deletes a listed position only after confirmation', async () => {
    getMock.mockResolvedValue({ data: { items: [{ id: 1, name: 'Enfermeiro' }], page: 1, pageSize: 20, totalCount: 1 } });
    deleteMock.mockResolvedValueOnce({ data: {} });
    const user = userEvent.setup();
    render(<MemoryRouter><PositionOverview /></MemoryRouter>);

    await screen.findByText('Enfermeiro');
    await user.click(screen.getByTitle('Excluir'));
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => expect(deleteMock).toHaveBeenCalled());
    expect(await screen.findByText('Cargo "Enfermeiro" excluído com sucesso!')).toBeInTheDocument();
  });

  it.each([
    ['plano de saúde', HealthInsurancePlanOverview, { id: 1, name: 'Saúde Municipal', abbreviation: 'SMS', type: 1 }],
    ['papel organizacional', OrganizationalRoleOverview, { id: 'role-1', name: 'Coordenador' }],
  ] as const)('deletes a listed %s only after confirmation', async (_, Page, model) => {
    getMock.mockResolvedValue({ data: { items: [model], page: 1, pageSize: 20, totalCount: 1 } });
    deleteMock.mockResolvedValueOnce({ data: {} });
    const user = userEvent.setup();
    render(<MemoryRouter><Page /></MemoryRouter>);

    await screen.findByText(model.name);
    await user.click(screen.getByTitle('Excluir'));
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => expect(deleteMock).toHaveBeenCalled());
  });

  it('creates an access policy from a controlled permission vocabulary', async () => {
    getMock.mockImplementation((url: string) =>
      Promise.resolve({
        data: {
          items: url === 'AdminPermission/?pageSize=100'
            ? [{ id: 'permission-1', resource: 'Resident', action: 'Read', feature: 'MedicalRecord' }]
            : [],
        },
      })
    );
    postMock.mockResolvedValueOnce({ data: {} });
    const user = userEvent.setup();
    render(<MemoryRouter><AccessPolicyOverview /></MemoryRouter>);

    await user.click(screen.getByRole('button', { name: 'Criar política' }));
    await screen.findByRole('option', { name: 'Resident.Read.MedicalRecord' });
    await user.selectOptions(document.querySelector('select[name="permissionId"]')!, 'permission-1');
    await user.selectOptions(document.querySelector('select[name="scopeType"]')!, '2');
    await user.type(screen.getByLabelText(/Identificador do escopo/), 'unit-1');
    await user.click(screen.getByRole('button', { name: 'Salvar' }));

    await waitFor(() => expect(postMock).toHaveBeenCalledWith(
      'AdminAccessPolicy/',
      expect.objectContaining({ resource: 'Resident', action: 'Read', feature: 'MedicalRecord', scopeType: 2, scopeKey: 'unit-1' })
    ));
  });

  it('changes an administrative account state after password confirmation', async () => {
    getMock.mockResolvedValue({
      data: { items: [{ id: 'user-1', displayName: 'Ana', email: 'ana@example.com', accountState: 2, identityOrigin: 1 }] },
    });
    putMock.mockResolvedValueOnce({ data: {} });
    const user = userEvent.setup();
    render(<MemoryRouter><AdminUserOverview /></MemoryRouter>);

    await screen.findByText('ana@example.com');
    await user.click(screen.getByTitle('Alterar estado'));
    await user.selectOptions(document.querySelector('select[name="accountState"]')!, '4');
    await user.type(screen.getByLabelText(/Sua senha atual/), 'SenhaSegura1!');
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => expect(putMock).toHaveBeenCalledWith(
      'AdminUser/user-1/state',
      { accountState: 4, currentPassword: 'SenhaSegura1!' }
    ));
    expect(await screen.findByText('Estado da conta atualizado com sucesso!')).toBeInTheDocument();
  });

  it('creates a provisioned administrative account without collecting a password', async () => {
    getMock.mockResolvedValue({ data: { items: [] } });
    postMock.mockResolvedValueOnce({
      data: { id: 'user-1', displayName: 'Ana', email: 'ana@example.com', emailSent: true },
    });
    const user = userEvent.setup();
    render(<MemoryRouter><AdminUserOverview /></MemoryRouter>);

    await user.click(screen.getByRole('button', { name: 'Adicionar' }));
    await user.type(screen.getByLabelText(/Nome/), 'Ana');
    await user.type(screen.getByLabelText(/E-mail/), 'ana@example.com');
    await user.click(screen.getByRole('button', { name: 'Salvar' }));

    await waitFor(() => expect(postMock).toHaveBeenCalledWith(
      'AdminUser/',
      { displayName: 'Ana', email: 'ana@example.com' }
    ));
    expect(await screen.findByText('Usuário "Ana" criado. O link de ativação foi enviado por e-mail.')).toBeInTheDocument();
  });

  it('deletes a listed religion only after confirmation', async () => {
    getMock.mockResolvedValue({ data: { items: [{ id: 1, name: 'Católica' }], page: 1, pageSize: 20, totalCount: 1 } });
    deleteMock.mockResolvedValueOnce({ data: {} });
    const user = userEvent.setup();
    render(<MemoryRouter><ReligionOverview /></MemoryRouter>);

    await screen.findByText('Católica');
    await user.click(screen.getByTitle('Excluir'));
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    await waitFor(() => expect(deleteMock).toHaveBeenCalled());
  });

  it('edits a listed health plan through its public form', async () => {
    getMock.mockResolvedValue({
      data: { items: [{ id: 1, name: 'Saúde Municipal', abbreviation: 'SMS', type: 1 }], page: 1, pageSize: 20, totalCount: 1 },
    });
    putMock.mockResolvedValueOnce({ data: { id: 1, name: 'Saúde Estadual', abbreviation: 'SMS', type: 1 } });
    const user = userEvent.setup();
    render(<MemoryRouter><HealthInsurancePlanOverview /></MemoryRouter>);
    await screen.findByText('Saúde Municipal');
    await user.click(screen.getByTitle('Editar'));
    await user.clear(document.querySelector('input[name="name"]')!);
    await user.type(document.querySelector('input[name="name"]')!, 'Saúde Estadual');
    await user.click(screen.getByRole('button', { name: 'Salvar' }));
    await waitFor(() => expect(putMock).toHaveBeenCalled());
  });
});
