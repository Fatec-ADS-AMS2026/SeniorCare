import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { axe } from 'jest-axe';
import CatalogPage from './CatalogPage';
import OperationalState from '@/types/models/OperationalState';

const getMock = vi.fn();

vi.mock('@/features/api', () => ({
  api: {
    get: (...args: unknown[]) => getMock(...args),
    post: vi.fn(),
    put: vi.fn(),
  },
  registerUnauthorizedHandler: vi.fn(),
}));

describe('CatalogPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('shows a loading state before the response resolves', () => {
    getMock.mockReturnValue(new Promise(() => {}));

    render(<CatalogPage />);

    expect(screen.getByRole('status')).toHaveTextContent('Carregando catálogo...');
  });

  it('shows an empty-state message when there are no modules', async () => {
    getMock.mockResolvedValueOnce({ data: [] });

    render(<CatalogPage />);

    await waitFor(() => {
      expect(screen.getByText('Nenhum módulo disponível para sua conta no momento.')).toBeInTheDocument();
    });
  });

  it('renders AVAILABLE modules as links ordered by Order, MAINTENANCE/UNAVAILABLE as non-interactive cards with their message', async () => {
    getMock.mockResolvedValueOnce({
      data: [
        {
          key: 'stock',
          name: 'Estoque',
          description: 'Controle de insumos',
          icon: 'Package',
          path: '/stock',
          order: 2,
          operationalState: OperationalState.MAINTENANCE,
          operationalMessage: 'Em manutenção programada.',
        },
        {
          key: 'care',
          name: 'Assistência',
          description: 'Cuidado dos residentes',
          icon: 'HeartStraight',
          path: '/care',
          order: 1,
          operationalState: OperationalState.AVAILABLE,
        },
      ],
    });

    render(<CatalogPage />);

    await waitFor(() => {
      expect(screen.getByText('Assistência')).toBeInTheDocument();
    });

    const careLink = screen.getByRole('link', { name: /Assistência/ });
    expect(careLink).toHaveAttribute('href', '/care');

    // MAINTENANCE não é um link (não abre navegação normal, §5.2).
    expect(screen.queryByRole('link', { name: /Estoque/ })).not.toBeInTheDocument();
    expect(screen.getByText('Em manutenção programada.')).toBeInTheDocument();
    expect(screen.getByText('Em manutenção')).toBeInTheDocument();

    // Ordenação segue Order (care=1 antes de stock=2), não a ordem da resposta.
    const links = screen.getAllByRole('link');
    const cardTitles = screen.getAllByRole('heading', { level: 2 }).map((h) => h.textContent);
    expect(cardTitles).toEqual(['Assistência', 'Estoque']);
    expect(links).toHaveLength(1);
  });

  it('never renders a DISABLED module even if the backend sends one (defense in depth)', async () => {
    getMock.mockResolvedValueOnce({
      data: [
        {
          key: 'care',
          name: 'Assistência',
          description: 'x',
          icon: 'HeartStraight',
          path: '/care',
          order: 1,
          operationalState: OperationalState.DISABLED,
        },
      ],
    });

    render(<CatalogPage />);

    await waitFor(() => {
      expect(screen.getByText('Nenhum módulo disponível para sua conta no momento.')).toBeInTheDocument();
    });
    expect(screen.queryByText('Assistência')).not.toBeInTheDocument();
  });

  it('shows a recoverable-failure state with correlationId and lets the user retry', async () => {
    const user = userEvent.setup();
    getMock.mockRejectedValueOnce({
      isAxiosError: true,
      response: {
        status: 500,
        data: { detail: 'Erro interno.', correlationId: 'corr-123' },
      },
    });
    getMock.mockResolvedValueOnce({ data: [] });

    render(<CatalogPage />);

    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeInTheDocument();
    });
    expect(screen.getByText('Erro interno.')).toBeInTheDocument();
    expect(screen.getByText(/corr-123/)).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Tentar novamente' }));

    await waitFor(() => {
      expect(screen.getByText('Nenhum módulo disponível para sua conta no momento.')).toBeInTheDocument();
    });
    expect(getMock).toHaveBeenCalledTimes(2);
  });

  // §8.6 — regressão de XSS armazenado: mesmo que um payload malicioso escape do
  // validador de entrada do backend (OperationalMessageSanitizer, defesa primária) e
  // chegue até aqui, o JSX de ModuleCard interpola operationalMessage como texto — nunca
  // como HTML — então o navegador nunca cria o elemento nem executa o script.
  it('renders a malicious operationalMessage as inert text, never as executable markup', async () => {
    const payload = '<script>window.__xss__ = true;</script><img src=x onerror="window.__xss__ = true">';
    getMock.mockResolvedValueOnce({
      data: [
        {
          key: 'stock',
          name: 'Estoque',
          description: 'Controle de insumos',
          icon: 'Package',
          path: '/stock',
          order: 1,
          operationalState: OperationalState.UNAVAILABLE,
          operationalMessage: payload,
        },
      ],
    });

    const { container } = render(<CatalogPage />);

    await waitFor(() => {
      expect(screen.getByText(payload)).toBeInTheDocument();
    });
    expect(container.querySelector('script')).toBeNull();
    expect(container.querySelector('img')).toBeNull();
    expect((window as unknown as { __xss__?: boolean }).__xss__).toBeUndefined();
  });

  it('has no detectable accessibility violations in the ready state', async () => {
    getMock.mockResolvedValueOnce({
      data: [
        {
          key: 'care',
          name: 'Assistência',
          description: 'Cuidado dos residentes',
          icon: 'HeartStraight',
          path: '/care',
          order: 1,
          operationalState: OperationalState.AVAILABLE,
        },
        {
          key: 'stock',
          name: 'Estoque',
          description: 'Controle de insumos',
          icon: 'Package',
          path: '/stock',
          order: 2,
          operationalState: OperationalState.UNAVAILABLE,
          operationalMessage: 'Indisponível no momento.',
        },
      ],
    });

    const { container } = render(<CatalogPage />);
    await waitFor(() => {
      expect(screen.getByText('Assistência')).toBeInTheDocument();
    });

    expect(await axe(container)).toHaveNoViolations();
  });
});
