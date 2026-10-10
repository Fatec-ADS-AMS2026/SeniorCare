import { createRoutes } from '@/utils/routesUtils';
import LoginPage from './pages/LoginPage';
import MfaChallengePage from './pages/MfaChallengePage';
import MfaEnrollPage from './pages/MfaEnrollPage';
import ActivateAccountPage from './pages/ActivateAccountPage';
import RecoverAccountPage from './pages/RecoverAccountPage';
import ResetPasswordPage from './pages/ResetPasswordPage';

// Rotas de auth ficam fora do RequireAuth porque são pré-sessão.
export const authRoutes = createRoutes({
  LOGIN: {
    path: '/login',
    displayName: 'Login',
    element: <LoginPage />,
  },
  ACTIVATE_ACCOUNT: {
    path: '/ativar-conta',
    displayName: 'Ativar conta',
    element: <ActivateAccountPage />,
  },
  RECOVER_ACCOUNT: {
    path: '/recuperar-senha',
    displayName: 'Recuperar acesso',
    element: <RecoverAccountPage />,
  },
  RESET_PASSWORD: {
    path: '/redefinir-senha',
    displayName: 'Redefinir senha',
    element: <ResetPasswordPage />,
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
