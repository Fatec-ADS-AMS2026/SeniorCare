import { useCallback, useEffect, useState } from 'react';
import permissionGroupService from '../services/permissionGroupService';
import PermissionGroup from '@/types/models/PermissionGroup';
import Table from '@/components/Table';
import { TableColumn } from '@/components/Table/types';
import { Pencil, Plus, Trash } from '@phosphor-icons/react';
import BreadcrumbPageTitle from '@/components/BreadcrumbPageTitle';
import SearchBar from '@/components/SearchBar';
import Button from '@/components/Button';
import { AlertModal, ConfirmModal } from '@/components/Modal';
import PermissionGroupFormModal from '../components/PermissionGroupFormModal';

export default function PermissionGroupOverview() {
  const columns: TableColumn<PermissionGroup>[] = [
    { label: 'Nome', attribute: 'name' },
  ];
  const [data, setData] = useState<PermissionGroup[]>([]);
  const [originalData, setOriginalData] = useState<PermissionGroup[]>([]);
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isAlertModalOpen, setIsAlertModalOpen] = useState(false);
  const [alertMessage, setAlertMessage] = useState('');
  const [alertType, setAlertType] = useState<'info' | 'success' | 'error'>(
    'info'
  );
  const [currentId, setCurrentId] = useState<string | null>(null);
  const [editingItem, setEditingItem] = useState<PermissionGroup | undefined>();

  const showAlert = (message: string, type: 'info' | 'success' | 'error') => {
    setAlertMessage(message);
    setAlertType(type);
    setIsAlertModalOpen(true);
  };

  const fetchData = useCallback(async () => {
    const res = await permissionGroupService.getAll();
    if (res.success && res.data) {
      setData([...res.data]);
      setOriginalData([...res.data]);
    } else {
      showAlert(res.message, 'error');
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleSearch = (searchTerm: string) => {
    if (!searchTerm) {
      setData(originalData);
      return;
    }
    setData(
      originalData.filter((g) =>
        g.name.toLowerCase().includes(searchTerm.toLowerCase())
      )
    );
  };

  const openCreateModal = () => {
    setEditingItem(undefined);
    setCurrentId(null);
    setIsFormModalOpen(true);
  };

  const openEditModal = (id: string) => {
    const item = data.find((row) => row.id === id);
    if (item) {
      setEditingItem(item);
      setCurrentId(id);
      setIsFormModalOpen(true);
    } else {
      showAlert('Registro não encontrado', 'error');
    }
  };

  const openDeleteModal = (id: string) => {
    setCurrentId(id);
    setIsDeleteModalOpen(true);
  };

  const handleSave = async (model: PermissionGroup) => {
    const res =
      currentId !== null
        ? await permissionGroupService.update(currentId, {
            name: model.name,
            rowVersion: model.rowVersion,
          })
        : await permissionGroupService.create({ name: model.name });

    if (res.success) {
      await fetchData();
      showAlert(
        `Grupo de permissão "${res.data?.name}" ${currentId !== null ? 'atualizado' : 'criado'} com sucesso!`,
        'success'
      );
    } else {
      showAlert(
        res.message || 'Erro inesperado ao salvar o grupo de permissão.',
        'error'
      );
      throw new Error(res.message);
    }
  };

  const deletePermissionGroup = async () => {
    if (!currentId) return;
    const res = await permissionGroupService.deleteById(currentId);
    if (res.success) {
      setIsDeleteModalOpen(false);
      const itemName = data.find((item) => item.id === currentId)?.name || '';
      setCurrentId(null);
      await fetchData();
      showAlert(
        `Grupo de permissão "${itemName}" excluído com sucesso!`,
        'success'
      );
    } else {
      showAlert(
        res.message || 'Erro inesperado ao excluir o grupo de permissão.',
        'error'
      );
    }
  };

  const Actions = ({ id }: { id: string }) => (
    <>
      <button
        onClick={() => openEditModal(id)}
        className='text-edit hover:text-hoverEdit'
      >
        <Pencil className='size-6' weight='fill' />
      </button>
      <button
        onClick={() => openDeleteModal(id)}
        className='text-danger hover:text-hoverDanger'
      >
        <Trash className='size-6' weight='fill' />
      </button>
    </>
  );

  return (
    <div>
      <BreadcrumbPageTitle title='Grupos de Permissão' />
      <div className='bg-neutralWhite px-6 py-6 max-w-[95%] mx-auto rounded-lg shadow-md mt-10'>
        <PermissionGroupFormModal
          isOpen={isFormModalOpen}
          onClose={() => {
            setIsFormModalOpen(false);
            setEditingItem(undefined);
          }}
          onSubmit={handleSave}
          objectData={editingItem}
        />
        <ConfirmModal
          isOpen={isDeleteModalOpen}
          onClose={() => setIsDeleteModalOpen(false)}
          onConfirm={deletePermissionGroup}
          title='Deseja realmente excluir esse Grupo de Permissão?'
          message='Ao excluir este grupo, ele será removido permanentemente do sistema.'
        />
        <AlertModal
          isOpen={isAlertModalOpen}
          onClose={() => setIsAlertModalOpen(false)}
          message={alertMessage}
          type={alertType}
        />
        <div className='flex items-center justify-between mb-4'>
          <SearchBar
            action={handleSearch}
            placeholder='Buscar grupo de permissão...'
          />
          <Button
            label='Adicionar'
            icon={<Plus />}
            iconPosition='left'
            color='success'
            size='medium'
            onClick={openCreateModal}
          />
        </div>
        <Table<PermissionGroup>
          columns={columns}
          data={data}
          actions={(id) => <Actions id={id} />}
        />
      </div>
    </div>
  );
}
