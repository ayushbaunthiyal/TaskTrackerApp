import { useState, useRef, DragEvent } from 'react';
import { Upload, File, Download, Trash2 } from 'lucide-react';
import { attachmentApi, Attachment } from '../api/attachmentApi';
import toast from 'react-hot-toast';
import { ConfirmDialog } from './ConfirmDialog';

interface FileUploadProps {
  taskId: string | null;
  attachments: Attachment[];
  onAttachmentsChange: (attachments: Attachment[]) => void;
  isOwner?: boolean;
}

export const FileUpload = ({ taskId, attachments, onAttachmentsChange, isOwner = true }: FileUploadProps) => {
  const [uploading, setUploading] = useState(false);
  const [dragActive, setDragActive] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [attachmentToDelete, setAttachmentToDelete] = useState<Attachment | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDrag = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  };

  const handleDrop = async (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);

    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      await handleFiles(Array.from(e.dataTransfer.files));
    }
  };

  const handleChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    e.preventDefault();
    if (e.target.files && e.target.files[0]) {
      await handleFiles(Array.from(e.target.files));
    }
  };

  const handleFiles = async (files: File[]) => {
    if (!taskId) {
      toast.error('Please save the task first before uploading attachments');
      return;
    }

    for (const file of files) {
      // Validate file size (10MB max)
      if (file.size > 10 * 1024 * 1024) {
        toast.error(`${file.name} is too large. Maximum size is 10MB`);
        continue;
      }

      try {
        setUploading(true);
        const attachment = await attachmentApi.uploadAttachment(taskId, file);
        onAttachmentsChange([...attachments, attachment]);
        toast.success(`${file.name} uploaded successfully`);
      } catch (error: any) {
        if (error.response?.status === 401 || error.response?.status === 403) {
          toast.error('You can only upload/delete attachments to your own tasks');
        } else {
          const message = error.response?.data?.error || `Failed to upload ${file.name}`;
          toast.error(message);
        }
      } finally {
        setUploading(false);
      }
    }

    // Reset file input
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleDownload = async (attachment: Attachment) => {
    try {
      await attachmentApi.downloadAttachment(attachment.id);
    } catch (error: any) {
      toast.error('Failed to download file');
    }
  };

  const handleDelete = async (attachment: Attachment) => {
    setAttachmentToDelete(attachment);
    setDeleteDialogOpen(true);
  };

  const confirmDelete = async () => {
    if (!attachmentToDelete) return;

    try {
      await attachmentApi.deleteAttachment(attachmentToDelete.id);
      onAttachmentsChange(attachments.filter(a => a.id !== attachmentToDelete.id));
      toast.success('Attachment deleted successfully');
    } catch (error: any) {
      if (error.response?.status === 401 || error.response?.status === 403) {
        toast.error('You can only upload/delete attachments to your own tasks');
      } else {
        const message = error.response?.data?.error || 'Failed to delete attachment';
        toast.error(message);
      }
    } finally {
      setDeleteDialogOpen(false);
      setAttachmentToDelete(null);
    }
  };

  const cancelDelete = () => {
    setDeleteDialogOpen(false);
    setAttachmentToDelete(null);
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  };

  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-2">
        Attachments
      </label>

      {/* Upload Area */}
      <div
        className={`relative border-2 border-dashed rounded-lg p-6 text-center transition ${
          dragActive
            ? 'border-indigo-500 bg-indigo-50'
            : 'border-gray-300 hover:border-gray-400'
        } ${!taskId || !isOwner ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}`}
        onDragEnter={isOwner ? handleDrag : undefined}
        onDragLeave={isOwner ? handleDrag : undefined}
        onDragOver={isOwner ? handleDrag : undefined}
        onDrop={isOwner ? handleDrop : undefined}
        onClick={() => taskId && isOwner && fileInputRef.current?.click()}
        title={!isOwner ? 'You can only upload/delete attachments to your own tasks' : ''}
        onDragEnter={handleDrag}
        onDragLeave={handleDrag}
        onDragOver={handleDrag}
        onDrop={handleDrop}
        onClick={() => taskId && fileInputRef.current?.click()}
      >
        <input
          ref={fileInputRef}
          type="file"
          multiple
          onChange={handleChange}
          className="hidden"
          disabled={!taskId || uploading || !isOwner}
        />
        
        <Upload className="w-10 h-10 mx-auto text-gray-400 mb-2" />
        <p className="text-sm text-gray-600">
          {!isOwner
            ? 'You can only upload/delete attachments to your own tasks'
            : taskId
            ? uploading
              ? 'Uploading...'
              : 'Drag & drop files here, or click to select'
            : 'Save the task first to upload attachments'}
        </p>
        <p className="text-xs text-gray-500 mt-1">
          Maximum file size: 10MB
        </p>
      </div>

      {/* Attachments List */}
      {attachments.length > 0 && (
        <div className="mt-4 space-y-2">
          {attachments.map((attachment) => (
            <div
              key={attachment.id}
              className="flex items-center justify-between bg-gray-50 p-3 rounded-lg border border-gray-200"
            >
              <div className="flex items-center gap-3 flex-1 min-w-0">
                <File className="w-5 h-5 text-gray-400 flex-shrink-0" />
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900 truncate">
                    {attachment.fileName}
                  </p>
                  <p className="text-xs text-gray-500">
                    {formatFileSize(attachment.fileSize)} • {new Date(attachment.uploadedAt).toLocaleDateString()}
                  </p>
                </div>
              </div>
              <div className="flex items-center gap-2 flex-shrink-0">
                <button
                  type="button"
                  onClick={() => handleDownload(attachment)}
                  className="p-2 text-gray-500 hover:text-indigo-600 transition"
                  title="Download"
                >
                  <Download className="w-4 h-4" />
                </button>
                <button
                  type="button"
                  onClick={() => isOwner && handleDelete(attachment)}
                  disabled={!isOwner}
                  className={`p-2 transition ${
                    isOwner
                      ? 'text-gray-500 hover:text-red-600 cursor-pointer'
                      : 'text-gray-300 cursor-not-allowed'
                  }`}
                  title={isOwner ? 'Delete' : 'You can only upload/delete attachments to your own tasks'}
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      <ConfirmDialog
        isOpen={deleteDialogOpen}
        title="Delete Attachment"
        message={`Are you sure you want to delete "${attachmentToDelete?.fileName}"? This action cannot be undone.`}
        confirmText="Delete"
        cancelText="Cancel"
        variant="danger"
        onConfirm={confirmDelete}
        onCancel={cancelDelete}
      />
    </div>
  );
};
