import { FormEvent, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { TextInput } from '@/components/FormControls';
import Button from '@/components/Button';
import authService from '../services/authService';
import useAppRoutes from '@/hooks/useAppRoutes';

interface ActivateAccountFormData {
  email: string;
  token: string;
  newPassword: string;
}

// Token/e-mail vêm do link de ativação repassado pelo admin (§10.6 — não existe
// serviço de e-mail no projeto, o admin copia o token da tela de criação de conta).
export default function ActivateAccountPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const routes = useAppRoutes();

  const [email, setEmail] = useState(searchParams.get('email') ?? '');
  const [token, setToken] = useState(searchParams.get('token') ?? '');
  const [newPassword, setNewPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [isDone, setIsDone] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);
    const res = await authService.activate({ email, token, newPassword });
    setIsSubmitting(false);

    if (!res.success) {
      setError(res.message || 'Não foi possível ativar a conta.');
      return;
    }
    setIsDone(true);
  };

  return (
    <div className='flex flex-col items-center justify-center h-full py-16'>
      <div className='w-full max-w-md bg-neutralWhite p-8 rounded-lg shadow-md flex flex-col gap-4'>
        <h1 className='text-2xl font-bold text-secondary'>Ativar conta</h1>
        {isDone ? (
          <>
            <p className='text-textSecondary text-sm'>
              Conta ativada com sucesso.
            </p>
            <Button
              label='Ir para o login'
              onClick={() => navigate(routes.LOGIN.path)}
            />
          </>
        ) : (
          <form onSubmit={handleSubmit} className='flex flex-col gap-4'>
            <TextInput<ActivateAccountFormData>
              label='E-mail'
              name='email'
              type='email'
              value={email}
              onChange={(_, value) => setEmail(value)}
              required
            />
            <TextInput<ActivateAccountFormData>
              label='Token de ativação'
              name='token'
              value={token}
              onChange={(_, value) => setToken(value)}
              required
            />
            <TextInput<ActivateAccountFormData>
              label='Defina sua senha'
              name='newPassword'
              type='password'
              value={newPassword}
              onChange={(_, value) => setNewPassword(value)}
              required
            />
            {error && <p className='text-danger text-sm'>{error}</p>}
            <Button
              type='submit'
              label={isSubmitting ? 'Ativando...' : 'Ativar conta'}
              disabled={isSubmitting}
            />
          </form>
        )}
      </div>
    </div>
  );
}
