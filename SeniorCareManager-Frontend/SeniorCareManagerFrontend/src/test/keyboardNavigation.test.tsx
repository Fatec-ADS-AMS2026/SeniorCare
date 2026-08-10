import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import ReligionOverview from '@/features/religion/pages/ReligionOverview';
import LoginForm from '@/features/auth/components/LoginForm';
import { AuthProvider } from '@/contexts/AuthContext';

// §11.6: substitui a sessão manual ao vivo (backend/DB indisponíveis nesta
// sessão de trabalho) por uma passagem mecanizada com userEvent.tab()/keyboard(),
// que dirige foco e eventos de teclado reais do DOM (jsdom) — cobre a mesma
// jornada de referência exigida pelo cenário "Verificação manual de teclado" da
// spec (login + um CRUD de referência, aqui Religião) e funciona como proteção
// de regressão permanente pro trap de foco do BaseModal (§11.4).

const getMock = vi.fn();

vi.mock('@/features/api', () => ({
  api: {
    get: (...args: unknown[]) => getMock(...args),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
    patch: vi.fn(),
  },
  registerUnauthorizedHandler: vi.fn(),
}));

const getAllMock = vi.fn();
const createMock = vi.fn();

vi.mock('../features/religion/services/religionService', () => ({
  default: {
    getAll: (...args: unknown[]) => getAllMock(...args),
    getById: vi.fn(),
    create: (...args: unknown[]) => createMock(...args),
    update: vi.fn(),
    deleteById: vi.fn(),
  },
}));

describe('navegação por teclado — login', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    getMock.mockRejectedValue({
      isAxiosError: true,
      response: { status: 401, data: {} },
    });
  });

  it('alcança todos os controles via Tab, na ordem, e o botão de mostrar/ocultar senha ativa por teclado', async () => {
    const user = userEvent.setup();
    render(
      <MemoryRouter initialEntries={['/login']}>
        <AuthProvider>
          <LoginForm />
        </AuthProvider>
      </MemoryRouter>
    );

    const emailInput = screen.getByPlaceholderText('Digite seu email');
    const passwordInput = screen.getByPlaceholderText('Digite sua senha');
    const toggleButton = screen.getByRole('button', { name: 'Mostrar senha' });
    const forgotButton = screen.getByRole('button', {
      name: 'Esqueceu sua senha?',
    });
    const submitButton = screen.getByRole('button', { name: 'Entrar' });

    await user.tab();
    expect(document.activeElement).toBe(emailInput);

    await user.tab();
    expect(document.activeElement).toBe(passwordInput);

    await user.tab();
    expect(document.activeElement).toBe(toggleButton);

    // Ativação por teclado (Espaço) do botão só-ícone — não só clique.
    expect(passwordInput).toHaveAttribute('type', 'password');
    await user.keyboard(' ');
    expect(passwordInput).toHaveAttribute('type', 'text');
    expect(
      screen.getByRole('button', { name: 'Ocultar senha' })
    ).toBe(toggleButton);

    await user.tab();
    expect(document.activeElement).toBe(forgotButton);

    await user.tab();
    expect(document.activeElement).toBe(submitButton);
  });
});

describe('navegação por teclado — CRUD de referência (Religião)', () => {
  const existingReligion = { id: 1, name: 'Catolicismo' };

  beforeEach(() => {
    vi.clearAllMocks();
    getAllMock.mockResolvedValue({
      success: true,
      message: '',
      data: [existingReligion],
    });
  });

  function renderReligionOverview() {
    return render(
      <MemoryRouter>
        <ReligionOverview />
      </MemoryRouter>
    );
  }

  it('abre o modal de cadastro por teclado, percorre a ordem de Tab (trap incluído) e Cancelar devolve o foco', async () => {
    const user = userEvent.setup();
    renderReligionOverview();

    await waitFor(() =>
      expect(screen.getByText('Catolicismo')).toBeInTheDocument()
    );

    const addButton = screen.getByRole('button', { name: 'Adicionar' });
    addButton.focus();
    await user.keyboard('{Enter}');

    await screen.findByRole('dialog');

    // §11.4: foco entra automaticamente no modal ao abrir (primeiro elemento
    // focável do container — o botão "Fechar" do cabeçalho, que vem antes do
    // campo do formulário na ordem do DOM).
    const closeButton = screen.getByRole('button', { name: 'Fechar' });
    expect(document.activeElement).toBe(closeButton);

    await user.tab();
    const nameInput = document.querySelector(
      'input[name="name"]'
    ) as HTMLInputElement;
    expect(document.activeElement).toBe(nameInput);
    await user.keyboard('Budismo');

    await user.tab();
    const cancelButton = screen.getByRole('button', { name: 'Cancelar' });
    expect(document.activeElement).toBe(cancelButton);

    await user.tab();
    const saveButton = screen.getByRole('button', { name: 'Salvar' });
    expect(document.activeElement).toBe(saveButton);

    // Tab a partir do último elemento focável prende de volta no primeiro —
    // não escapa pro conteúdo de fundo enquanto o modal está aberto.
    await user.tab();
    expect(document.activeElement).toBe(closeButton);

    cancelButton.focus();
    await user.keyboard('{Enter}');

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
    expect(document.activeElement).toBe(addButton);
  });

  it('salva com dados digitados via teclado (Enter no botão Salvar submete o formulário)', async () => {
    const user = userEvent.setup();
    createMock.mockResolvedValueOnce({
      success: true,
      message: '',
      data: { id: 2, name: 'Budismo' },
    });

    renderReligionOverview();

    await waitFor(() =>
      expect(screen.getByText('Catolicismo')).toBeInTheDocument()
    );

    await user.click(screen.getByRole('button', { name: 'Adicionar' }));
    await screen.findByRole('dialog');

    const nameInput = document.querySelector(
      'input[name="name"]'
    ) as HTMLInputElement;
    nameInput.focus();
    await user.keyboard('Budismo');

    screen.getByRole('button', { name: 'Salvar' }).focus();
    await user.keyboard('{Enter}');

    await waitFor(() => {
      expect(createMock).toHaveBeenCalledWith(
        expect.objectContaining({ name: 'Budismo' })
      );
    });
    // O sucesso reabre a jornada num AlertModal (mesmo componente Modal,
    // achado registrado em §11.7: sem controle de OK visível, só Esc/backdrop
    // fecham — mantido fora do escopo mecânico desta seção).
    await waitFor(() => {
      expect(
        screen.getByText('Religião "Budismo" criada com sucesso!')
      ).toBeInTheDocument();
    });
    await user.keyboard('{Escape}');
    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });

  it('Esc fecha o modal e devolve o foco pro botão que abriu', async () => {
    const user = userEvent.setup();
    renderReligionOverview();

    await waitFor(() =>
      expect(screen.getByText('Catolicismo')).toBeInTheDocument()
    );

    const addButton = screen.getByRole('button', { name: 'Adicionar' });
    await user.click(addButton);
    await screen.findByRole('dialog');

    await user.keyboard('{Escape}');

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
    expect(document.activeElement).toBe(addButton);
  });

  it('os botões de ação Editar/Excluir da linha são alcançáveis e ativáveis por teclado', async () => {
    const user = userEvent.setup();
    renderReligionOverview();

    await waitFor(() =>
      expect(screen.getByText('Catolicismo')).toBeInTheDocument()
    );

    const editButton = screen.getByRole('button', { name: 'Editar' });
    editButton.focus();
    await user.keyboard('{Enter}');

    const editDialog = await screen.findByRole('dialog');
    expect(screen.getByText('Editar Religião')).toBeInTheDocument();
    await user.keyboard('{Escape}');
    await waitFor(() => expect(editDialog).not.toBeInTheDocument());

    const deleteButton = screen.getByRole('button', { name: 'Excluir' });
    deleteButton.focus();
    await user.keyboard('{Enter}');

    await screen.findByRole('dialog');
    expect(
      screen.getByText('Deseja realmente excluir essa Religião?')
    ).toBeInTheDocument();
  });
});
