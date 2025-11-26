import { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { taskApi } from '../api/taskApi';
import { Task, TaskFilters, TaskStatus, TaskPriority } from '../types';
import { Plus, Search, Filter, LogOut, RotateCcw, User, ChevronDown, Key } from 'lucide-react';
import toast from 'react-hot-toast';
import { TaskCard } from './TaskCard';
import { ConfirmDialog } from './ConfirmDialog';
import { useAuth } from '../context/AuthContext';
import { getCurrentUser } from '../utils/jwt';

export const TaskList = () => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState<TaskFilters>({
    searchTerm: '',
    sortBy: 'DueDate',
    sortDescending: false,
    pageNumber: 1,
    pageSize: 8,
  });
  const [totalPages, setTotalPages] = useState(1);
  const [showFilters, setShowFilters] = useState(false);
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState<{ isOpen: boolean; taskId: string | null }>({
    isOpen: false,
    taskId: null,
  });
  const { logout } = useAuth();
  const navigate = useNavigate();
  const userMenuRef = useRef<HTMLDivElement>(null);
  const currentUser = getCurrentUser();

  useEffect(() => {
    loadTasks();
  }, [filters]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (userMenuRef.current && !userMenuRef.current.contains(event.target as Node)) {
        setShowUserMenu(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

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
    setDeleteConfirm({ isOpen: true, taskId: id });
  };

  const confirmDelete = async () => {
    if (!deleteConfirm.taskId) return;
    
    try {
      await taskApi.deleteTask(deleteConfirm.taskId);
      toast.success('Task deleted successfully');
      setDeleteConfirm({ isOpen: false, taskId: null });
      loadTasks();
    } catch (error: any) {
      const message = error.response?.data?.error || 'Failed to delete task';
      toast.error(message);
      setDeleteConfirm({ isOpen: false, taskId: null });
    }
  };

  const cancelDelete = () => {
    setDeleteConfirm({ isOpen: false, taskId: null });
  };

  const resetFilters = () => {
    setFilters({
      searchTerm: '',
      sortBy: 'DueDate',
      sortDescending: false,
      pageNumber: 1,
      pageSize: 8,
    });
    toast.success('Filters reset');
  };

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const isOverdue = (dueDate: string | null): boolean => {
    if (!dueDate) return false;
    const taskDate = new Date(dueDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    taskDate.setHours(0, 0, 0, 0);
    return taskDate < today;
  };

  const isDueToday = (dueDate: string | null): boolean => {
    if (!dueDate) return false;
    const taskDate = new Date(dueDate);
    const today = new Date();
    taskDate.setHours(0, 0, 0, 0);
    today.setHours(0, 0, 0, 0);
    return taskDate.getTime() === today.getTime();
  };

  const isDueSoon = (dueDate: string | null): boolean => {
    if (!dueDate) return false;
    const taskDate = new Date(dueDate);
    const today = new Date();
    taskDate.setHours(0, 0, 0, 0);
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);
    const twoDaysFromNow = new Date(today);
    twoDaysFromNow.setDate(twoDaysFromNow.getDate() + 2);
    return taskDate >= tomorrow && taskDate <= twoDaysFromNow;
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <ConfirmDialog
        isOpen={deleteConfirm.isOpen}
        title="Delete Task"
        message="Are you sure you want to delete this task? This action cannot be undone."
        confirmText="Delete"
        cancelText="Cancel"
        variant="danger"
        onConfirm={confirmDelete}
        onCancel={cancelDelete}
      />
      
      <div className="max-w-[1600px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Active Tasks</h1>
            <p className="text-gray-600 mt-1">Manage and track Tasks</p>
          </div>
          <div className="flex gap-3 items-center">
            <Link
              to="/tasks/new"
              className="inline-flex items-center gap-2 bg-indigo-600 text-white px-6 py-3 rounded-lg hover:bg-indigo-700 transition shadow-sm"
            >
              <Plus className="w-5 h-5" />
              New Task
            </Link>
            
            {/* User Profile Dropdown */}
            <div className="relative" ref={userMenuRef}>
              <button
                onClick={() => setShowUserMenu(!showUserMenu)}
                className="inline-flex items-center gap-3 bg-white border-2 border-gray-200 px-4 py-2.5 rounded-lg hover:border-indigo-300 hover:bg-indigo-50 transition shadow-sm"
              >
                <div className="w-8 h-8 bg-indigo-600 rounded-full flex items-center justify-center">
                  <User className="w-5 h-5 text-white" />
                </div>
                <div className="text-left hidden md:block">
                  <p className="text-sm font-semibold text-gray-900">
                    {currentUser?.firstName} {currentUser?.lastName}
                  </p>
                  <p className="text-xs text-gray-500">{currentUser?.email}</p>
                </div>
                <ChevronDown className={`w-4 h-4 text-gray-600 transition-transform ${showUserMenu ? 'rotate-180' : ''}`} />
              </button>

              {/* Dropdown Menu */}
              {showUserMenu && (
                <div className="absolute right-0 mt-2 w-64 bg-white rounded-lg shadow-xl border border-gray-200 py-2 z-50">
                  {/* User Info Section */}
                  <div className="px-4 py-3 border-b border-gray-100">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 bg-indigo-600 rounded-full flex items-center justify-center">
                        <User className="w-6 h-6 text-white" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-semibold text-gray-900 truncate">
                          {currentUser?.firstName} {currentUser?.lastName}
                        </p>
                        <p className="text-xs text-gray-500 truncate">{currentUser?.email}</p>
                      </div>
                    </div>
                  </div>

                  {/* Menu Items */}
                  <div className="py-1">
                    <Link
                      to="/change-password"
                      onClick={() => setShowUserMenu(false)}
                      className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 hover:bg-indigo-50 hover:text-indigo-700 transition"
                    >
                      <Key className="w-4 h-4" />
                      Change Password
                    </Link>
                    <button
                      onClick={() => {
                        setShowUserMenu(false);
                        handleLogout();
                      }}
                      className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-red-600 hover:bg-red-50 transition"
                    >
                      <LogOut className="w-4 h-4" />
                      Logout
                    </button>
                  </div>
                </div>
              )}
            </div>
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
            <button
              onClick={resetFilters}
              className="inline-flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition text-gray-700"
              title="Reset all filters"
            >
              <RotateCcw className="w-5 h-5" />
              Reset
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
                  value={filters.sortBy ?? 'DueDate'}
                  onChange={(e) => setFilters({ ...filters, sortBy: e.target.value, pageNumber: 1 })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                >
                  <option value="DueDate">Due Date</option>
                  <option value="CreatedAt">Created Date</option>
                  <option value="UpdatedAt">Updated Date</option>
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
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {tasks.map((task) => {
                const isActive = task.status !== TaskStatus.Completed && task.status !== TaskStatus.Cancelled;
                return (
                  <TaskCard
                    key={task.id}
                    task={task}
                    onDelete={handleDelete}
                    isOverdue={isActive && isOverdue(task.dueDate)}
                    isDueToday={isActive && isDueToday(task.dueDate)}
                    isDueSoon={isActive && isDueSoon(task.dueDate)}
                  />
                );
              })}
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
