import { configureAxe } from 'jest-axe';

// jsdom não renderiza estilo computado de verdade — o checker de contraste do
// axe-core não é confiável nesse ambiente (limitação documentada do
// axe-core/jsdom). Contraste é conferido à mão e documentado em tasks.md (§11).
export const axe = configureAxe({
  rules: {
    'color-contrast': { enabled: false },
  },
});
