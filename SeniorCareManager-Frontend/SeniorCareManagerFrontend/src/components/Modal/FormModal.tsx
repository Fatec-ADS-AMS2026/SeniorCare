import * as Modal from './BaseModal';
import { FormEvent } from 'react';
import Button from '../Button';
import { Plus, X } from '@phosphor-icons/react';
import { useModalForm } from '@/hooks/useModalForm';
import { ModalProps } from './types';

interface FormModalProps extends ModalProps {
  onSubmit: (data?: unknown) => void;
}

export default function FormModal({
  isOpen,
  onClose,
  onSubmit,
  title,
  children,
  closeOnBackdropClick = false,
  showCloseButton = true,
}: FormModalProps) {
  const { isSubmitting, handleSubmit } = useModalForm();

  const handleFormSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    try {
      await handleSubmit(onSubmit, onClose);
    } catch {
      // useModalForm.handleSubmit relança de propósito pra manter a modal
      // aberta em caso de erro (onClose não é chamado) — o próprio onSubmit já
      // trata a mensagem de erro (alerta/estado local); sem este catch, todo
      // FormModal com um onSubmit que falha gera uma promise rejeitada sem
      // handler (achado: reproduzível em qualquer tela existente que usa
      // FormModal — Religion/Position/HealthInsurancePlan também têm esse
      // padrão de "throw" no onSubmit).
    }
  };

  return (
    <Modal.ModalRoot
      isOpen={isOpen}
      onClose={onClose}
      closeOnBackdropClick={closeOnBackdropClick && !isSubmitting}
    >
      <form onSubmit={handleFormSubmit}>
        <Modal.ModalHeader
          onClose={onClose}
          showCloseButton={showCloseButton}
          title={title}
        />
        <Modal.ModalContent>{children}</Modal.ModalContent>
        <Modal.ModalFooter>
          <Button
            type='button'
            onClick={onClose}
            label='Cancelar'
            color='danger'
            icon={<X weight='bold' />}
            disabled={isSubmitting}
          />
          <Button
            type='submit'
            label={isSubmitting ? 'Salvando...' : 'Salvar'}
            color='success'
            icon={<Plus weight='bold' />}
            disabled={isSubmitting}
          />
        </Modal.ModalFooter>
      </form>
    </Modal.ModalRoot>
  );
}
