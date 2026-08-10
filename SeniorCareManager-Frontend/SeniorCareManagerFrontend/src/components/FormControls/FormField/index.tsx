import { XCircle } from '@phosphor-icons/react';

interface FormFieldProps {
  id?: string;
  label?: string;
  error?: string;
  required?: boolean;
  children: React.ReactNode;
  // §11: um grupo de checkboxes não tem "o" controle único que um
  // <label htmlFor> possa apontar — usa <fieldset>/<legend> em vez de <label>.
  groupLabel?: boolean;
}

/**
 * Este componente é utilizado para fornecer elementos básicos aos campos de um
 * formulário, como label e espaço para mensagem de erro.
 */
export function FormField({
  id,
  label,
  error,
  required,
  children,
  groupLabel,
}: FormFieldProps) {
  const errorId = id ? `${id}-error` : undefined;

  const labelContent = label ? (
    groupLabel ? (
      <legend className='block text-textPrimary text-sm mb-1 break-all'>
        {label}
        {required && '*'}
      </legend>
    ) : (
      <label
        htmlFor={id}
        className='block text-textPrimary text-sm mb-1 break-all'
      >
        {label}
        {required && '*'}
      </label>
    )
  ) : null;

  const body = (
    <>
      {labelContent}
      <div className='relative'>{children}</div>
      {error && (
        <span
          id={errorId}
          className='text-danger text-xs flex gap-1 items-center'
        >
          <XCircle />
          {error}
        </span>
      )}
    </>
  );

  if (groupLabel) {
    return (
      <fieldset
        className='border-0 p-0 m-0 min-w-0'
        aria-describedby={errorId}
      >
        {body}
      </fieldset>
    );
  }

  return <div>{body}</div>;
}
