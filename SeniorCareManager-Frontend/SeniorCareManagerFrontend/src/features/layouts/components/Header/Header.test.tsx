import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import Header from './index';
import { AuthProvider } from '@/contexts/AuthContext';
import { ThemeContext } from '@/contexts/ThemeContext';

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

function renderHeader() {
  return render(
    <MemoryRouter initialEntries={['/admin']}>
      <ThemeContext.Provider
        value={{ theme: 'light', fontSize: 16, toggleTheme: vi.fn(), changeFontSize: vi.fn() }}
      >
        <AuthProvider>
          <Routes>
            <Route path='/admin' element={<Header />} />
            <Route path='/login' element={<div>Tela de login</div>} />
          </Routes>
        </AuthProvider>
      </ThemeContext.Provider>
    </MemoryRouter>
  );
}

describe('Header', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('shows no user menu while anonymous', async () => {
    getMock.mockRejectedValueOnce({
      isAxiosError: true,
      response: { status: 401, data: {} },
    });

    renderHeader();

    await waitFor(() => {
      expect(getMock).toHaveBeenCalled();
    });
    expect(screen.queryByText('Sair')).not.toBeInTheDocument();
  });

  it('shows the signed-in user and logs out on click', async () => {
    const user = userEvent.setup();
    getMock.mockResolvedValueOnce({
      data: {
        userId: '1',
        institutionId: '1',
        institutionName: 'ILPI Teste',
        displayName: 'Fulana de Tal',
        email: 'fulana@example.com',
        roles: [],
        organizationalResponsibilities: [],
        effectivePermissions: [],
      },
    });
    postMock.mockResolvedValueOnce({ data: { message: 'Sessão encerrada.' } });

    renderHeader();

    await waitFor(() => {
      expect(screen.getByText('Fulana de Tal')).toBeInTheDocument();
    });

    await user.click(screen.getByText('Sair'));

    await waitFor(() => {
      expect(screen.getByText('Tela de login')).toBeInTheDocument();
    });
    expect(postMock).toHaveBeenCalledWith('Auth/logout');
  });
});
