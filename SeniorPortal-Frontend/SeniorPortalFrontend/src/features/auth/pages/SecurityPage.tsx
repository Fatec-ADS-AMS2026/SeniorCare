import { FormEvent, useState } from 'react';
import authService from '../services/authService';

// §5.4 — /security é rota do próprio Senior Portal
// (senior-portal-contracts.md §4). Escopo desta seção: regeneração de
// códigos de recuperação MFA (único contrato de "segurança da conta" já
// pronto no backend) — troca de senha, ativação e recuperação continuam em
// care-web/stock-web até uma tarefa própria migrar esses fluxos (mesma
// decisão de §4.2 para o authService).
export default function SecurityPage() {
  const [currentPassword, setCurrentPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [recoveryCodes, setRecoveryCodes] = useState<string[] | null>(null);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);
    const res = await authService.regenerateRecoveryCodes({ currentPassword });
    setIsSubmitting(false);

    if (!res.success || !res.data) {
      setError(res.message || 'Não foi possível gerar novos códigos.');
      return;
    }

    setRecoveryCodes(res.data.recoveryCodes);
    setCurrentPassword('');
  };

  return (
    <div className='max-w-2xl mx-auto p-4 md:p-8 w-full'>
      <h1 className='text-2xl font-bold text-secondary mb-6'>Segurança da conta</h1>

      <section className='flex flex-col gap-4'>
        <h2 className='text-lg font-semibold text-textPrimary'>
          Códigos de recuperação da verificação em duas etapas
        </h2>
        <p className='text-textSecondary text-sm'>
          Gerar novos códigos invalida os códigos anteriores. Confirme sua senha atual para continuar.
        </p>

        {recoveryCodes ? (
          <ul className='font-mono text-sm bg-neutralLighter p-4 rounded grid grid-cols-2 gap-2'>
            {recoveryCodes.map((code) => (
              <li key={code}>{code}</li>
            ))}
          </ul>
        ) : (
          <form onSubmit={handleSubmit} className='flex flex-col gap-3 max-w-sm'>
            {error && (
              <p className='text-danger text-sm' role='alert'>
                {error}
              </p>
            )}
            <div className='flex flex-col'>
              <label htmlFor='currentPassword' className='text-sm font-semibold mb-1 text-textSecondary'>
                Senha atual
              </label>
              <input
                id='currentPassword'
                type='password'
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
                required
                className='h-12 px-4 border border-textSecondary rounded focus:outline-none'
              />
            </div>
            <button
              type='submit'
              disabled={isSubmitting}
              className='bg-primary h-12 rounded text-neutralWhite font-semibold hover:bg-hoverButton transition-colors disabled:opacity-60'
            >
              {isSubmitting ? 'Gerando...' : 'Gerar novos códigos'}
            </button>
          </form>
        )}
      </section>
    </div>
  );
}
