import { ReactNode, useEffect, useRef } from 'react';
import { createPortal } from 'react-dom';
import { X } from '@phosphor-icons/react';
import { ModalProps } from './types';

interface ModalHeaderProps {
  title?: string;
  showCloseButton?: boolean;
  onClose: () => void;
}

interface ModalContentProps {
  children: ReactNode;
}

interface ModalFooterProps {
  children: ReactNode;
}

const FOCUSABLE_SELECTOR =
  'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])';

export const ModalRoot = ({
  isOpen,
  onClose,
  children,
  closeOnBackdropClick = true,
}: ModalProps) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const previouslyFocusedRef = useRef<HTMLElement | null>(null);

  // Quem renderiza o conteúdo do modal costuma recriar `onClose` a cada
  // keystroke (handlers inline, sem useCallback) — uma ref evita que o efeito
  // abaixo dependa dessa referência instável.
  const onCloseRef = useRef(onClose);
  useEffect(() => {
    onCloseRef.current = onClose;
  });

  // §11: foco entra no modal ao abrir, fica preso nele enquanto aberto (Tab não
  // escapa pro conteúdo de fundo) e volta pro controle que abriu ao fechar.
  // Depende só de `isOpen` de propósito — se dependesse de `onClose` também, o
  // efeito reexecutaria a cada re-render do conteúdo (ex.: a cada tecla digitada
  // num campo do formulário dentro do modal) e roubaria o foco de volta pro
  // primeiro elemento focável no meio da digitação.
  useEffect(() => {
    if (!isOpen) return;

    previouslyFocusedRef.current = document.activeElement as HTMLElement | null;
    document.body.style.overflow = 'hidden';

    const getFocusable = () => {
      const container = containerRef.current;
      return container
        ? Array.from(container.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR))
        : [];
    };

    // Síncrono — `children` já está montado no DOM neste ponto (mesmo commit
    // que produziu `containerRef.current`), não precisa de outro tick.
    const focusable = getFocusable();
    (focusable[0] ?? containerRef.current)?.focus();

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onCloseRef.current();
        return;
      }
      if (event.key !== 'Tab') return;

      const focusable = getFocusable();
      if (focusable.length === 0) {
        event.preventDefault();
        return;
      }
      const first = focusable[0];
      const last = focusable[focusable.length - 1];

      if (event.shiftKey && document.activeElement === first) {
        event.preventDefault();
        last.focus();
      } else if (!event.shiftKey && document.activeElement === last) {
        event.preventDefault();
        first.focus();
      }
    };

    window.addEventListener('keydown', handleKeyDown);

    return () => {
      document.body.style.overflow = 'unset';
      window.removeEventListener('keydown', handleKeyDown);
      previouslyFocusedRef.current?.focus?.();
    };
  }, [isOpen]);

  if (!isOpen) {
    return null;
  }

  return createPortal(
    <div
      // Overlay
      className='fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50'
      onClick={closeOnBackdropClick ? onClose : undefined}
    >
      <div
        // Container da Modal
        ref={containerRef}
        role='dialog'
        aria-modal='true'
        tabIndex={-1}
        className='relative bg-white transition-all rounded-[10px] shadow-lg w-full max-w-xl px-4 py-6 bg-neutralWhite'
        onClick={(e) => e.stopPropagation()}
      >
        {children}
      </div>
    </div>,
    document.body
  );
};

export const ModalHeader = ({
  title,
  showCloseButton = true,
  onClose,
}: ModalHeaderProps) => {
  return (
    <>
      <div className='flex items-center px-2 pb-1'>
        {title && (
          <h3 className='text-xl font-semibold text-textPrimary'>{title}</h3>
        )}
        {showCloseButton && (
          <button
            type='button'
            aria-label='Fechar'
            className='ml-auto h-6 w-6 flex items-center justify-center bg-transparent text-textSecondary'
            onClick={onClose}
          >
            <X size={24} />
          </button>
        )}
      </div>

      {/* Separador */}
      <hr className='border-t border-neutralDark w-full mx-auto' />
    </>
  );
};

export const ModalContent = ({ children }: ModalContentProps) => {
  return <div className='px-3 py-4 text-textSecondary'>{children}</div>;
};

export const ModalFooter = ({ children }: ModalFooterProps) => {
  return (
    <>
      <hr className='border-t border-neutralDark w-full mx-auto' />
      <div className='flex justify-end gap-7 px-2 pt-4'>{children}</div>
    </>
  );
};
