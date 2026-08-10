import { TextInput } from '@/components/FormControls';
import { ModalProps, FormModal } from '@/components/Modal';
import useFormData from '@/hooks/useFormData';
import OrganizationalRole from '@/types/models/OrganizationalRole';
import { useEffect } from 'react';

interface OrganizationalRoleFormModalProps extends Omit<ModalProps, 'children'> {
  onSubmit: (data: OrganizationalRole) => Promise<void>;
  objectData?: OrganizationalRole;
}

export default function OrganizationalRoleFormModal({
  onClose,
  onSubmit,
  isOpen,
  objectData,
}: OrganizationalRoleFormModalProps) {
  const { data, setData, updateField, reset } = useFormData<OrganizationalRole>({
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

  const title = objectData?.id
    ? 'Editar Papel Organizacional'
    : 'Criar Papel Organizacional';

  return (
    <FormModal
      isOpen={isOpen}
      onClose={handleClose}
      onSubmit={handleSubmit}
      title={title}
    >
      <div className='flex flex-col gap-4'>
        <TextInput<OrganizationalRole>
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
