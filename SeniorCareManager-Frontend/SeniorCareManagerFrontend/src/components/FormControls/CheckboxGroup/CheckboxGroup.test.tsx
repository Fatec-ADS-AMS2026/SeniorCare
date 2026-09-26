import { useState } from 'react';
import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import CheckboxGroup from './index';

function PermissionSelector() {
  const [values, setValues] = useState<unknown[]>(['read']);
  return <CheckboxGroup<{ permissions: unknown[] }>
    label='Permissões'
    required
    values={values}
    options={[
      { name: 'permissions', value: 'read', label: 'Ler' },
      { name: 'permissions', value: 'write', label: 'Escrever' },
    ]}
    onChange={(_, next) => setValues(next)}
  />;
}

describe('CheckboxGroup', () => {
  it('adds and removes selected values', async () => {
    const user = userEvent.setup();
    render(<PermissionSelector />);

    await user.click(screen.getByLabelText('Escrever'));
    expect(screen.getByLabelText('Escrever')).toBeChecked();
    await user.click(screen.getByLabelText('Ler'));
    expect(screen.getByLabelText('Ler')).not.toBeChecked();
  });
});
