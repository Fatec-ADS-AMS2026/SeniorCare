import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { ThemeContext } from '@/contexts/ThemeContext';
import LandingPage from './index';

const originalLocation = window.location;

function mockLocationAssign() {
  const assign = vi.fn();
  Object.defineProperty(window, 'location', {
    value: { ...originalLocation, assign },
    writable: true,
    configurable: true,
  });
  return assign;
}

function restoreLocation() {
  Object.defineProperty(window, 'location', {
    value: originalLocation,
    writable: true,
    configurable: true,
  });
}

function renderLandingPage(Component: typeof LandingPage) {
  return render(
    <MemoryRouter>
      <ThemeContext.Provider
        value={{ theme: 'light', fontSize: 16, toggleTheme: vi.fn(), changeFontSize: vi.fn() }}
      >
        <Component />
      </ThemeContext.Provider>
    </MemoryRouter>
  );
}

describe('LandingPage', () => {
  afterEach(() => {
    restoreLocation();
  });

  it('renders the legacy marketing splash when path-based routing is not active (default deploy)', () => {
    renderLandingPage(LandingPage);

    expect(
      screen.getByText('Plataforma de ferramentas para gerenciar o cuidado e bem-estar de idosos.')
    ).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Fazer login' })).toBeInTheDocument();
  });

  // §9.2 — mesma técnica de vi.stubEnv + vi.resetModules + reimportação
  // dinâmica de LoginForm.test.tsx: isPathBasedRouting é constante de
  // módulo, fixada no primeiro import.
  describe('quando VITE_BASE_PATH está ativo (roteamento por caminho)', () => {
    beforeEach(() => {
      vi.stubEnv('VITE_BASE_PATH', '/care/');
      vi.resetModules();
    });

    afterEach(() => {
      vi.unstubAllEnvs();
    });

    it('redireciona pra raiz do Senior Portal em vez de mostrar a landing legada', async () => {
      const assignSpy = mockLocationAssign();
      const { default: FreshLandingPage } = await import('./index');

      renderLandingPage(FreshLandingPage);

      await waitFor(() => {
        expect(assignSpy).toHaveBeenCalledWith('/');
      });
      expect(
        screen.queryByText('Plataforma de ferramentas para gerenciar o cuidado e bem-estar de idosos.')
      ).not.toBeInTheDocument();
    });
  });
});
