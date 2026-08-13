import { Envelope, Eye, EyeSlash, Lock } from '@phosphor-icons/react';
import { FormEvent, useEffect, useState } from 'react';
import { useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import authService from '../../services/authService';
import useAuth from '@/hooks/useAuth';
import useAppRoutes from '@/hooks/useAppRoutes';
import { resolveReturnPath } from '@/utils/returnPath';

// §9.2 — mesmo sinal de ativação usado por AppRoutes.tsx pro `basename`: só
// quando o build sai com VITE_BASE_PATH != `/` (roteamento por caminho
// realmente ativado, §8.2/§9.7) este login "frio" (sem destino interno
// validado) deixa de ser a entrada legítima e passa a redirecionar pro login
// do Senior Portal, agora a porta de entrada canônica.
const isPathBasedRouting = Boolean(
  import.meta.env.VITE_BASE_PATH && import.meta.env.VITE_BASE_PATH !== '/'
);

// Caminho absoluto a partir da ORIGEM (não desta app) — window.location.assign
// não passa pelo basename do React Router (isso só se aplica a navigate()/Link
// deste próprio router), então `/login` aqui alcança o login do Senior Portal
// (a app raiz sob roteamento por caminho), nunca o `/login` deste app
// (que ficaria em `/care/login`).
const PORTAL_LOGIN_PATH = '/login';

export default function LoginForm() {
  const [showPassword, setShowPassword] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const routes = useAppRoutes();

  // §9.2 — "retorno interno validado": `from` (sintetizado por RequireAuth,
  // nunca alcançável por URL) ou um `returnTo` já validado pelo allowlist
  // (§4.5/§6.4). Só quando NENHUM dos dois existe é que esta chegada em
  // `/login` é "fria" — bookmark antigo, link legado ou a landing page
  // legada (também redirecionada, ver LandingPage) — e deve mandar pro
  // portal em vez de renderizar o formulário legado.
  const from = (location.state as { from?: { pathname?: string } } | null)?.from
    ?.pathname;
  const crossAppReturnTo = resolveReturnPath(searchParams.get('returnTo'));
  const isLegacyColdArrival = isPathBasedRouting && !from && !crossAppReturnTo;

  useEffect(() => {
    if (isLegacyColdArrival) {
      window.location.assign(PORTAL_LOGIN_PATH);
    }
  }, [isLegacyColdArrival]);

  const togglePasswordVisibility = () => {
    setShowPassword((prevState) => !prevState);
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);

    const res = await authService.login({ email, password });
    setIsSubmitting(false);

    if (!res.success || !res.data) {
      setError(res.message || 'Não foi possível entrar.');
      return;
    }

    const { status, identity, challengeToken } = res.data;

    if (status === 'ok' && identity) {
      login(identity);
      // §6.4 — prioridade: destino interno preservado por RequireAuth (mais
      // específico, nunca alcançável por URL) > returnTo validado da query
      // string (entrada cruzada — portal ou link direto, §4.5/§6.4) >
      // destino padrão do app.
      //
      // from usa navigate() (SPA, rápido) porque é sempre uma rota deste
      // próprio roteador — RequireAuth só sintetiza esse state a partir de
      // uma navegação que já aconteceu dentro deste app. crossAppReturnTo
      // usa window.location.assign() (navegação de página inteira) porque
      // portal/care/stock são três bundles/roteadores React Router
      // SEPARADOS (design.md decisão 1, sem module federation) — navigate()
      // nunca resolve uma rota fora do router deste app, mesmo pra `/`
      // (raiz do portal) ou `/stock/...`, então usá-lo aqui resultaria em
      // tela em branco ou na LANDING deste próprio app por engano.
      if (from) {
        navigate(from, { replace: true });
        return;
      }

      if (crossAppReturnTo) {
        window.location.assign(crossAppReturnTo);
        return;
      }

      navigate(routes.ADMIN_OVERVIEW.path, { replace: true });
      return;
    }

    // §6.4 — o returnTo pendente precisa sobreviver à etapa de MFA (mesmo
    // racional do Senior Portal, §4.5): sem repassar a query string aqui, o
    // destino cruzado se perderia depois de confirmar o desafio.
    const pendingQuery = searchParams.toString();

    if (status === 'mfa_required' && challengeToken) {
      navigate(
        { pathname: routes.MFA_CHALLENGE.path, search: pendingQuery ? `?${pendingQuery}` : '' },
        { state: { challengeToken } }
      );
      return;
    }

    if (status === 'mfa_enrollment_required' && challengeToken) {
      navigate(
        { pathname: routes.MFA_ENROLL.path, search: pendingQuery ? `?${pendingQuery}` : '' },
        { state: { challengeToken } }
      );
      return;
    }

    setError('Resposta inesperada do servidor.');
  };

  if (isLegacyColdArrival) {
    return (
      <p className='text-textSecondary text-center' role='status'>
        Redirecionando para o portal...
      </p>
    );
  }

  return (
    <form
      className='flex flex-col justify-center items-center'
      onSubmit={handleSubmit}
    >
      <h3 className='font-bold text-4xl md:text-5xl mb-4 text-secondary'>
        Login
      </h3>
      {error && (
        <p className='mb-4 w-full max-w-md text-danger text-sm text-center'>
          {error}
        </p>
      )}
      <div className='flex flex-col w-full max-w-md mb-4'>
        <label
          htmlFor='email'
          className='text-lg font-semibold mb-2 tracking-wide text-textSecondary'
        >
          Email
        </label>
        <div className='flex items-center border border-textSecondary rounded'>
          <Envelope size={24} className='mx-2 text-textSecondary shrink-0' />
          <input
            id='email'
            type='email'
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            className='flex-1 h-12 px-4 focus:outline-none'
            placeholder='Digite seu email'
          />
        </div>
      </div>
      <div className='flex flex-col w-full max-w-md mb-2'>
        <label
          htmlFor='password'
          className='text-lg font-semibold mb-2 tracking-wide text-textSecondary'
        >
          Senha
        </label>
        <div className='flex items-center border border-textSecondary rounded'>
          <Lock size={24} className='mx-2 text-textSecondary shrink-0' />
          <input
            id='password'
            type={showPassword ? 'text' : 'password'}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            className='flex-1 h-12 px-4 focus:outline-none w-full'
            placeholder='Digite sua senha'
          />
          <button
            type='button'
            onClick={togglePasswordVisibility}
            className='text-textSecondary mx-2 shrink-0'
            aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'}
          >
            {showPassword ? <EyeSlash size={24} /> : <Eye size={24} />}
          </button>
        </div>
        <button
          type='button'
          onClick={() => navigate(routes.RECOVER_ACCOUNT.path)}
          className='mt-2 cursor-pointer hover:text-secondary transition-colors text-right text-textSecondary self-end'
        >
          Esqueceu sua senha?
        </button>
      </div>
      <button
        type='submit'
        disabled={isSubmitting}
        className='bg-primary h-12 w-full max-w-md rounded text-neutralWhite font-semibold hover:bg-hoverButton transition-colors text-lg mt-5 disabled:opacity-60'
      >
        {isSubmitting ? 'Entrando...' : 'Entrar'}
      </button>
    </form>
  );
}
