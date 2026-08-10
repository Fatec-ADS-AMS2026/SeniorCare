import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { axe } from './axe';
import LoginForm from '@/features/auth/components/LoginForm';
import CarrierForm from '@/features/carrier/pages/CarrierForm';
import ManufacturerOverview from '@/features/manufacturer/pages/ManufacturerOverview';
import { AuthProvider } from '@/contexts/AuthContext';

// §11: verificação automatizada dos 4 padrões de UI citados no design.md
// (Button, FormControls, Table, Modal) nos pontos de referência da baseline de
// acessibilidade — login, um formulário avulso, uma listagem em Table e um
// modal de formulário. A regra color-contrast fica desligada (ver src/test/axe.ts);
// contraste é conferido à mão e documentado em tasks.md.

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

const getAllMock = vi.fn();
const createMock = vi.fn();
const updateMock = vi.fn();
const deleteByIdMock = vi.fn();

vi.mock('../features/carrier/services/carrierService', () => ({
  default: {
    getAll: (...args: unknown[]) => getAllMock(...args),
    getById: vi.fn(),
    create: (...args: unknown[]) => createMock(...args),
    update: (...args: unknown[]) => updateMock(...args),
    deleteById: (...args: unknown[]) => deleteByIdMock(...args),
  },
}));

vi.mock('../features/manufacturer/services/manufacturerService', () => ({
  default: {
    getAll: (...args: unknown[]) => getAllMock(...args),
    getById: vi.fn(),
    create: (...args: unknown[]) => createMock(...args),
    update: (...args: unknown[]) => updateMock(...args),
    deleteById: (...args: unknown[]) => deleteByIdMock(...args),
  },
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

  it('formulário avulso (CarrierForm) não tem violações', async () => {
    const { container } = render(
      <MemoryRouter initialEntries={['/transportadora/0']}>
        <Routes>
          <Route path='/transportadora/:id' element={<CarrierForm />} />
        </Routes>
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Cadastrar Transportadora' })
      ).toBeInTheDocument();
    });

    expect(await axe(container)).toHaveNoViolations();
  });

  it('listagem em Table (ManufacturerOverview) não tem violações', async () => {
    getAllMock.mockResolvedValue({
      success: true,
      message: '',
      data: [
        {
          id: 1,
          corporateName: 'Acme Fabricante Ltda',
          tradeName: 'Acme',
          cpfCnpj: '12345678000199',
          phone: '11999999999',
          email: 'contato@acme.example',
        },
      ],
    });

    const { container } = render(
      <MemoryRouter>
        <ManufacturerOverview />
      </MemoryRouter>
    );

    await waitFor(() =>
      expect(screen.getByText('Acme Fabricante Ltda')).toBeInTheDocument()
    );

    expect(await axe(container)).toHaveNoViolations();
  });

  it('modal de formulário (ManufacturerFormModal aberto) não tem violações', async () => {
    const user = userEvent.setup();
    getAllMock.mockResolvedValue({ success: true, message: '', data: [] });

    const { container } = render(
      <MemoryRouter>
        <ManufacturerOverview />
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
