import { describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import CarrierForm from './CarrierForm';

const { create } = vi.hoisted(() => ({ create: vi.fn() }));
vi.mock('../services/carrierService', () => ({ default: { create, getById: vi.fn(), update: vi.fn() } }));

describe('CarrierForm', () => {
  it('creates a carrier from the registration form', async () => {
    create.mockResolvedValueOnce({ success: true, data: {} });
    vi.stubGlobal('alert', vi.fn());
    const user = userEvent.setup();
    render(<MemoryRouter initialEntries={['/carrier/0']}><Routes><Route path='/carrier/:id' element={<CarrierForm />} /></Routes></MemoryRouter>);

    for (const [name, value] of Object.entries({
      corporateName: 'Transportadora Ltda', tradeName: 'Transportes', cpfCnpj: '12345678000100', street: 'Rua Teste', number: '1',
      addressComplement: 'Sala 1', district: 'Centro', postalCode: '01001000', city: 'São Paulo', state: 'SP', phone: '11999999999', email: 'contato@example.com',
    })) await user.type(document.querySelector(`input[name="${name}"]`)! as HTMLInputElement, value);
    await user.click(screen.getByRole('button', { name: 'Cadastrar Transportadora' }));

    await waitFor(() => expect(create).toHaveBeenCalledWith(expect.objectContaining({ corporateName: 'Transportadora Ltda', city: 'São Paulo' })));
  });
});
