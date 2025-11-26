import { apiClient } from './axiosConfig';

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
  getTaskAuditLogs: async (taskId: string): Promise<AuditLog[]> => {
    const response = await apiClient.get(`/AuditLogs/task/${taskId}`);
    return response.data;
  },

  getUserAuditLogs: async (userId: string): Promise<AuditLog[]> => {
    const response = await apiClient.get(`/AuditLogs/user/${userId}`);
    return response.data;
  },
};
