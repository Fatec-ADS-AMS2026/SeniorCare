import '@testing-library/jest-dom';
import { expect } from 'vitest';
import { toHaveNoViolations } from 'jest-axe';

expect.extend(toHaveNoViolations);

// Node 22+ expõe um `localStorage` global experimental que exige
// `--localstorage-file` pra funcionar; sem essa flag ele existe (não é
// `undefined` em `typeof`) mas toda chamada (`.getItem`/`.setItem`) quebra
// com TypeError. Isso sobrepõe o localStorage funcional que o jsdom
// forneceria, então qualquer componente que use `localStorage` direto (ex.:
// ThemeContext.tsx, mesmo padrão já usado por care-web/stock-web) quebra em
// teste — não é um bug do componente. Poliflha aqui, uma vez, pra todos os
// testes.
class MemoryStorage implements Storage {
  private store = new Map<string, string>();

  get length(): number {
    return this.store.size;
  }

  clear(): void {
    this.store.clear();
  }

  getItem(key: string): string | null {
    return this.store.has(key) ? this.store.get(key)! : null;
  }

  key(index: number): string | null {
    return Array.from(this.store.keys())[index] ?? null;
  }

  removeItem(key: string): void {
    this.store.delete(key);
  }

  setItem(key: string, value: string): void {
    this.store.set(key, String(value));
  }
}

function needsPolyfill(): boolean {
  try {
    window.localStorage.setItem('__storage_probe__', '1');
    window.localStorage.removeItem('__storage_probe__');
    return false;
  } catch {
    return true;
  }
}

if (needsPolyfill()) {
  const memoryStorage = new MemoryStorage();
  Object.defineProperty(globalThis, 'localStorage', { value: memoryStorage, configurable: true, writable: true });
  Object.defineProperty(window, 'localStorage', { value: memoryStorage, configurable: true, writable: true });
}
