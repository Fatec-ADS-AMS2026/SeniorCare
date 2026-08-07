import axios, { AxiosRequestConfig, AxiosResponse } from 'axios';
import Cookies from 'js-cookie';
import { ApiResponse } from './types';

// Caminho de mesma origem — em dev, o proxy do Vite (vite.config.ts) encaminha
// /api para VITE_API_PROXY_TARGET; em produção, o nginx (nginx.conf) encaminha
// /api para o backend na rede Docker. Nunca aponta pra um host fixo aqui.
const axiosInstance = axios.create({
  baseURL: '/api/v1/',
});

// Interceptor para adicionar o token JWT a cada requisição
axiosInstance.interceptors.request.use(
  (config) => {
    const token = Cookies.get('auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Exporta um encapsulamento para uso na aplicação
const api = {
  get: async <T>(url: string, config?: AxiosRequestConfig) => {
    const response = await axiosInstance.get(url, config);
    return response as AxiosResponse<ApiResponse<T>>;
  },
  post: async <T>(url: string, data?: T, config?: AxiosRequestConfig) => {
    const response = await axiosInstance.post(url, data, config);
    return response as AxiosResponse<ApiResponse<T>>;
  },
  put: async <T>(url: string, data?: T, config?: AxiosRequestConfig) => {
    const response = await axiosInstance.put(url, data, config);
    return response as AxiosResponse<ApiResponse<T>>;
  },
  delete: async <T>(url: string, config?: AxiosRequestConfig) => {
    const response = await axiosInstance.delete(url, config);
    return response as AxiosResponse<ApiResponse<T>>;
  },
  patch: async <T>(url: string, data?: T, config?: AxiosRequestConfig) => {
    const response = await axiosInstance.patch(url, data, config);
    return response as AxiosResponse<ApiResponse<T>>;
  },
};

export default api;
