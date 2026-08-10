import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import Role from '@/types/models/Role';

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface RoleUpsertRequest {
  name: string;
  rowVersion?: number;
}

// Id é Guid (não number) — não passa por generateGenericMethods. Link/unlink de
// grupos de permissão e usuários (POST/DELETE .../permission-groups,.../users) não
// tem endpoint de leitura (AdminRoleController não expõe "quais grupos/usuários
// este papel tem hoje") — por isso não há UI de gestão de vínculos aqui (§10.7,
// achado documentado em tasks.md: gerenciar vínculos sem visibilidade do estado
// atual seria enganoso).
const roleService = {
  getAll: async (): Promise<ServiceResult<Role[]>> => {
    try {
      const res = await api.get<PagedResult<Role>>('AdminRole/');
      return { success: true, message: '', data: res.data.items };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  create: async (request: RoleUpsertRequest): Promise<ServiceResult<Role>> => {
    try {
      const res = await api.post<Role>('AdminRole/', { name: request.name });
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  update: async (id: string, request: RoleUpsertRequest): Promise<ServiceResult<Role>> => {
    try {
      const res = await api.put<Role>(`AdminRole/${id}`, request);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },

  deleteById: async (id: string): Promise<ServiceResult<undefined>> => {
    try {
      await api.delete(`AdminRole/${id}`);
      return { success: true, message: 'Excluído com sucesso' };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default roleService;
