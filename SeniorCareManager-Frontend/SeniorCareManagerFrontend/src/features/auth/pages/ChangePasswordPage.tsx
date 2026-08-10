import { FormEvent, useState } from 'react';
import { TextInput } from '@/components/FormControls';
import Button from '@/components/Button';
import authService from '../services/authService';
import useAuth from '@/hooks/useAuth';

interface ChangePasswordFormData {
  email: string;
  currentPassword: string;
  newPassword: string;
}

// AuthController.ChangePassword é [AllowAnonymous] de propósito (§7) — se
// autoidentifica por e-mail+senha atual no corpo, não pela sessão. Pré-preenche o
// e-mail se já houver sessão, mas funciona igual sem ela.
export default function ChangePasswordPage() {
  const { identity } = useAuth();

  const [email, setEmail] = useState(identity?.email ?? '');
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [isDone, setIsDone] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);
    const res = await authService.changePassword({
      email,
      currentPassword,
      newPassword,
    });
    setIsSubmitting(false);

    if (!res.success) {
      setError(res.message || 'Não foi possível alterar a senha.');
      return;
    }
    setIsDone(true);
    setCurrentPassword('');
    setNewPassword('');
  };

  return (
    <div className='flex flex-col items-center justify-center h-full py-16'>
      <form
        onSubmit={handleSubmit}
        className='w-full max-w-md bg-neutralWhite p-8 rounded-lg shadow-md flex flex-col gap-4'
      >
        <h1 className='text-2xl font-bold text-secondary'>Alterar senha</h1>
        <p className='text-textSecondary text-sm'>
          Alterar a senha encerra todas as suas sessões ativas — você precisará
          entrar novamente.
        </p>
        {isDone && (
          <p className='text-success text-sm'>Senha alterada com sucesso.</p>
        )}
        <TextInput<ChangePasswordFormData>
          label='E-mail'
          name='email'
          type='email'
          value={email}
          onChange={(_, value) => setEmail(value)}
          required
        />
        <TextInput<ChangePasswordFormData>
          label='Senha atual'
          name='currentPassword'
          type='password'
          value={currentPassword}
          onChange={(_, value) => setCurrentPassword(value)}
          required
        />
        <TextInput<ChangePasswordFormData>
          label='Nova senha'
          name='newPassword'
          type='password'
          value={newPassword}
          onChange={(_, value) => setNewPassword(value)}
          required
        />
        {error && <p className='text-danger text-sm'>{error}</p>}
        <Button
          type='submit'
          label={isSubmitting ? 'Alterando...' : 'Alterar senha'}
          disabled={isSubmitting}
        />
      </form>
    </div>
  );
}
