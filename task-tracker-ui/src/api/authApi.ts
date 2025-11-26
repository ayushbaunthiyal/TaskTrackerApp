import { apiClient } from './axiosConfig';
import { LoginRequest, RegisterRequest, AuthResponse } from '../types';

export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await apiClient.post('/Auth/login', data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await apiClient.post('/Auth/register', data);
    return response.data;
  },

  logout: async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      await apiClient.post('/Auth/revoke', { refreshToken });
    }
    localStorage.clear();
  },

  changePassword: async (data: { CurrentPassword: string; NewPassword: string }): Promise<void> => {
    await apiClient.post('/Auth/change-password', data);
  },
};
