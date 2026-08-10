import { TextInput } from '@/components/FormControls';
import { ModalProps, FormModal } from '@/components/Modal';
import useFormData from '@/hooks/useFormData';
import Role from '@/types/models/Role';
import { useEffect } from 'react';

interface RoleFormModalProps extends Omit<ModalProps, 'children'> {
  onSubmit: (data: Role) => Promise<void>;
  objectData?: Role;
}

export default function RoleFormModal({
  onClose,
  onSubmit,
  isOpen,
  objectData,
}: RoleFormModalProps) {
  const { data, setData, updateField, reset } = useFormData<Role>({
    id: '',
    institutionId: '',
    name: '',
    rowVersion: 0,
  });

  useEffect(() => {
    if (!isOpen) return;
    if (objectData) {
      setData(objectData);
    } else {
      reset();
    }
  }, [isOpen, objectData, setData, reset]);

  const handleSubmit = async () => {
    await onSubmit(data);
    handleClose();
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  const title = objectData?.id ? 'Editar Papel' : 'Criar Papel';

  return (
    <FormModal
      isOpen={isOpen}
      onClose={handleClose}
      onSubmit={handleSubmit}
      title={title}
    >
      <div className='flex flex-col gap-4'>
        <TextInput<Role>
          name='name'
          label='Nome'
          onChange={updateField}
          value={data.name}
          required
        />
      </div>
    </FormModal>
  );
}
