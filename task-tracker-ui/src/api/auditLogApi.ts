import { apiClient } from './axiosConfig';
import { AuditLogListDto, AuditLogFilters, PaginatedResponse } from '../types';

export interface AuditLog {
  id: string;
  userId?: string;
  action: string;
  entityType: string;
  entityId: string;
  timestamp: string;
  details: string;
  userEmail?: string;
}

export const auditLogApi = {
  getAllAuditLogs: async (filters: AuditLogFilters): Promise<PaginatedResponse<AuditLogListDto>> => {
    const params = new URLSearchParams();
    
    if (filters.searchTerm) params.append('searchTerm', filters.searchTerm);
    if (filters.userEmail) params.append('userEmail', filters.userEmail);
    if (filters.entityType) params.append('entityType', filters.entityType);
    if (filters.action) params.append('action', filters.action);
    if (filters.dateFrom) params.append('dateFrom', filters.dateFrom);
    if (filters.dateTo) params.append('dateTo', filters.dateTo);
    if (filters.sortBy) params.append('sortBy', filters.sortBy);
    if (filters.sortDescending !== undefined) params.append('sortDescending', filters.sortDescending.toString());
    params.append('pageNumber', filters.pageNumber.toString());
    params.append('pageSize', filters.pageSize.toString());

    const response = await apiClient.get(`/AuditLogs?${params.toString()}`);
    return response.data;
  },

  getTaskAuditLogs: async (taskId: string): Promise<AuditLog[]> => {
    const response = await apiClient.get(`/AuditLogs/task/${taskId}`);
    return response.data;
  },

  getUserAuditLogs: async (userId: string): Promise<AuditLog[]> => {
    const response = await apiClient.get(`/AuditLogs/user/${userId}`);
    return response.data;
  },
};
