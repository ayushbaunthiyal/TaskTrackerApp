import { useState, useEffect, FormEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { taskApi } from '../api/taskApi';
import { attachmentApi, Attachment } from '../api/attachmentApi';
import { TaskStatus, TaskPriority, TaskFormData } from '../types';
import { Save, X } from 'lucide-react';
import toast from 'react-hot-toast';
import { FileUpload } from './FileUpload';
import { AuditTrail } from './AuditTrail';
import { getCurrentUserId } from '../utils/jwt';

export const TaskForm = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState<TaskFormData>({
    Title: '',
    Description: '',
    Status: TaskStatus.Pending,
    Priority: TaskPriority.Medium,
    Tags: [],
    DueDate: null,
  });
  const [tagInput, setTagInput] = useState('');
  const [attachments, setAttachments] = useState<Attachment[]>([]);
  const [taskOwnerId, setTaskOwnerId] = useState<string | null>(null);
  const currentUserId = getCurrentUserId();
  const isOwner = !id || !taskOwnerId || taskOwnerId === currentUserId;

  useEffect(() => {
    if (id) loadTask();
  }, [id]);

  const loadTask = async () => {
    try {
      const task = await taskApi.getTaskById(id!);
      setFormData({
        Title: task.title,
        Description: task.description,
        Status: task.status,
        Priority: task.priority,
        Tags: task.tags,
        DueDate: task.dueDate ? task.dueDate.split('T')[0] : null,
      });
      setTaskOwnerId(task.userId);
      
      // Load attachments
      const taskAttachments = await attachmentApi.getTaskAttachments(id!);
      setAttachments(taskAttachments);
    } catch (error) {
      toast.error('Failed to load task');
      navigate('/tasks');
    }
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      // Convert date to ISO string with timezone if provided
      const dataToSend = {
        ...formData,
        DueDate: formData.DueDate ? new Date(formData.DueDate + 'T00:00:00Z').toISOString() : null,
      };

      if (id) {
        await taskApi.updateTask(id, dataToSend);
        toast.success('Task updated successfully');
      } else {
        await taskApi.createTask(dataToSend);
        toast.success('Task created successfully');
      }
      navigate('/tasks');
    } catch (error: any) {
      const message = error.response?.data?.error || `Failed to ${id ? 'update' : 'create'} task`;
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  const addTag = () => {
    if (tagInput.trim() && !formData.Tags.includes(tagInput.trim())) {
      setFormData({ ...formData, Tags: [...formData.Tags, tagInput.trim()] });
      setTagInput('');
    }
  };

  const removeTag = (tag: string) => {
    setFormData({ ...formData, Tags: formData.Tags.filter(t => t !== tag) });
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="bg-white rounded-lg shadow-sm p-8">
          <div className="flex justify-between items-center mb-6">
            <h1 className="text-2xl font-bold text-gray-900">
              {id ? 'Edit Task' : 'Create New Task'}
            </h1>
            <button
              onClick={() => navigate('/tasks')}
              className="text-gray-500 hover:text-gray-700"
            >
              <X className="w-6 h-6" />
            </button>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Title *
              </label>
              <input
                type="text"
                value={formData.Title}
                onChange={(e) => setFormData({ ...formData, Title: e.target.value })}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                required
                maxLength={200}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Description *
              </label>
              <textarea
                value={formData.Description}
                onChange={(e) => setFormData({ ...formData, Description: e.target.value })}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                rows={4}
                required
                maxLength={1000}
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Status
                </label>
                <select
                  value={formData.Status}
                  onChange={(e) => setFormData({ ...formData, Status: Number(e.target.value) })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                >
                  <option value={TaskStatus.Pending}>Pending</option>
                  <option value={TaskStatus.InProgress}>In Progress</option>
                  <option value={TaskStatus.Completed}>Completed</option>
                  <option value={TaskStatus.Cancelled}>Cancelled</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Priority
                </label>
                <select
                  value={formData.Priority}
                  onChange={(e) => setFormData({ ...formData, Priority: Number(e.target.value) })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                >
                  <option value={TaskPriority.Low}>Low</option>
                  <option value={TaskPriority.Medium}>Medium</option>
                  <option value={TaskPriority.High}>High</option>
                  <option value={TaskPriority.Critical}>Critical</option>
                </select>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Due Date
              </label>
              <input
                type="date"
                value={formData.DueDate || ''}
                onChange={(e) => setFormData({ ...formData, DueDate: e.target.value || null })}
                min={new Date().toISOString().split('T')[0]}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Tags
              </label>
              <div className="flex gap-2 mb-3">
                <input
                  type="text"
                  value={tagInput}
                  onChange={(e) => setTagInput(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addTag())}
                  className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                  placeholder="Add a tag..."
                />
                <button
                  type="button"
                  onClick={addTag}
                  className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition"
                >
                  Add
                </button>
              </div>
              <div className="flex flex-wrap gap-2">
                {formData.Tags.map((tag) => (
                  <span
                    key={tag}
                    className="inline-flex items-center gap-1 bg-indigo-100 text-indigo-700 px-3 py-1 rounded-full text-sm"
                  >
                    {tag}
                    <button
                      type="button"
                      onClick={() => removeTag(tag)}
                      className="hover:text-indigo-900"
                    >
                      <X className="w-4 h-4" />
                    </button>
                  </span>
                ))}
              </div>
            </div>

            <FileUpload
              taskId={id || null}
              attachments={attachments}
              onAttachmentsChange={setAttachments}
              isOwner={isOwner}
            />

            <AuditTrail taskId={id || null} />

            <div className="flex gap-4 pt-4">
              <button
                type="submit"
                disabled={loading}
                className="flex-1 inline-flex items-center justify-center gap-2 bg-indigo-600 text-white px-6 py-3 rounded-lg hover:bg-indigo-700 transition disabled:opacity-50"
              >
                <Save className="w-5 h-5" />
                {loading ? 'Saving...' : id ? 'Update Task' : 'Create Task'}
              </button>
              <button
                type="button"
                onClick={() => navigate('/tasks')}
                className="px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};
