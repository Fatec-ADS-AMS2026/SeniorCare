import { useContext } from 'react';
import { AuthContext } from '@/contexts/AuthContext';

export default function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth precisa ser usado dentro de um AuthProvider.');
  }
  return context;
}
