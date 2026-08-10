import axios, { AxiosRequestConfig, AxiosResponse } from 'axios';

// Caminho de mesma origem — em dev, o proxy do Vite (vite.config.ts) encaminha
// /api para VITE_API_PROXY_TARGET; em produção, o nginx (nginx.conf) encaminha
// /api para o backend na rede Docker. Nunca aponta pra um host fixo aqui.
//
// withCredentials: a sessão real (§7) é um cookie HttpOnly único (sem bearer
// separado) — sem isso o navegador não anexa o cookie de sessão às
// requisições. O interceptor de Authorization: Bearer que existia aqui foi
// removido — nunca houve (nem haverá) token/cookie "auth_token"; o backend
// nem lê esse header (mesma correção já aplicada no front-end de estoque, §9).
const axiosInstance = axios.create({
  baseURL: '/api/v1/',
  withCredentials: true,
});

// §10.4: callback registrado pelo AuthProvider — 401 fora do login/restauração
// limpa a sessão em memória e redireciona pro /login. Vive aqui (fora do React) só
// como um ponto de registro; a lógica de fato (guarda contra loop, etc.) mora no
// AuthProvider, evitando importar o router aqui e criar dependência circular com
// as rotas de cada feature (mesmo motivo do useAppRoutes.ts existir).
let unauthorizedHandler: (() => void) | null = null;

export function registerUnauthorizedHandler(handler: (() => void) | null): void {
  unauthorizedHandler = handler;
}

// Chamadas do próprio fluxo de login/restauração tratam 401 como um resultado
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

// Exporta um encapsulamento para uso na aplicação. Devolve o corpo cru da
// resposta (T) — a API não envelopa sucesso em {success,message,data} desde
// a §3a (Problem Details centralizado); envelopar aqui seria mentir pro
// TypeScript sobre um formato que o backend nunca manda.
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
  delete: async <T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> => {
    return axiosInstance.delete<T>(url, config);
  },
  patch: async <T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> => {
    return axiosInstance.patch<T>(url, data, config);
  },
};

export default api;
