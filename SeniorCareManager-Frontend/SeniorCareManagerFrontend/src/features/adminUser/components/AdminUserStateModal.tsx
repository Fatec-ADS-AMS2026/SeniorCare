import { FormEvent, useEffect, useState } from 'react';
import * as Modal from '@/components/Modal/BaseModal';
import Button from '@/components/Button';
import { SelectInput, TextInput } from '@/components/FormControls';
import { AccountState } from '@/types/models/AdminUser';

interface StateChangeFormData {
  accountState: string;
  currentPassword: string;
}

interface AdminUserStateModalProps {
  isOpen: boolean;
  onClose: () => void;
  currentState?: AccountState;
  onConfirm: (accountState: AccountState, currentPassword: string) => void | Promise<void>;
  isSubmitting?: boolean;
  error?: string;
}

const STATE_OPTIONS = [
  { label: 'Provisionada', value: AccountState.PROVISIONED },
  { label: 'Ativa', value: AccountState.ACTIVE },
  { label: 'Inativa', value: AccountState.INACTIVE },
  { label: 'Bloqueada', value: AccountState.BLOCKED },
  { label: 'Expirada', value: AccountState.EXPIRED },
];

// Troca de estado de conta exige reautenticação (senha de quem age, não da conta
// afetada, §10.6) — combina o select de estado com a senha atual num só passo.
export default function AdminUserStateModal({
  isOpen,
  onClose,
  onConfirm,
  currentState,
  isSubmitting = false,
  error,
}: AdminUserStateModalProps) {
  const [accountState, setAccountState] = useState<string>(
    String(currentState ?? AccountState.ACTIVE)
  );
  const [currentPassword, setCurrentPassword] = useState('');

  useEffect(() => {
    if (isOpen) {
      setAccountState(String(currentState ?? AccountState.ACTIVE));
      setCurrentPassword('');
    }
  }, [isOpen, currentState]);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    onConfirm(Number(accountState) as AccountState, currentPassword);
  };

  return (
    <Modal.ModalRoot isOpen={isOpen} onClose={onClose}>
      <Modal.ModalHeader title='Alterar estado da conta' onClose={onClose} />
      <form onSubmit={handleSubmit}>
        <Modal.ModalContent>
          <div className='flex flex-col gap-4'>
            <SelectInput<StateChangeFormData>
              name='accountState'
              label='Novo estado'
              value={accountState}
              onChange={(_, value) => setAccountState(value)}
              options={STATE_OPTIONS}
              required
            />
            <TextInput<StateChangeFormData>
              name='currentPassword'
              label='Sua senha atual'
              type='password'
              value={currentPassword}
              onChange={(_, value) => setCurrentPassword(value)}
              required
            />
            {error && <p className='text-danger text-sm'>{error}</p>}
          </div>
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
