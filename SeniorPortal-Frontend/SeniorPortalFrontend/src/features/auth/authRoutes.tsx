import { createRoutes } from '@/utils/routesUtils';
import LoginPage from './pages/LoginPage';
import MfaChallengePage from './pages/MfaChallengePage';
import MfaEnrollPage from './pages/MfaEnrollPage';

// Rotas de auth ficam fora do RequireAuth — login/MFA são inerentemente
// pré-sessão. Recuperação/ativação/troca de senha continuam vivendo em
// care-web/stock-web por ora (não migradas nesta seção).
export const authRoutes = createRoutes({
  LOGIN: {
    path: '/login',
    displayName: 'Login',
    element: <LoginPage />,
  },
  MFA_CHALLENGE: {
    path: '/login/mfa',
    displayName: 'Verificação em duas etapas',
    element: <MfaChallengePage />,
  },
  MFA_ENROLL: {
    path: '/mfa/enroll',
    displayName: 'Cadastro de verificação em duas etapas',
    element: <MfaEnrollPage />,
  },
});
