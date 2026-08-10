import { TextInput } from '@/components/FormControls';
import { ModalProps, FormModal } from '@/components/Modal';
import useFormData from '@/hooks/useFormData';
import PermissionGroup from '@/types/models/PermissionGroup';
import { useEffect } from 'react';

interface PermissionGroupFormModalProps extends Omit<ModalProps, 'children'> {
  onSubmit: (data: PermissionGroup) => Promise<void>;
  objectData?: PermissionGroup;
}

export default function PermissionGroupFormModal({
  onClose,
  onSubmit,
  isOpen,
  objectData,
}: PermissionGroupFormModalProps) {
  const { data, setData, updateField, reset } = useFormData<PermissionGroup>({
    id: '',
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

  const title = objectData?.id
    ? 'Editar Grupo de Permissão'
    : 'Criar Grupo de Permissão';

  return (
    <FormModal
      isOpen={isOpen}
      onClose={handleClose}
      onSubmit={handleSubmit}
      title={title}
    >
      <div className='flex flex-col gap-4'>
        <TextInput<PermissionGroup>
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
