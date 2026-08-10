import { TextInput } from '@/components/FormControls';
import { ModalProps, FormModal } from '@/components/Modal';
import useFormData from '@/hooks/useFormData';
import { useEffect } from 'react';

interface AdminUserCreateFormData {
  email: string;
  displayName: string;
}

interface AdminUserFormModalProps extends Omit<ModalProps, 'children'> {
  onSubmit: (data: AdminUserCreateFormData) => Promise<void>;
}

// Só criação — não há campo de senha administrativa (§10.6): a conta nasce
// PROVISIONED e só ganha credencial pelo fluxo de ativação por token.
export default function AdminUserFormModal({
  onClose,
  onSubmit,
  isOpen,
}: AdminUserFormModalProps) {
  const { data, updateField, reset } = useFormData<AdminUserCreateFormData>({
    email: '',
    displayName: '',
  });

  useEffect(() => {
    if (!isOpen) reset();
  }, [isOpen, reset]);

  const handleSubmit = async () => {
    await onSubmit(data);
    handleClose();
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <FormModal
      isOpen={isOpen}
      onClose={handleClose}
      onSubmit={handleSubmit}
      title='Criar Usuário'
    >
      <div className='flex flex-col gap-4'>
        <TextInput<AdminUserCreateFormData>
          name='displayName'
          label='Nome'
          onChange={updateField}
          value={data.displayName}
          required
        />
        <TextInput<AdminUserCreateFormData>
          name='email'
          label='E-mail'
          type='email'
          onChange={updateField}
          value={data.email}
          required
        />
      </div>
    </FormModal>
  );
}
