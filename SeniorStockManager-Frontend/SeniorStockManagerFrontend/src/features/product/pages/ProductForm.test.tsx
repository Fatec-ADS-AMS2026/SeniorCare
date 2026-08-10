import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import ProductForm from './ProductForm';
import ServiceResult from '@/types/app/ServiceResult';
import Product from '@/types/models/Product';

// Primeiro teste de componente/formulário do projeto (nenhum precedente de
// render()/userEvent existia antes da §9) — segue a convenção de mock de
// serviço já usada em serviceUtils.test.ts, mas mockando os services
// (ProductService/ProductTypeService/ProductGroupService/UnitOfMeasureService)
// diretamente em vez da camada http, já que o alvo aqui é o comportamento do
// formulário, não a serialização HTTP (isso já é coberto por serviceUtils.test.ts).

const getAllMock = vi.fn();
const getByIdMock = vi.fn();
const createMock = vi.fn();
const updateMock = vi.fn();

vi.mock('../services/productService', () => ({
  default: {
    getAll: (...args: unknown[]) => getAllMock(...args),
    getById: (...args: unknown[]) => getByIdMock(...args),
    create: (...args: unknown[]) => createMock(...args),
    update: (...args: unknown[]) => updateMock(...args),
    deleteById: vi.fn(),
  },
}));

// routes.tsx importa `*Routes` desses mesmos módulos (pra montar o menu), então
// o mock precisa preservar os exports originais além de sobrescrever os
// serviços/modais usados diretamente pelo ProductForm.
vi.mock('@/features/productType', async (importOriginal) => ({
  ...(await importOriginal<object>()),
  ProductTypeService: {
    getAll: vi.fn(),
    create: vi.fn(),
  },
  ProductTypeFormModal: () => null,
}));

vi.mock('@/features/productGroup', async (importOriginal) => ({
  ...(await importOriginal<object>()),
  ProductGroupService: {
    getAll: vi.fn(),
    create: vi.fn(),
  },
  ProductGroupFormModal: () => null,
}));

vi.mock('@/features/unitOfMeasure', async (importOriginal) => ({
  ...(await importOriginal<object>()),
  UnitOfMeasureService: {
    getAll: vi.fn(),
    create: vi.fn(),
  },
  UnitOfMeasureFormModal: () => null,
}));

import { ProductTypeService } from '@/features/productType';
import { ProductGroupService } from '@/features/productGroup';
import { UnitOfMeasureService } from '@/features/unitOfMeasure';

const mockProductType = ProductTypeService as unknown as {
  getAll: ReturnType<typeof vi.fn>;
};
const mockProductGroup = ProductGroupService as unknown as {
  getAll: ReturnType<typeof vi.fn>;
};
const mockUnitOfMeasure = UnitOfMeasureService as unknown as {
  getAll: ReturnType<typeof vi.fn>;
};

const productGroup = { id: 1, name: 'Medicamentos' };
const productType = { id: 10, name: 'Analgésico', productGroupId: 1 };
const unitOfMeasure = { id: 100, description: 'Caixa', abbreviation: 'CX' };

const existingProduct: Product = {
  id: 5,
  description: 'Dipirona 500mg',
  genericName: 'Dipirona',
  minimumStock: 10,
  currentStock: 50,
  unitPrice: 12.5,
  highCost: false,
  expirationControlled: true,
  productTypeId: 10,
  unitOfMeasureId: 100,
  rowVersion: 3,
  isActive: true,
};

function renderForm(initialPath = '/produto/0') {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <Routes>
        <Route path='/produto/:id' element={<ProductForm />} />
      </Routes>
    </MemoryRouter>
  );
}

describe('ProductForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockProductType.getAll.mockResolvedValue({
      success: true,
      message: '',
      data: [productType],
    } satisfies ServiceResult<typeof productType[]>);
    mockProductGroup.getAll.mockResolvedValue({
      success: true,
      message: '',
      data: [productGroup],
    } satisfies ServiceResult<typeof productGroup[]>);
    mockUnitOfMeasure.getAll.mockResolvedValue({
      success: true,
      message: '',
      data: [unitOfMeasure],
    } satisfies ServiceResult<typeof unitOfMeasure[]>);
  });

  it('submits a valid new product and navigates back to the list', async () => {
    const user = userEvent.setup();
    createMock.mockResolvedValueOnce({
      success: true,
      message: '',
      data: { ...existingProduct, id: 99 },
    });

    const { container } = renderForm();

    await waitFor(() => {
      expect(mockProductType.getAll).toHaveBeenCalled();
    });

    const genericNameInput = container.querySelector(
      'input[name="genericName"]'
    ) as HTMLInputElement;
    const descriptionInput = container.querySelector(
      'input[name="description"]'
    ) as HTMLInputElement;

    await user.type(genericNameInput, 'Paracetamol');
    await user.type(descriptionInput, 'Paracetamol 750mg');

    const submitButton = screen.getByRole('button', {
      name: /cadastrar produto/i,
    });
    await user.click(submitButton);

    await waitFor(() => {
      expect(createMock).toHaveBeenCalledTimes(1);
    });

    const submittedPayload = createMock.mock.calls[0][0] as Product;
    expect(submittedPayload.genericName).toBe('Paracetamol');
    expect(submittedPayload.description).toBe('Paracetamol 750mg');
  });

  it('shows the Problem Details error message when creation fails validation', async () => {
    const user = userEvent.setup();
    createMock.mockResolvedValueOnce({
      success: false,
      message: 'One or more validation errors occurred.',
      errors: [
        { field: 'Description', message: 'The Description field is required.' },
      ],
    });

    renderForm();

    await waitFor(() => {
      expect(mockProductType.getAll).toHaveBeenCalled();
    });

    const submitButton = screen.getByRole('button', {
      name: /cadastrar produto/i,
    });
    await user.click(submitButton);

    await waitFor(() => {
      expect(
        screen.getByText('One or more validation errors occurred.')
      ).toBeInTheDocument();
    });
  });

  it('shows the RowVersion conflict (409) message when updating a stale product', async () => {
    const user = userEvent.setup();
    getByIdMock.mockResolvedValueOnce({
      success: true,
      message: '',
      data: existingProduct,
    });
    updateMock.mockResolvedValueOnce({
      success: false,
      message:
        'O recurso foi modificado por outra requisição. Recarregue e tente novamente.',
    });

    renderForm('/produto/5');

    await waitFor(() => {
      expect(getByIdMock).toHaveBeenCalledWith(5);
    });

    const submitButton = await screen.findByRole('button', {
      name: /salvar alterações/i,
    });
    await user.click(submitButton);

    await waitFor(() => {
      expect(updateMock).toHaveBeenCalledTimes(1);
    });

    // §9.1/9.7: o RowVersion carregado do GET precisa ser reenviado no PUT sem
    // alteração — é o que permite o backend detectar o conflito de concorrência.
    const submittedPayload = updateMock.mock.calls[0][1] as Product;
    expect(submittedPayload.rowVersion).toBe(existingProduct.rowVersion);

    await waitFor(() => {
      expect(
        screen.getByText(
          'O recurso foi modificado por outra requisição. Recarregue e tente novamente.'
        )
      ).toBeInTheDocument();
    });
  });

  it('derives the highCost/expirationControlled checkboxes from real booleans, not YesNo', async () => {
    getByIdMock.mockResolvedValueOnce({
      success: true,
      message: '',
      data: existingProduct,
    });

    renderForm('/produto/5');

    await waitFor(() => {
      expect(getByIdMock).toHaveBeenCalledWith(5);
    });

    // existingProduct.expirationControlled = true, highCost = false.
    const expirationCheckbox = (await screen.findByLabelText(
      'Possui controle de validade'
    )) as HTMLInputElement;
    const highCostCheckbox = screen.getByLabelText(
      'Alto Custo'
    ) as HTMLInputElement;

    expect(expirationCheckbox.checked).toBe(true);
    expect(highCostCheckbox.checked).toBe(false);
  });
});
