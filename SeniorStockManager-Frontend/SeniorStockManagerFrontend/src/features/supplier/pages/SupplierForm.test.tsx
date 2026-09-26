import { describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import SupplierForm from './SupplierForm';

const { create } = vi.hoisted(() => ({ create: vi.fn() }));
vi.mock('../services/supplierService', () => ({ default: { create, getById: vi.fn(), update: vi.fn() } }));

describe('SupplierForm', () => {
  it('creates a supplier from the public registration form', async () => {
    create.mockResolvedValueOnce({ success: true, data: {} });
    const user = userEvent.setup();
    render(<MemoryRouter initialEntries={['/supplier/0']}><Routes><Route path='/supplier/:id' element={<SupplierForm />} /></Routes></MemoryRouter>);

    for (const [name, value] of Object.entries({
      tradeName: 'Fornecedor Teste', corporateName: 'Fornecedor Teste Ltda', cpfCnpj: '12345678000100',
      phone: '11999999999', postalCode: '01001000', street: 'Rua Teste', number: '1', district: 'Centro', city: 'São Paulo', state: 'SP',
    })) await user.type(document.querySelector(`input[name="${name}"]`)! as HTMLInputElement, value);
    await user.click(screen.getByRole('button', { name: 'Cadastrar Fornecedor' }));

    await waitFor(() => expect(create).toHaveBeenCalledWith(expect.objectContaining({
      tradeName: 'Fornecedor Teste', corporateName: 'Fornecedor Teste Ltda', city: 'São Paulo', state: 'SP',
    })));
  });
});
