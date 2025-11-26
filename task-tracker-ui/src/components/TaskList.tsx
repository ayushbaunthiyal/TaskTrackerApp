import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { taskApi } from '../api/taskApi';
import { Task, TaskFilters, TaskStatus, TaskPriority } from '../types';
import { Plus, Search, Filter, LogOut } from 'lucide-react';
import toast from 'react-hot-toast';
import { TaskCard } from './TaskCard';
import { differenceInHours, parseISO } from 'date-fns';
import { useAuth } from '../context/AuthContext';

export const TaskList = () => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState<TaskFilters>({
    searchTerm: '',
    pageNumber: 1,
    pageSize: 25,
  });
  const [totalPages, setTotalPages] = useState(1);
  const [showFilters, setShowFilters] = useState(false);
  const { logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    loadTasks();
  }, [filters]);

  const loadTasks = async () => {
    try {
      setLoading(true);
      const response = await taskApi.getTasks(filters);
      console.log('Tasks API Response:', response);
      console.log('Tasks items:', response.items);
      console.log('Total pages:', response.totalPages);
      setTasks(response.items || []);
      setTotalPages(response.totalPages || 1);
    } catch (error: any) {
      console.error('Error loading tasks:', error);
      console.error('Error response:', error.response);
      toast.error('Failed to load tasks');
      setTasks([]);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this task?')) return;
    
    try {
      await taskApi.deleteTask(id);
      toast.success('Task deleted successfully');
      loadTasks();
    } catch (error: any) {
      const message = error.response?.data?.error || 'Failed to delete task';
      toast.error(message);
    }
  };

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const isDueSoon = (dueDate: string | null): boolean => {
    if (!dueDate) return false;
    const hours = differenceInHours(parseISO(dueDate), new Date());
    return hours > 0 && hours <= 24;
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Active Tasks</h1>
            <p className="text-gray-600 mt-1">Manage and track Tasks</p>
          </div>
          <div className="flex gap-3">
            <Link
              to="/tasks/new"
              className="inline-flex items-center gap-2 bg-indigo-600 text-white px-6 py-3 rounded-lg hover:bg-indigo-700 transition shadow-sm"
            >
              <Plus className="w-5 h-5" />
              New Task
            </Link>
            <button
              onClick={handleLogout}
              className="inline-flex items-center gap-2 bg-gray-200 text-gray-700 px-6 py-3 rounded-lg hover:bg-gray-300 transition shadow-sm"
            >
              <LogOut className="w-5 h-5" />
              Logout
            </button>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
          <div className="flex gap-4 items-center">
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <input
                type="text"
                placeholder="Search tasks..."
                value={filters.searchTerm}
                onChange={(e) => setFilters({ ...filters, searchTerm: e.target.value, pageNumber: 1 })}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
              />
            </div>
            <button
              onClick={() => setShowFilters(!showFilters)}
              className="inline-flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition"
            >
              <Filter className="w-5 h-5" />
              Filters
            </button>
          </div>

          {showFilters && (
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4 pt-4 border-t">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Status</label>
                <select
                  value={filters.status ?? ''}
                  onChange={(e) => setFilters({ ...filters, status: e.target.value ? Number(e.target.value) : undefined, pageNumber: 1 })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                >
                  <option value="">All</option>
                  <option value={TaskStatus.Pending}>Pending</option>
                  <option value={TaskStatus.InProgress}>In Progress</option>
                  <option value={TaskStatus.Completed}>Completed</option>
                  <option value={TaskStatus.Cancelled}>Cancelled</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Priority</label>
                <select
                  value={filters.priority ?? ''}
                  onChange={(e) => setFilters({ ...filters, priority: e.target.value ? Number(e.target.value) : undefined, pageNumber: 1 })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                >
                  <option value="">All</option>
                  <option value={TaskPriority.Low}>Low</option>
                  <option value={TaskPriority.Medium}>Medium</option>
                  <option value={TaskPriority.High}>High</option>
                  <option value={TaskPriority.Critical}>Critical</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Sort By</label>
                <select
                  value={filters.sortBy ?? 'CreatedAt'}
                  onChange={(e) => setFilters({ ...filters, sortBy: e.target.value, pageNumber: 1 })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                >
                  <option value="CreatedAt">Created Date</option>
                  <option value="UpdatedAt">Updated Date</option>
                  <option value="DueDate">Due Date</option>
                  <option value="Priority">Priority</option>
                  <option value="Status">Status</option>
                </select>
              </div>
            </div>
          )}
        </div>

        {loading ? (
          <div className="text-center py-12">
            <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
          </div>
        ) : tasks.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg shadow-sm">
            <p className="text-gray-500 text-lg">No tasks found</p>
            <Link to="/tasks/new" className="text-indigo-600 hover:text-indigo-700 mt-2 inline-block">
              Create your first task
            </Link>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {tasks.map((task) => (
                <TaskCard
                  key={task.id}
                  task={task}
                  onDelete={handleDelete}
                  isDueSoon={isDueSoon(task.dueDate)}
                />
              ))}
            </div>

            {totalPages > 1 && (
              <div className="flex justify-center gap-2 mt-8">
                <button
                  onClick={() => setFilters({ ...filters, pageNumber: filters.pageNumber - 1 })}
                  disabled={filters.pageNumber === 1}
                  className="px-4 py-2 border border-gray-300 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
                >
                  Previous
                </button>
                <span className="px-4 py-2 text-gray-700">
                  Page {filters.pageNumber} of {totalPages}
                </span>
                <button
                  onClick={() => setFilters({ ...filters, pageNumber: filters.pageNumber + 1 })}
                  disabled={filters.pageNumber === totalPages}
                  className="px-4 py-2 border border-gray-300 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
                >
                  Next
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};
