import { useContext } from 'react';
import { TextAa, CircleHalf } from '@phosphor-icons/react';
import { ThemeContext } from '@/contexts/ThemeContext';

const FONT_STEP = 2;
const MIN_FONT_SIZE = 10;
const MAX_FONT_SIZE = 44;

// §5.4 — contrato de preferências (senior-portal-contracts.md §5): reusa
// ThemeContext tal como está (chaves `theme`/`fontSize`, mesmos limites),
// sem introduzir estado novo. Compartilhado automaticamente com care/stock
// assim que estiverem sob a mesma origem — nenhuma sincronização aqui.
export default function PreferencesControls() {
  const { theme, fontSize, toggleTheme, changeFontSize } = useContext(ThemeContext);
  const isHighContrast = theme === 'high-contrast';

  return (
    <div className='flex items-center gap-4' role='group' aria-label='Preferências de acessibilidade'>
      <button
        type='button'
        onClick={() => toggleTheme(isHighContrast ? 'light' : 'high-contrast')}
        aria-pressed={isHighContrast}
        className='flex items-center gap-1 text-sm text-textSecondary hover:text-primary transition-colors'
      >
        <CircleHalf size={20} aria-hidden='true' />
        {isHighContrast ? 'Contraste padrão' : 'Alto contraste'}
      </button>

      <div className='flex items-center gap-2 text-sm text-textSecondary'>
        <TextAa size={20} aria-hidden='true' />
        <button
          type='button'
          onClick={() => changeFontSize(fontSize - FONT_STEP)}
          disabled={fontSize <= MIN_FONT_SIZE}
          aria-label='Diminuir tamanho da fonte'
          className='w-7 h-7 flex items-center justify-center border border-textSecondary rounded disabled:opacity-40'
        >
          A-
        </button>
        <span aria-live='polite'>{fontSize}px</span>
        <button
          type='button'
          onClick={() => changeFontSize(fontSize + FONT_STEP)}
          disabled={fontSize >= MAX_FONT_SIZE}
          aria-label='Aumentar tamanho da fonte'
          className='w-7 h-7 flex items-center justify-center border border-textSecondary rounded disabled:opacity-40'
        >
          A+
        </button>
      </div>
    </div>
  );
}
