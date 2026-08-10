import { FormEvent, useEffect, useState } from 'react';
import * as Modal from './BaseModal';
import { ModalProps } from './types';
import Button from '../Button';
import { TextInput } from '../FormControls';

interface ReauthModalProps extends Omit<ModalProps, 'children'> {
  message: string;
  onConfirm: (currentPassword: string) => void | Promise<void>;
  isSubmitting?: boolean;
  error?: string;
}

interface ReauthFormData {
  currentPassword: string;
}

// Modal de reautenticação (§10.6-10.8) — ações sensíveis (mudar estado de conta,
// revogar sessão, alterar parâmetros de segurança) exigem a senha atual de quem
// está agindo, não da conta afetada. Único padrão de reautenticação do projeto até
// agora — reaproveitar aqui se aparecer de novo.
export default function ReauthModal({
  isOpen,
  onClose,
  onConfirm,
  message,
  title = 'Confirme sua senha',
  isSubmitting = false,
  error,
  ...props
}: ReauthModalProps) {
  const [currentPassword, setCurrentPassword] = useState('');

  useEffect(() => {
    if (!isOpen) setCurrentPassword('');
  }, [isOpen]);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    onConfirm(currentPassword);
  };

  return (
    <Modal.ModalRoot isOpen={isOpen} onClose={onClose} {...props}>
      <Modal.ModalHeader title={title} onClose={onClose} />
      <form onSubmit={handleSubmit}>
        <Modal.ModalContent>
          <p className='mb-4'>{message}</p>
          <TextInput<ReauthFormData>
            label='Sua senha atual'
            name='currentPassword'
            type='password'
            value={currentPassword}
            onChange={(_, value) => setCurrentPassword(value)}
            required
          />
          {error && <p className='text-danger text-sm mt-2'>{error}</p>}
        </Modal.ModalContent>
        <Modal.ModalFooter>
          <Button
            type='button'
            label='Cancelar'
            onClick={onClose}
            color='textSecondary'
            className='font-semibold'
            size='medium'
          />
          <Button
            type='submit'
            label={isSubmitting ? 'Confirmando...' : 'Confirmar'}
            color='primary'
            className='font-semibold'
            size='medium'
            disabled={isSubmitting}
          />
        </Modal.ModalFooter>
      </form>
    </Modal.ModalRoot>
  );
}
