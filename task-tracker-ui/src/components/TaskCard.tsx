import { Link, useNavigate } from 'react-router-dom';
import { Task, TaskStatus, TaskPriority } from '../types';
import { Calendar, Tag, Trash2, Edit, AlertCircle } from 'lucide-react';
import { format, parseISO } from 'date-fns';

interface TaskCardProps {
  task: Task;
  onDelete: (id: string) => void;
  isDueSoon: boolean;
}

export const TaskCard = ({ task, onDelete, isDueSoon }: TaskCardProps) => {
  const navigate = useNavigate();

  const statusColors = {
    [TaskStatus.Pending]: 'bg-gray-100 text-gray-800',
    [TaskStatus.InProgress]: 'bg-blue-100 text-blue-800',
    [TaskStatus.Completed]: 'bg-green-100 text-green-800',
    [TaskStatus.Cancelled]: 'bg-red-100 text-red-800',
  };

  const priorityColors = {
    [TaskPriority.Low]: 'bg-green-100 text-green-800',
    [TaskPriority.Medium]: 'bg-yellow-100 text-yellow-800',
    [TaskPriority.High]: 'bg-orange-100 text-orange-800',
    [TaskPriority.Critical]: 'bg-red-100 text-red-800',
  };

  const statusLabels = {
    [TaskStatus.Pending]: 'Pending',
    [TaskStatus.InProgress]: 'In Progress',
    [TaskStatus.Completed]: 'Completed',
    [TaskStatus.Cancelled]: 'Cancelled',
  };

  const priorityLabels = {
    [TaskPriority.Low]: 'Low',
    [TaskPriority.Medium]: 'Medium',
    [TaskPriority.High]: 'High',
    [TaskPriority.Critical]: 'Critical',
  };

  return (
    <div 
      className={`bg-white rounded-lg shadow-sm hover:shadow-md transition-all p-6 cursor-pointer ${
        isDueSoon ? 'border-2 border-yellow-400' : 'border border-gray-200'
      }`}
      onClick={() => navigate(`/tasks/edit/${task.id}`)}
    >
      {isDueSoon && (
        <div className="flex items-center gap-2 text-yellow-700 text-sm font-medium mb-3 bg-yellow-50 px-3 py-2 rounded">
          <AlertCircle className="w-4 h-4" />
          Due within 24 hours
        </div>
      )}
      
      <div className="flex justify-between items-start mb-4">
        <h3 className="text-lg font-semibold text-gray-900 line-clamp-2 flex-1 pr-2">{task.title}</h3>
        <div className="flex gap-2" onClick={(e) => e.stopPropagation()}>
          <Link
            to={`/tasks/edit/${task.id}`}
            className="text-gray-500 hover:text-indigo-600 transition"
          >
            <Edit className="w-5 h-5" />
          </Link>
          <button
            onClick={(e) => {
              e.stopPropagation();
              onDelete(task.id);
            }}
            className="text-gray-500 hover:text-red-600 transition"
          >
            <Trash2 className="w-5 h-5" />
          </button>
        </div>
      </div>

      <p className="text-gray-600 text-sm mb-4 line-clamp-3">{task.description}</p>

      <div className="flex flex-wrap gap-2 mb-4">
        <span className={`px-3 py-1 rounded-full text-xs font-medium ${statusColors[task.status]}`}>
          {statusLabels[task.status]}
        </span>
        <span className={`px-3 py-1 rounded-full text-xs font-medium ${priorityColors[task.priority]}`}>
          {priorityLabels[task.priority]}
        </span>
      </div>

      {task.tags.length > 0 && (
        <div className="flex items-center gap-2 mb-4 flex-wrap">
          <Tag className="w-4 h-4 text-gray-400" />
          {task.tags.map((tag, index) => (
            <span key={index} className="text-xs text-gray-600 bg-gray-100 px-2 py-1 rounded">
              {tag}
            </span>
          ))}
        </div>
      )}

      {task.dueDate && (
        <div className="flex items-center gap-2 text-sm text-gray-500">
          <Calendar className="w-4 h-4" />
          Due: {format(parseISO(task.dueDate), 'MMM dd, yyyy')}
        </div>
      )}
    </div>
  );
};
