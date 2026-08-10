import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import Permission from '@/types/models/Permission';

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

// Vocabulário fixo do sistema (§5) — só leitura, nunca criado por API. Usado pra
// popular o seletor de Resource/Action/Feature em UserPermissionOverride e
// AccessPolicy (§10.7), em vez de campos de texto livre sujeitos a erro de digitação.
const permissionService = {
  getAll: async (): Promise<ServiceResult<Permission[]>> => {
    try {
      // pageSize=100 (o teto de CatalogQuery) — o vocabulário inteiro precisa caber
      // num único carregamento, ou o seletor de permissão ficaria incompleto.
      const res = await api.get<PagedResult<Permission>>('AdminPermission/?pageSize=100');
      return { success: true, message: '', data: res.data.items };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default permissionService;
