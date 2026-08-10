import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import UserPermissionOverrideOverview from './UserPermissionOverrideOverview';

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

const user1 = {
  id: 'u1',
  email: 'fulana@example.com',
  displayName: 'Fulana',
  accountState: 2,
  identityOrigin: 1,
};

const permission1 = {
  id: 'p1',
  resource: 'Carrier',
  action: 'write',
  description: '',
  isSystemOperation: false,
};

function renderOverview() {
  return render(
    <MemoryRouter>
      <UserPermissionOverrideOverview />
    </MemoryRouter>
  );
}

describe('UserPermissionOverrideOverview', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    getMock.mockImplementation((url: string) => {
      if (url.startsWith('AdminUserPermissionOverride')) {
        return Promise.resolve({
          data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
        });
      }
      if (url.startsWith('AdminUser')) {
        return Promise.resolve({
          data: { items: [user1], page: 1, pageSize: 20, totalCount: 1 },
        });
      }
      if (url.startsWith('AdminPermission')) {
        return Promise.resolve({
          data: { items: [permission1], page: 1, pageSize: 100, totalCount: 1 },
        });
      }
      return Promise.reject(new Error(`unexpected url ${url}`));
    });
  });

  it('creates a permission override for a chosen user and permission', async () => {
    const user = userEvent.setup();
    postMock.mockResolvedValueOnce({
      data: {
        id: 'o1',
        userId: user1.id,
        resource: permission1.resource,
        action: permission1.action,
        effect: 1,
        justification: 'Cobertura de plantão excepcional',
        grantedByUserId: 'admin1',
        validFrom: '2026-01-01',
        validTo: '2026-01-31',
      },
    });

    renderOverview();

    await user.click(screen.getByRole('button', { name: 'Criar exceção' }));

    await waitFor(() => {
      expect(
        screen.getByRole('option', { name: 'Fulana' })
      ).toBeInTheDocument();
    });

    const selects = document.querySelectorAll('select');
    // Usuário, Permissão, Efeito, Tipo de escopo — nessa ordem no formulário.
    await user.selectOptions(selects[0], user1.id);
    await user.selectOptions(selects[1], permission1.id);

    const validToInput = document.querySelector(
      'input[name="validTo"]'
    ) as HTMLInputElement;
    await user.type(validToInput, '2026-01-31');

    const justificationInput = document.querySelector(
      'input[name="justification"]'
    ) as HTMLInputElement;
    await user.type(justificationInput, 'Cobertura de plantão excepcional');

    await user.click(screen.getByRole('button', { name: 'Salvar' }));

    await waitFor(() => {
      expect(postMock).toHaveBeenCalledWith(
        'AdminUserPermissionOverride/',
        expect.objectContaining({
          userId: user1.id,
          resource: 'Carrier',
          action: 'write',
        })
      );
    });
  });
});
