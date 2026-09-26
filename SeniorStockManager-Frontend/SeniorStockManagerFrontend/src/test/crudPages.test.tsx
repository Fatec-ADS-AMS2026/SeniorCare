import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import UnitOfMeasureOverview from '@/features/unitOfMeasure/pages/UnitOfMeasureOverview';
import ProductGroupOverview from '@/features/productGroup/pages/ProductGroupOverview';
import ProductTypeOverview from '@/features/productType/pages/ProductTypeOverview';
import ProductOverview from '@/features/product/pages/ProductOverview';
import SupplierOverview from '@/features/supplier/pages/SupplierOverview';
import ManufacturerOverview from '@/features/manufacturer/pages/ManufacturerOverview';

const getMock = vi.fn();
const postMock = vi.fn();
const deleteMock = vi.fn();
const putMock = vi.fn();

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
  ['unidades de medida', UnitOfMeasureOverview],
  ['grupos de produto', ProductGroupOverview],
  ['tipos de produto', ProductTypeOverview],
  ['produtos', ProductOverview],
  ['fornecedores', SupplierOverview],
] as const;

describe('páginas CRUD de estoque', () => {
  beforeEach(() => {
    getMock.mockResolvedValue({
      data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
    });
  });

  it.each(pages)('renders the empty %s catalogue and exposes its create action', async (_, Page) => {
    render(
      <MemoryRouter>
        <Page />
      </MemoryRouter>
    );

    await waitFor(() => expect(getMock).toHaveBeenCalled());
    expect(screen.getByRole('button', { name: 'Adicionar' })).toBeEnabled();
  });

  it.each([
    ['unidade de medida', UnitOfMeasureOverview, ['description', 'abbreviation'], ['Caixa', 'CX']],
    ['grupo de produto', ProductGroupOverview, ['name'], ['Medicamentos']],
    ['tipo de produto', ProductTypeOverview, ['name'], ['Analgésicos']],
  ] as const)('creates a %s through its public form', async (_, Page, fields, values) => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({ data: { id: 1 } });
    render(<MemoryRouter><Page /></MemoryRouter>);
    await waitFor(() => expect(getMock).toHaveBeenCalled());
    await user.click(screen.getByRole('button', { name: 'Adicionar' }));
    for (const [field, value] of fields.map((field, index) => [field, values[index]] as const)) {
      await user.type(document.querySelector(`input[name="${field}"]`)! , value);
    }
    await user.click(screen.getByRole('button', { name: 'Salvar' }));
    await waitFor(() => expect(postMock).toHaveBeenCalled());
  });

  it('blocks an overlong unit description before sending it to the API', async () => {
    const user = userEvent.setup();
    render(<MemoryRouter><UnitOfMeasureOverview /></MemoryRouter>);

    await waitFor(() => expect(getMock).toHaveBeenCalled());
    await user.click(screen.getByRole('button', { name: 'Adicionar' }));
    await user.type(document.querySelector('input[name="description"]')!, 'A'.repeat(51));
    await user.type(document.querySelector('input[name="abbreviation"]')!, 'CX');
    postMock.mockClear();
    await user.click(screen.getByRole('button', { name: 'Salvar' }));

    expect(await screen.findByText('Campo descrição deve conter menos de 50 caracteres')).toBeInTheDocument();
    expect(postMock).not.toHaveBeenCalled();
  });

  it('filters listed units by description', async () => {
    getMock.mockResolvedValue({
      data: {
        items: [
          { id: 1, description: 'Caixa', abbreviation: 'CX' },
          { id: 2, description: 'Frasco', abbreviation: 'FR' },
        ],
        page: 1,
        pageSize: 20,
        totalCount: 2,
      },
    });
    const user = userEvent.setup();
    render(<MemoryRouter><UnitOfMeasureOverview /></MemoryRouter>);

    await screen.findByText('Caixa');
    await user.type(screen.getByPlaceholderText('Buscar Unidade de Medida...'), 'Caixa');
    await user.click(screen.getByRole('button', { name: 'Buscar' }));

    expect(screen.getByText('Caixa')).toBeInTheDocument();
    expect(screen.queryByText('Frasco')).not.toBeInTheDocument();
  });

  it.each([
    ['unidade de medida', UnitOfMeasureOverview, { id: 1, description: 'Caixa', abbreviation: 'CX' }],
    ['grupo de produto', ProductGroupOverview, { id: 1, name: 'Medicamentos' }],
    ['tipo de produto', ProductTypeOverview, { id: 1, name: 'Analgésicos' }],
    ['produto', ProductOverview, { id: 1, description: 'Dipirona' }],
    ['fornecedor', SupplierOverview, { id: 1, tradeName: 'Fornecedor Teste' }],
    ['fabricante', ManufacturerOverview, { id: 1, corporateName: 'Fabricante Teste', tradeName: 'Teste', cpfCnpj: '123' }],
  ] as const)('deletes a listed %s only after confirmation', async (_, Page, model) => {
    getMock.mockResolvedValue({ data: { items: [model], page: 1, pageSize: 20, totalCount: 1 } });
    deleteMock.mockResolvedValueOnce({ data: {} });
    const user = userEvent.setup();
    render(<MemoryRouter><Page /></MemoryRouter>);

    await screen.findByText(Object.values(model)[1] as string);
    await user.click(screen.getByTitle('Excluir'));
    await user.click(screen.getByRole('button', { name: 'Confirmar' }));
    await waitFor(() => expect(deleteMock).toHaveBeenCalled());
  });

  it('creates a manufacturer through its public form', async () => {
    postMock.mockResolvedValueOnce({ data: { id: 1, corporateName: 'Fabricante Ltda' } });
    const user = userEvent.setup();
    render(<MemoryRouter><ManufacturerOverview /></MemoryRouter>);
    await waitFor(() => expect(getMock).toHaveBeenCalled());
    await user.click(screen.getByRole('button', { name: 'Adicionar' }));
    for (const [name, value] of Object.entries({
      corporateName: 'Fabricante Ltda', tradeName: 'Fabricante', cpfCnpj: '12345678000100', phone: '11999999999', email: 'contato@example.com',
    })) await user.type(document.querySelector(`input[name="${name}"]`)! as HTMLInputElement, value);
    await user.click(screen.getByRole('button', { name: 'Salvar' }));
    await waitFor(() => expect(postMock).toHaveBeenCalled());
  });

  it('edits a listed manufacturer through its public form', async () => {
    getMock.mockResolvedValue({
      data: { items: [{ id: 1, corporateName: 'Fabricante Ltda', tradeName: 'Fabricante', cpfCnpj: '123', phone: '11999999999', email: 'contato@example.com' }] },
    });
    putMock.mockResolvedValueOnce({ data: { corporateName: 'Fabricante Atualizado' } });
    const user = userEvent.setup();
    render(<MemoryRouter><ManufacturerOverview /></MemoryRouter>);
    await screen.findByText('Fabricante Ltda');
    await user.click(screen.getByTitle('Editar'));
    await user.clear(document.querySelector('input[name="corporateName"]')!);
    await user.type(document.querySelector('input[name="corporateName"]')!, 'Fabricante Atualizado');
    await user.click(screen.getByRole('button', { name: 'Salvar' }));
    await waitFor(() => expect(putMock).toHaveBeenCalled());
  });

  it('edits a listed product group through its public form', async () => {
    getMock.mockResolvedValue({ data: { items: [{ id: 1, name: 'Medicamentos' }] } });
    putMock.mockResolvedValueOnce({ data: { id: 1, name: 'Medicamentos Gerais' } });
    const user = userEvent.setup();
    render(<MemoryRouter><ProductGroupOverview /></MemoryRouter>);
    await screen.findByText('Medicamentos');
    await user.click(screen.getByTitle('Editar'));
    await user.clear(document.querySelector('input[name="name"]')!);
    await user.type(document.querySelector('input[name="name"]')!, 'Medicamentos Gerais');
    await user.click(screen.getByRole('button', { name: 'Salvar' }));
    await waitFor(() => expect(putMock).toHaveBeenCalled());
  });
});
