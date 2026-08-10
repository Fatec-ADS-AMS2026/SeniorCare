import { BaseFieldProps } from '../types';
import { FormField } from '../FormField';
import { SelectHTMLAttributes, useId } from 'react';

interface SelectInputProps<T>
  extends BaseFieldProps,
    Omit<SelectHTMLAttributes<HTMLSelectElement>, 'onChange' | 'name'> {
  options: { label: string; value: string | number }[];
  onChange: (attribute: keyof T, value: string) => void;
  name: keyof T;
}

export default function SelectInput<T>({
  label,
  error,
  required,
  value,
  options,
  onChange,
  icon,
  name,
  ...props
}: SelectInputProps<T>) {
  const id = useId();
  const errorId = error ? `${id}-error` : undefined;

  return (
    <FormField id={id} label={label} error={error} required={required}>
      {icon && (
        <span className='absolute top-2.5 left-2 text-xl text-textSecondary shrink-0'>
          {icon}
        </span>
      )}
      <select
        id={id}
        value={value}
        name={String(name)}
        onChange={(e) => onChange(e.target.name as keyof T, e.target.value)}
        aria-invalid={!!error}
        aria-describedby={errorId}
        className={`w-full py-2 text-sm text-textPrimary rounded border focus:outline-none focus:ring-2 focus:ring-primary ${
          error ? 'border-danger' : 'border-neutralDark'
        } ${icon ? 'pr-2 pl-7' : 'px-1'}`}
        {...props}
      >
        {/* Placeholder */}
        <option value='' disabled>
          Selecione um...
        </option>
        {/* Lista de opções */}
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </FormField>
  );
}
