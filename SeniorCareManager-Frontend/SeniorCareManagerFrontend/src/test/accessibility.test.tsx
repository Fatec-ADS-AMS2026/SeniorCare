import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { axe } from './axe';
import LoginForm from '@/features/auth/components/LoginForm';
import MfaChallengePage from '@/features/auth/pages/MfaChallengePage';
import RoleOverview from '@/features/role/pages/RoleOverview';
import { AuthProvider } from '@/contexts/AuthContext';

// §11: verificação automatizada dos 4 padrões de UI citados no design.md
// (Button, FormControls, Table, Modal) nos pontos de referência da baseline de
// acessibilidade — login, um formulário avulso, uma listagem em Table e um
// modal de formulário. A regra color-contrast fica desligada (ver src/test/axe.ts);
// contraste é conferido à mão e documentado em tasks.md.

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

describe('acessibilidade (jest-axe)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('login (LoginForm) não tem violações', async () => {
    getMock.mockRejectedValue({
      isAxiosError: true,
      response: { status: 401, data: {} },
    });

    const { container } = render(
      <MemoryRouter initialEntries={['/login']}>
        <AuthProvider>
          <Routes>
            <Route path='/login' element={<LoginForm />} />
          </Routes>
        </AuthProvider>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Entrar' })).toBeInTheDocument();
    });

    expect(await axe(container)).toHaveNoViolations();
  });

  it('formulário avulso (MfaChallengePage) não tem violações', async () => {
    getMock.mockRejectedValue({
      isAxiosError: true,
      response: { status: 401, data: {} },
    });

    const { container } = render(
      <MemoryRouter
        initialEntries={[
          { pathname: '/login/mfa', state: { challengeToken: 'challenge-abc' } },
        ]}
      >
        <AuthProvider>
          <Routes>
            <Route path='/login/mfa' element={<MfaChallengePage />} />
          </Routes>
        </AuthProvider>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Confirmar' })).toBeInTheDocument();
    });

    expect(await axe(container)).toHaveNoViolations();
  });

  it('listagem em Table (RoleOverview) não tem violações', async () => {
    getMock.mockResolvedValue({
      data: {
        items: [
          {
            id: '11111111-1111-1111-1111-111111111111',
            institutionId: '22222222-2222-2222-2222-222222222222',
            name: 'Cuidador',
            rowVersion: 3,
          },
        ],
        page: 1,
        pageSize: 20,
        totalCount: 1,
      },
    });

    const { container } = render(
      <MemoryRouter>
        <RoleOverview />
      </MemoryRouter>
    );

    await waitFor(() => expect(screen.getByText('Cuidador')).toBeInTheDocument());

    expect(await axe(container)).toHaveNoViolations();
  });

  it('modal de formulário (RoleFormModal aberto) não tem violações', async () => {
    const user = userEvent.setup();
    getMock.mockResolvedValue({
      data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
    });

    const { container } = render(
      <MemoryRouter>
        <RoleOverview />
      </MemoryRouter>
    );

    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Adicionar' })).toBeInTheDocument()
    );
    await user.click(screen.getByRole('button', { name: 'Adicionar' }));

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    expect(await axe(container)).toHaveNoViolations();
  });
});
