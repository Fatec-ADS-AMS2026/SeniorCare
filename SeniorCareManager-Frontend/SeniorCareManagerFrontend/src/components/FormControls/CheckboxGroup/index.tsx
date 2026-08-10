import { useId } from 'react';
import { FormField } from '../FormField';
import { BaseFieldProps } from '../types';
import Checkbox from '../Checkbox';

interface CheckboxOption<T> {
  value: unknown;
  label: string;
  disabled?: boolean;
  name: keyof T;
}

interface CheckboxGroupProps<T> extends BaseFieldProps {
  options: CheckboxOption<T>[];
  values: unknown[];
  onChange: (attribute: keyof T, values: unknown[]) => void;
}

/**
 * Um grupo de checkboxes para múltiplas seleções
 */
export default function CheckboxGroup<T>({
  label,
  error,
  required,
  options,
  values,
  onChange,
}: CheckboxGroupProps<T>) {
  const id = useId();
  const handleChange = (
    attribute: keyof T,
    optionValue: unknown,
    checked: boolean
  ) => {
    if (checked) {
      onChange(attribute, [...values, optionValue]);
    } else {
      onChange(
        attribute,
        values.filter((value) => value !== optionValue)
      );
    }
  };

  return (
    <FormField id={id} label={label} error={error} required={required} groupLabel>
      <div className={`space-y-2 ${label && 'px-2'}`}>
        {options.map((option) => (
          <Checkbox
            key={`Option_${option.value}`}
            label={option.label}
            name={option.name}
            checked={values.includes(option.value)}
            onChange={(name, checked) =>
              handleChange(name, option.value, checked)
            }
            disabled={option.disabled}
          />
        ))}
      </div>
    </FormField>
  );
}
