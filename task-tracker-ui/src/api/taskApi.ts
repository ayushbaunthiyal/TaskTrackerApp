import { apiClient } from './axiosConfig';
import { Task, TaskFormData, PaginatedResponse, TaskFilters } from '../types';

export const taskApi = {
  getTasks: async (filters: TaskFilters): Promise<PaginatedResponse<Task>> => {
    const params = new URLSearchParams();
    
    if (filters.searchTerm) params.append('searchTerm', filters.searchTerm);
    if (filters.status !== undefined) params.append('status', filters.status.toString());
    if (filters.priority !== undefined) params.append('priority', filters.priority.toString());
    if (filters.sortBy) params.append('sortBy', filters.sortBy);
    if (filters.sortDescending !== undefined) params.append('sortDescending', filters.sortDescending.toString());
    params.append('pageNumber', filters.pageNumber.toString());
    params.append('pageSize', filters.pageSize.toString());

    const response = await apiClient.get(`/Tasks?${params.toString()}`);
    return response.data;
  },

  getTaskById: async (id: string): Promise<Task> => {
    const response = await apiClient.get(`/Tasks/${id}`);
    return response.data;
  },

  createTask: async (data: TaskFormData): Promise<Task> => {
    const response = await apiClient.post('/Tasks', data);
    return response.data;
  },

  updateTask: async (id: string, data: TaskFormData): Promise<Task> => {
    const response = await apiClient.put(`/Tasks/${id}`, data);
    return response.data;
  },

  deleteTask: async (id: string): Promise<void> => {
    await apiClient.delete(`/Tasks/${id}`);
  },
};
