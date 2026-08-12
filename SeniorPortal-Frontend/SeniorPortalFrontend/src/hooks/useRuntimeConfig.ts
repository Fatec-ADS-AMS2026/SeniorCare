import { useContext } from 'react';
import { RuntimeConfigContext } from '@/contexts/RuntimeConfigContext';

export default function useRuntimeConfig() {
  return useContext(RuntimeConfigContext);
}
