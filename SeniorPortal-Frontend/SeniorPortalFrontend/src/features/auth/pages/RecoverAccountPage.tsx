import { FormEvent, useState } from 'react';
import authService from '../services/authService';

export default function RecoverAccountPage() {
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setIsSubmitting(true);
    const result = await authService.recover({ email });
    setIsSubmitting(false);
    setMessage(result.data?.message || 'Se o e-mail informado tiver uma conta elegível, instruções de recuperação foram enviadas.');
  };

  return (
    <main className='flex min-h-screen items-center justify-center px-8 py-16'>
      <section className='w-full max-w-md rounded-lg bg-neutralWhite p-8 shadow-md'>
        <h1 className='mb-6 text-3xl font-bold text-secondary'>Recuperar acesso</h1>
        {message ? (
          <p className='text-textSecondary'>{message}</p>
        ) : (
          <form className='flex flex-col gap-4' onSubmit={handleSubmit}>
            <label className='flex flex-col gap-1 font-semibold text-textSecondary'>
              E-mail
              <input className='h-12 rounded border border-textSecondary px-3 font-normal' type='email' value={email} onChange={(event) => setEmail(event.target.value)} required />
            </label>
            <button className='h-12 rounded bg-primary font-semibold text-neutralWhite disabled:opacity-60' type='submit' disabled={isSubmitting}>
              {isSubmitting ? 'Enviando...' : 'Enviar instruções'}
            </button>
          </form>
        )}
      </section>
    </main>
  );
}
