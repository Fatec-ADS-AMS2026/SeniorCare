import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { RuntimeConfigProvider } from './RuntimeConfigContext';
import useRuntimeConfig from '@/hooks/useRuntimeConfig';

function Consumer() {
  const { config, loaded } = useRuntimeConfig();
  return (
    <div>
      <span data-testid='loaded'>{loaded ? 'yes' : 'no'}</span>
      <span data-testid='publicName'>{config.publicName}</span>
    </div>
  );
}

describe('RuntimeConfigProvider', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('loads publicName from public-config.json', async () => {
    vi.mocked(fetch).mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve({ publicName: 'ILPI Jardim das Flores' }),
    } as Response);

    render(
      <RuntimeConfigProvider>
        <Consumer />
      </RuntimeConfigProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('loaded').textContent).toBe('yes');
    });
    expect(screen.getByTestId('publicName').textContent).toBe('ILPI Jardim das Flores');
  });

  it('falls back to "SeniorCare" when the file is missing (network/404)', async () => {
    vi.mocked(fetch).mockResolvedValueOnce({ ok: false, status: 404 } as Response);

    render(
      <RuntimeConfigProvider>
        <Consumer />
      </RuntimeConfigProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('loaded').textContent).toBe('yes');
    });
    expect(screen.getByTestId('publicName').textContent).toBe('SeniorCare');
  });

  it('falls back to "SeniorCare" when the file has an invalid publicName', async () => {
    vi.mocked(fetch).mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve({ publicName: '   ' }),
    } as Response);

    render(
      <RuntimeConfigProvider>
        <Consumer />
      </RuntimeConfigProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('loaded').textContent).toBe('yes');
    });
    expect(screen.getByTestId('publicName').textContent).toBe('SeniorCare');
  });

  it('falls back to "SeniorCare" when the fetch rejects (network unavailable)', async () => {
    vi.mocked(fetch).mockRejectedValueOnce(new Error('network down'));

    render(
      <RuntimeConfigProvider>
        <Consumer />
      </RuntimeConfigProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('loaded').textContent).toBe('yes');
    });
    expect(screen.getByTestId('publicName').textContent).toBe('SeniorCare');
  });
});
