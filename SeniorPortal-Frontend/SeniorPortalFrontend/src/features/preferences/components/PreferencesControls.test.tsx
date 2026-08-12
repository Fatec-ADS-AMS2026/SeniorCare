import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { axe } from 'jest-axe';
import PreferencesControls from './PreferencesControls';
import { ThemeProvider } from '@/contexts/ThemeContext';

function renderControls() {
  return render(
    <ThemeProvider>
      <PreferencesControls />
    </ThemeProvider>
  );
}

describe('PreferencesControls', () => {
  it('toggles high-contrast theme and persists it', async () => {
    const user = userEvent.setup();
    renderControls();

    const toggle = screen.getByRole('button', { name: /Alto contraste/ });
    await user.click(toggle);

    expect(screen.getByRole('button', { name: /Contraste padrão/ })).toHaveAttribute('aria-pressed', 'true');
    expect(localStorage.getItem('theme')).toBe('high-contrast');
    expect(document.documentElement.getAttribute('data-theme')).toBe('high-contrast');
  });

  it('increases and decreases the font size within the documented limits', async () => {
    const user = userEvent.setup();
    renderControls();

    await user.click(screen.getByRole('button', { name: 'Aumentar tamanho da fonte' }));
    expect(screen.getByText('18px')).toBeInTheDocument();
    expect(localStorage.getItem('fontSize')).toBe('18');

    await user.click(screen.getByRole('button', { name: 'Diminuir tamanho da fonte' }));
    await user.click(screen.getByRole('button', { name: 'Diminuir tamanho da fonte' }));
    expect(screen.getByText('14px')).toBeInTheDocument();
  });

  it('has no detectable accessibility violations', async () => {
    const { container } = renderControls();
    expect(await axe(container)).toHaveNoViolations();
  });
});
