import { ReactNode, createContext, useEffect, useState } from 'react';

// §4.1 — mecanismo concreto travado por
// docs/architecture/senior-portal-contracts.md §1: `public-config.json` servido
// ao lado do bundle (public/, nunca importado pelo JS — o bundler não o inlina)
// e lido em runtime, uma vez, antes de renderizar o cabeçalho. Em produção, o
// entrypoint do container (§8) sobrescreve esse arquivo estático a partir de uma
// variável de ambiente; aqui só existe o arquivo padrão de dev/build
// (public/public-config.json) e o carregamento em runtime. Chave `publicName`,
// fallback "SeniorCare" se o arquivo faltar, for inválido ou a rede falhar — o
// portal nunca trava esperando essa configuração.
export interface RuntimeConfig {
  publicName: string;
}

export const DEFAULT_RUNTIME_CONFIG: RuntimeConfig = { publicName: 'SeniorCare' };

interface RuntimeConfigContextType {
  config: RuntimeConfig;
  loaded: boolean;
}

export const RuntimeConfigContext = createContext<RuntimeConfigContextType>({
  config: DEFAULT_RUNTIME_CONFIG,
  loaded: false,
});

interface RuntimeConfigProviderProps {
  children: ReactNode;
}

function isValidPublicName(value: unknown): value is string {
  return typeof value === 'string' && value.trim().length > 0;
}

export function RuntimeConfigProvider({ children }: RuntimeConfigProviderProps) {
  const [config, setConfig] = useState<RuntimeConfig>(DEFAULT_RUNTIME_CONFIG);
  const [loaded, setLoaded] = useState(false);

  useEffect(() => {
    let cancelled = false;

    // cache: 'no-store' — este arquivo pode ser trocado pelo entrypoint do
    // container sem rebuild do bundle (§8); um cache de navegador/CDN
    // agressivo faria o portal exibir o nome público antigo indefinidamente.
    fetch('/public-config.json', { cache: 'no-store' })
      .then((res) => (res.ok ? res.json() : Promise.reject(new Error(`status ${res.status}`))))
      .then((data: unknown) => {
        if (cancelled) return;
        const publicName = data && typeof data === 'object' && 'publicName' in data && isValidPublicName(data.publicName)
          ? data.publicName
          : DEFAULT_RUNTIME_CONFIG.publicName;
        setConfig({ publicName });
      })
      .catch(() => {
        if (!cancelled) setConfig(DEFAULT_RUNTIME_CONFIG);
      })
      .finally(() => {
        if (!cancelled) setLoaded(true);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <RuntimeConfigContext.Provider value={{ config, loaded }}>
      {children}
    </RuntimeConfigContext.Provider>
  );
}
