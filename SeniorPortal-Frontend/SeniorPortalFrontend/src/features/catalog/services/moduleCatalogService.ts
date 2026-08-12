import { api } from '@/features/api';
import { handleServiceError } from '@/utils/serviceUtils';
import ServiceResult from '@/types/app/ServiceResult';
import ModuleCatalogItem from '@/types/models/ModuleCatalogItem';

const moduleCatalogService = {
  getModules: async (): Promise<ServiceResult<ModuleCatalogItem[]>> => {
    try {
      const res = await api.get<ModuleCatalogItem[]>('me/modules');
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  },
};

export default moduleCatalogService;
