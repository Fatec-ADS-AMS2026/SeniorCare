import { FormEvent, useState } from 'react';
import { TextInput } from '@/components/FormControls';
import Button from '@/components/Button';
import authService from '../services/authService';

interface RecoverFormData {
  email: string;
}

// §7: Recover sempre devolve a mesma mensagem neutra (200), exista ou não conta
// elegível pra aquele e-mail — mensagem de sucesso é sempre a mesma, sem distinguir.
export default function RecoverAccountPage() {
  const [email, setEmail] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState('');

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    const res = await authService.recover({ email });
    setIsSubmitting(false);
    setMessage(
      res.data?.message ||
        'Se o e-mail informado tiver uma conta elegível, instruções de recuperação foram enviadas.'
    );
  };

  return (
    <div className='flex flex-col items-center justify-center h-full py-16'>
      <form
        onSubmit={handleSubmit}
        className='w-full max-w-md bg-neutralWhite p-8 rounded-lg shadow-md flex flex-col gap-4'
      >
        <h1 className='text-2xl font-bold text-secondary'>Recuperar acesso</h1>
        {message ? (
          <p className='text-textSecondary text-sm'>{message}</p>
        ) : (
          <>
            <TextInput<RecoverFormData>
              label='E-mail'
              name='email'
              type='email'
              value={email}
              onChange={(_, value) => setEmail(value)}
              required
            />
            <Button
              type='submit'
              label={isSubmitting ? 'Enviando...' : 'Enviar instruções'}
              disabled={isSubmitting}
            />
          </>
        )}
      </form>
    </div>
  );
}
