import { ReactNode, createContext, useEffect, useState } from 'react';

interface ThemeContextType {
  theme: string;
  fontSize: number;
  toggleTheme: (newTheme: string) => void;
  changeFontSize: (newFontSize: number) => void;
}

interface ThemeProviderProps {
  children: ReactNode;
}

const VALID_THEMES = ['light', 'high-contrast'];
const DEFAULT_THEME = 'light';
const DEFAULT_FONT_SIZE = 16;
const MIN_FONT_SIZE = 10;
const MAX_FONT_SIZE = 44;

// §11: mesmo limite usado tanto na leitura inicial (localStorage pode ter sido
// adulterado/corrompido fora dos limites seguros) quanto na troca em tempo real.
const clampFontSize = (value: number) =>
  Math.min(MAX_FONT_SIZE, Math.max(MIN_FONT_SIZE, value));

export const ThemeContext = createContext<ThemeContextType>({
  theme: DEFAULT_THEME,
  fontSize: DEFAULT_FONT_SIZE,
  changeFontSize() {},
  toggleTheme() {},
});

export function ThemeProvider({ children }: ThemeProviderProps) {
  const [theme, setTheme] = useState(DEFAULT_THEME);
  const [fontSize, setFontSize] = useState(DEFAULT_FONT_SIZE); // Tamanho da fonte em pixels

  useEffect(() => {
    const storedTheme = localStorage.getItem('theme');
    const savedTheme = VALID_THEMES.includes(storedTheme ?? '')
      ? (storedTheme as string)
      : DEFAULT_THEME;

    const storedFontSize = Number(localStorage.getItem('fontSize'));
    const savedFontSize = Number.isFinite(storedFontSize) && storedFontSize > 0
      ? clampFontSize(storedFontSize)
      : DEFAULT_FONT_SIZE;

    setTheme(savedTheme);
    setFontSize(savedFontSize);
    document.documentElement.setAttribute('data-theme', savedTheme);
    document.documentElement.style.setProperty(
      '--font-size',
      `${savedFontSize}px`
    );
  }, []);

  const toggleTheme = (newTheme: string) => {
    const safeTheme = VALID_THEMES.includes(newTheme) ? newTheme : DEFAULT_THEME;
    setTheme(safeTheme);
    localStorage.setItem('theme', safeTheme);
    document.documentElement.setAttribute('data-theme', safeTheme);
  };

  const changeFontSize = (newFontSize: number) => {
    if (!Number.isFinite(newFontSize)) return;
    const safeFontSize = clampFontSize(newFontSize);
    setFontSize(safeFontSize);
    localStorage.setItem('fontSize', `${safeFontSize}`);

    // Atualiza a variável CSS do tamanho da fonte
    document.documentElement.style.setProperty(
      '--font-size',
      `${safeFontSize}px`
    );
  };

  return (
    <ThemeContext.Provider
      value={{ theme, fontSize, toggleTheme, changeFontSize }}
    >
      {children}
    </ThemeContext.Provider>
  );
}
