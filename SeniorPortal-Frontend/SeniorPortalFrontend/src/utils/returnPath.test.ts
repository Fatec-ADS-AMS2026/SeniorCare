import { describe, it, expect, vi } from 'vitest';
import { isSafeReturnPath, resolveReturnPath, RETURN_PATH_FALLBACK } from './returnPath';

// §4.5/§4.6 — contrato travado em
// docs/architecture/senior-portal-contracts.md §3.
describe('isSafeReturnPath', () => {
  it.each(['/', '/care', '/care/', '/care/residents/1', '/stock', '/stock/products'])(
    'accepts known destination %s',
    (value) => {
      expect(isSafeReturnPath(value)).toBe(true);
    }
  );

  it.each([
    ['//evil.com', 'protocol-relative origin'],
    ['http://evil.com', 'absolute URL (http)'],
    ['https://evil.com', 'absolute URL (https)'],
    ['javascript:alert(1)', 'javascript scheme'],
    ['\\\\evil.com', 'backslash'],
    ['/care\\..\\..', 'embedded backslash'],
    ['/admin', 'unknown prefix (not yet in the allowlist)'],
    ['/carefully', 'prefix look-alike without separator'],
    ['', 'empty string'],
    ['relative/without/leading/slash', 'no leading slash'],
  ])('rejects %s (%s)', (value) => {
    expect(isSafeReturnPath(value)).toBe(false);
  });
});

describe('resolveReturnPath', () => {
  it('returns the fallback for null/undefined/empty input', () => {
    expect(resolveReturnPath(null)).toBe(RETURN_PATH_FALLBACK);
    expect(resolveReturnPath(undefined)).toBe(RETURN_PATH_FALLBACK);
    expect(resolveReturnPath('')).toBe(RETURN_PATH_FALLBACK);
  });

  it('returns the validated path when safe', () => {
    expect(resolveReturnPath('/care/residents')).toBe('/care/residents');
  });

  it('falls back and logs a warning for a rejected destination', () => {
    const warnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});

    expect(resolveReturnPath('https://evil.com')).toBe(RETURN_PATH_FALLBACK);
    expect(warnSpy).toHaveBeenCalledOnce();

    warnSpy.mockRestore();
  });
});
