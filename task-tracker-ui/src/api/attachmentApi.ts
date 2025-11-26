import { apiClient } from './axiosConfig';

export interface Attachment {
  id: string;
  taskId: string;
  fileName: string;
  fileSize: number;
  uploadedAt: string;
}

export const attachmentApi = {
  getTaskAttachments: async (taskId: string): Promise<Attachment[]> => {
    const response = await apiClient.get(`/Attachments/task/${taskId}`);
    return response.data;
  },

  uploadAttachment: async (taskId: string, file: File): Promise<Attachment> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post(`/Attachments/task/${taskId}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  downloadAttachment: async (attachmentId: string): Promise<void> => {
    const response = await apiClient.get(`/Attachments/${attachmentId}`, {
      responseType: 'blob',
    });

    // Create a download link
    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    
    // Extract filename from Content-Disposition header or use a default
    const contentDisposition = response.headers['content-disposition'];
    let fileName = 'download';
    if (contentDisposition) {
      const fileNameMatch = contentDisposition.match(/filename="?(.+)"?/);
      if (fileNameMatch) {
        fileName = fileNameMatch[1];
      }
    }
    
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  },

  deleteAttachment: async (attachmentId: string): Promise<void> => {
    await apiClient.delete(`/Attachments/${attachmentId}`);
  },
};
