import { useContext } from 'react';
import {
  CircleHalf,
  MagnifyingGlassMinus,
  MagnifyingGlassPlus,
} from '@phosphor-icons/react';
import { ThemeContext } from '@/contexts/ThemeContext';
import BreadcrumbPageTitle from '@/components/BreadcrumbPageTitle';
import Button from '@/components/Button';

// §11: controles reais reaproveitando o ThemeContext (mesma lógica de limite e
// persistência do AccessibilityBar, sem duplicar) — pra quem chega direto nesta
// página sem passar pelo cabeçalho.
export default function AccessibilityPage() {
  const { theme, fontSize, toggleTheme, changeFontSize } =
    useContext(ThemeContext);

  const changeTheme = () => {
    toggleTheme(theme === 'light' ? 'high-contrast' : 'light');
  };

  return (
    <div>
      <BreadcrumbPageTitle title='Acessibilidade' />
      <div className='bg-neutralWhite px-6 py-6 max-w-2xl mx-auto rounded-lg shadow-md mt-10 flex flex-col gap-6'>
        <section>
          <h2 className='text-lg font-semibold text-textPrimary mb-2'>
            Navegação por teclado
          </h2>
          <p className='text-textSecondary text-sm'>
            Todos os campos, botões, links e ações desta plataforma podem ser
            alcançados e acionados só com o teclado — use Tab/Shift+Tab para
            mover o foco entre eles, Enter ou Espaço para ativar, e Esc para
            fechar janelas (modais). O elemento com foco é sempre destacado
            visualmente.
          </p>
        </section>
        <section>
          <h2 className='text-lg font-semibold text-textPrimary mb-2'>
            Alto contraste
          </h2>
          <p className='text-textSecondary text-sm mb-3'>
            Alterna as cores da interface para um esquema de maior contraste.
            A preferência fica salva neste navegador.
          </p>
          <Button
            label={
              theme === 'light'
                ? 'Ativar alto contraste'
                : 'Desativar alto contraste'
            }
            icon={<CircleHalf weight='fill' />}
            onClick={changeTheme}
            color='secondary'
            size='medium'
          />
        </section>
        <section>
          <h2 className='text-lg font-semibold text-textPrimary mb-2'>
            Tamanho da fonte
          </h2>
          <p className='text-textSecondary text-sm mb-3'>
            Ajusta o tamanho do texto em toda a plataforma (atual: {fontSize}
            px). A preferência fica salva neste navegador.
          </p>
          <div className='flex gap-3'>
            <Button
              label='Diminuir fonte'
              icon={<MagnifyingGlassMinus weight='bold' />}
              onClick={() => changeFontSize(fontSize - 2)}
              color='secondary'
              size='medium'
            />
            <Button
              label='Aumentar fonte'
              icon={<MagnifyingGlassPlus weight='bold' />}
              onClick={() => changeFontSize(fontSize + 2)}
              color='secondary'
              size='medium'
            />
          </div>
        </section>
      </div>
    </div>
  );
}
