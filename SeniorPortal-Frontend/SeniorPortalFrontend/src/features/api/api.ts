import axios, { AxiosRequestConfig, AxiosResponse } from 'axios';

// Caminho de mesma origem — em dev, o proxy do Vite (vite.config.ts) encaminha
// /api para VITE_API_PROXY_TARGET; em produção, o nginx/Caddy encaminha /api
// para o backend na mesma origem. Nunca aponta pra um host fixo aqui.
//
// withCredentials: a sessão é um único cookie HttpOnly (sem bearer separado,
// sem token em memória) — sem isso o navegador não anexa o cookie de sessão
// às requisições. Nunca existe Authorization: Bearer nem token/cookie
// "auth_token" legível por JS neste projeto (mesmo padrão já usado por
// care-web/stock-web, confirmado por investigação direta antes de §4.2 —
// design.md descrevia um modelo de token curto em memória que nunca chegou a
// ser implementado no backend; o cookie único é estritamente mais forte,
// já que nenhum token passa pelo JavaScript em momento algum).
const axiosInstance = axios.create({
  baseURL: '/api/v1/',
  withCredentials: true,
});

// Callback registrado pelo AuthProvider — 401 fora do login/restauração limpa a
// sessão em memória e redireciona pro /login. Vive aqui (fora do React) só como
// ponto de registro; a lógica de fato mora no AuthProvider.
let unauthorizedHandler: (() => void) | null = null;

export function registerUnauthorizedHandler(handler: (() => void) | null): void {
  unauthorizedHandler = handler;
}

// Chamadas do próprio fluxo de login/restauração tratam 401 como resultado
// normal (credencial inválida / sessão inexistente) — não devem disparar o
// redirecionamento global, ou o formulário de login nunca conseguiria mostrar
// "credenciais inválidas".
const AUTH_BOOTSTRAP_PATHS = ['auth/me', 'auth/login'];

axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      const requestUrl = (error.config?.url ?? '').toLowerCase();
      const isBootstrapCall = AUTH_BOOTSTRAP_PATHS.some((path) => requestUrl.includes(path));
      if (!isBootstrapCall) {
        unauthorizedHandler?.();
      }
    }
    return Promise.reject(error);
  }
);

// Devolve o corpo cru da resposta (T) — a API não envelopa sucesso em
// {success,message,data}; envelopar aqui mentiria pro TypeScript sobre um
// formato que o backend nunca manda.
const api = {
  get: async <T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> => {
    return axiosInstance.get<T>(url, config);
  },
  post: async <T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> => {
    return axiosInstance.post<T>(url, data, config);
  },
  put: async <T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> => {
    return axiosInstance.put<T>(url, data, config);
  },
};

export default api;
