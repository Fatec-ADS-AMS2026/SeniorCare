import { FormEvent, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import authService from '../services/authService';

export default function ResetPasswordPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [email, setEmail] = useState(searchParams.get('email') ?? '');
  const [token, setToken] = useState(searchParams.get('token') ?? '');
  const [newPassword, setNewPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isDone, setIsDone] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setError('');
    setIsSubmitting(true);
    const result = await authService.resetPassword({ email, token, newPassword });
    setIsSubmitting(false);

    if (!result.success) {
      setError(result.message || 'Não foi possível redefinir a senha.');
      return;
    }

    setIsDone(true);
  };

  return (
    <main className='flex min-h-screen items-center justify-center px-8 py-16'>
      <section className='w-full max-w-md rounded-lg bg-neutralWhite p-8 shadow-md'>
        <h1 className='mb-6 text-3xl font-bold text-secondary'>Redefinir senha</h1>
        {isDone ? (
          <>
            <p className='mb-6 text-textSecondary'>Senha redefinida com sucesso.</p>
            <button className='h-12 w-full rounded bg-primary font-semibold text-neutralWhite' onClick={() => navigate('/login')}>
              Ir para o login
            </button>
          </>
        ) : (
          <form className='flex flex-col gap-4' onSubmit={handleSubmit}>
            <label className='flex flex-col gap-1 font-semibold text-textSecondary'>
              E-mail
              <input className='h-12 rounded border border-textSecondary px-3 font-normal' type='email' value={email} onChange={(event) => setEmail(event.target.value)} required />
            </label>
            <label className='flex flex-col gap-1 font-semibold text-textSecondary'>
              Token de recuperação
              <input className='h-12 rounded border border-textSecondary px-3 font-normal' value={token} onChange={(event) => setToken(event.target.value)} required />
            </label>
            <label className='flex flex-col gap-1 font-semibold text-textSecondary'>
              Nova senha
              <input className='h-12 rounded border border-textSecondary px-3 font-normal' type='password' value={newPassword} onChange={(event) => setNewPassword(event.target.value)} required />
            </label>
            {error && <p className='text-danger' role='alert'>{error}</p>}
            <button className='h-12 rounded bg-primary font-semibold text-neutralWhite disabled:opacity-60' type='submit' disabled={isSubmitting}>
              {isSubmitting ? 'Redefinindo...' : 'Redefinir senha'}
            </button>
          </form>
        )}
      </section>
    </main>
  );
}
