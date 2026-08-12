import { useCallback, useEffect, useState } from 'react';
import moduleCatalogService from '../services/moduleCatalogService';
import ModuleCard from '../components/ModuleCard';
import ModuleCatalogItem from '@/types/models/ModuleCatalogItem';
import OperationalState from '@/types/models/OperationalState';

type CatalogStatus = 'loading' | 'ready' | 'error';

// §5.1/§5.3 — consome só GET /api/v1/me/modules (nenhuma outra chamada de
// domínio). Estados: carregamento, vazio (nenhum módulo autorizado/habilitado
// — não é erro), falha recuperável (com retry e correlationId quando o
// backend fornecer) e pronto.
export default function CatalogPage() {
  const [status, setStatus] = useState<CatalogStatus>('loading');
  const [modules, setModules] = useState<ModuleCatalogItem[]>([]);
  const [errorMessage, setErrorMessage] = useState('');
  const [correlationId, setCorrelationId] = useState<string | undefined>();

  const load = useCallback(async () => {
    setStatus('loading');
    const res = await moduleCatalogService.getModules();

    if (!res.success || !res.data) {
      setErrorMessage(res.message || 'Não foi possível carregar o catálogo.');
      setCorrelationId(res.correlationId);
      setStatus('error');
      return;
    }

    // Defesa em profundidade própria do front-end — o backend já omite
    // DISABLED de /me/modules (§3.2), mas o catálogo nunca deve renderizar
    // esse estado mesmo diante de uma resposta inesperada.
    setModules(res.data.filter((m) => m.operationalState !== OperationalState.DISABLED));
    setStatus('ready');
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  if (status === 'loading') {
    return (
      <div className='flex items-center justify-center h-full w-full py-24' role='status'>
        <span className='text-textSecondary'>Carregando catálogo...</span>
      </div>
    );
  }

  if (status === 'error') {
    return (
      <div className='flex flex-col items-center justify-center h-full w-full py-24 gap-3' role='alert'>
        <h1 className='text-xl font-bold text-danger'>Não foi possível carregar o catálogo</h1>
        <p className='text-textSecondary text-center max-w-md'>{errorMessage}</p>
        {correlationId && (
          <p className='text-textSecondary text-xs'>Identificador de correlação: {correlationId}</p>
        )}
        <button
          type='button'
          onClick={load}
          className='bg-primary h-10 px-4 rounded text-neutralWhite font-semibold hover:bg-hoverButton transition-colors'
        >
          Tentar novamente
        </button>
      </div>
    );
  }

  if (modules.length === 0) {
    return (
      <div className='flex items-center justify-center h-full w-full py-24'>
        <p className='text-textSecondary text-center max-w-md'>
          Nenhum módulo disponível para sua conta no momento.
        </p>
      </div>
    );
  }

  return (
    <ul
      className='grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 p-4 md:p-8 w-full list-none'
      aria-label='Catálogo de módulos'
    >
      {modules
        .slice()
        .sort((a, b) => a.order - b.order)
        .map((module) => (
          <li key={module.key}>
            <ModuleCard module={module} />
          </li>
        ))}
    </ul>
  );
}
