import { describe, it, expect, vi } from 'vitest';
import { isSafeReturnPath, resolveReturnPath } from './returnPath';

describe('isSafeReturnPath', () => {
  it.each(['/', '/care', '/care/residents/1', '/stock', '/stock/products'])(
    'accepts known destination %s',
    (value) => {
      expect(isSafeReturnPath(value)).toBe(true);
    }
  );

  it.each([
    ['//evil.com', 'protocol-relative origin'],
    ['http://evil.com', 'absolute URL (http)'],
    ['javascript:alert(1)', 'javascript scheme'],
    ['\\\\evil.com', 'backslash'],
    ['/admin', 'unknown prefix (not in the allowlist)'],
    ['', 'empty string'],
    ['relative/without/leading/slash', 'no leading slash'],
  ])('rejects %s (%s)', (value) => {
    expect(isSafeReturnPath(value)).toBe(false);
  });
});

describe('resolveReturnPath', () => {
  it('returns null for null/undefined/empty input (no forced navigation)', () => {
    expect(resolveReturnPath(null)).toBeNull();
    expect(resolveReturnPath(undefined)).toBeNull();
    expect(resolveReturnPath('')).toBeNull();
  });

  it('returns the validated path when safe', () => {
    expect(resolveReturnPath('/care/residents')).toBe('/care/residents');
  });

  it('returns null and logs a warning for a rejected destination', () => {
    const warnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});

    expect(resolveReturnPath('http://evil.com')).toBeNull();
    expect(warnSpy).toHaveBeenCalledOnce();

    warnSpy.mockRestore();
  });
});
