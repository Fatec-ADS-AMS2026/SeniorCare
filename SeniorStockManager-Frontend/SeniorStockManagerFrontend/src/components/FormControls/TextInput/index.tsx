import { InputHTMLAttributes, useId } from 'react';
import { FormField } from '../FormField';
import { BaseFieldProps } from '../types';

interface TextInputProps<T>
  extends BaseFieldProps,
    Omit<InputHTMLAttributes<HTMLInputElement>, 'onChange' | 'name'> {
  type?: 'text' | 'email' | 'number' | 'password';
  name: keyof T;
  onChange: (attribute: keyof T, value: string) => void;
}

/**
 * Um input comum que suporta texto, email e números
 */
export default function TextInput<T>({
  label,
  error,
  required,
  type = 'text',
  value,
  icon,
  name,
  onChange,
  ...props
}: TextInputProps<T>) {
  const id = useId();
  const errorId = error ? `${id}-error` : undefined;

  return (
    <FormField id={id} label={label} error={error} required={required}>
      {icon && (
        <span className='absolute top-2.5 left-2 text-xl text-textSecondary shrink-0'>
          {icon}
        </span>
      )}
      <input
        id={id}
        type={type}
        value={value}
        name={String(name)}
        onChange={(e) => onChange(e.target.name as keyof T, e.target.value)}
        aria-invalid={!!error}
        aria-describedby={errorId}
        className={`w-full py-2 text-sm text-textPrimary rounded border focus:outline-none focus:ring-2 focus:ring-primary ${
          error ? 'border-danger' : 'border-neutralDark'
        } ${icon ? 'pr-2 pl-8' : 'px-2'}`}
        {...props}
      />
    </FormField>
  );
}
