import { createRoutes } from '@/utils/routesUtils';
import LoginPage from './pages/LoginPage';
import MfaChallengePage from './pages/MfaChallengePage';
import MfaEnrollPage from './pages/MfaEnrollPage';
import RecoverAccountPage from './pages/RecoverAccountPage';
import ResetPasswordPage from './pages/ResetPasswordPage';
import ActivateAccountPage from './pages/ActivateAccountPage';
import ChangePasswordPage from './pages/ChangePasswordPage';

// Todas as rotas de auth ficam sob HeaderFooterLayout (público, fora do RequireAuth
// — §10.4): login/MFA/recuperação/ativação são inerentemente pré-sessão, e
// change-password (AuthController.ChangePassword) é [AllowAnonymous] de propósito,
// se autoidentificando por e-mail+senha atual em vez da sessão.
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
  RECOVER_ACCOUNT: {
    path: '/recuperar-senha',
    displayName: 'Recuperar senha',
    element: <RecoverAccountPage />,
  },
  RESET_PASSWORD: {
    path: '/redefinir-senha',
    displayName: 'Redefinir senha',
    element: <ResetPasswordPage />,
  },
  ACTIVATE_ACCOUNT: {
    path: '/ativar-conta',
    displayName: 'Ativar conta',
    element: <ActivateAccountPage />,
  },
  CHANGE_PASSWORD: {
    path: '/alterar-senha',
    displayName: 'Alterar senha',
    element: <ChangePasswordPage />,
  },
});
