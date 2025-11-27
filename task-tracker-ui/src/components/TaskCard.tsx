import { Link, useNavigate } from 'react-router-dom';
import { Task, TaskStatus, TaskPriority } from '../types';
import { Calendar, Tag, Trash2, Edit, AlertCircle, Paperclip, User } from 'lucide-react';
import { format, parseISO } from 'date-fns';

interface TaskCardProps {
  task: Task;
  onDelete: (id: string) => void;
  isOverdue: boolean;
  isDueToday: boolean;
  isDueSoon: boolean;
}

export const TaskCard = ({ task, onDelete, isOverdue, isDueToday, isDueSoon }: TaskCardProps) => {
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

  // Determine border and background based on due date status
  let cardClasses = 'bg-white rounded-lg shadow-sm hover:shadow-md transition-all p-4 cursor-pointer ';
  if (isOverdue) {
    cardClasses += 'border-2 border-red-500 bg-red-50';
  } else if (isDueToday) {
    cardClasses += 'border-2 border-orange-500 bg-orange-50';
  } else if (isDueSoon) {
    cardClasses += 'border-2 border-yellow-400';
  } else {
    cardClasses += 'border border-gray-200';
  }

  return (
    <div 
      className={cardClasses}
      onClick={() => navigate(`/tasks/edit/${task.id}`)}
    >
      {isOverdue && (
        <div className="flex items-center gap-2 text-red-700 text-sm font-medium mb-2 bg-red-100 px-2 py-1 rounded">
          <AlertCircle className="w-4 h-4" />
          Overdue
        </div>
      )}
      {isDueToday && (
        <div className="flex items-center gap-2 text-orange-700 text-sm font-medium mb-2 bg-orange-100 px-2 py-1 rounded">
          <AlertCircle className="w-4 h-4" />
          Due Today
        </div>
      )}
      {isDueSoon && (
        <div className="flex items-center gap-2 text-yellow-700 text-sm font-medium mb-2 bg-yellow-50 px-2 py-1 rounded">
          <AlertCircle className="w-4 h-4" />
          Due Soon
        </div>
      )}
      
      <div className="flex justify-between items-start mb-3">
        <div className="flex-1 pr-2">
          <h3 className="text-base font-semibold text-gray-900 line-clamp-2">{task.title}</h3>
          <div className="flex items-center gap-1 text-xs text-gray-500 mt-1">
            <User className="w-3 h-3" />
            <span>{task.userName}</span>
          </div>
        </div>
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

      <p className="text-gray-600 text-sm mb-3 line-clamp-3">{task.description}</p>

      <div className="flex flex-wrap gap-2 mb-3">
        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${statusColors[task.status]}`}>
          {statusLabels[task.status]}
        </span>
        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${priorityColors[task.priority]}`}>
          {priorityLabels[task.priority]}
        </span>
      </div>

      {task.tags.length > 0 && (
        <div className="flex items-center gap-2 mb-2 flex-wrap">
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

      {task.attachmentCount !== undefined && task.attachmentCount > 0 && (
        <div className="flex items-center gap-2 text-sm text-gray-500 mt-2">
          <Paperclip className="w-4 h-4" />
          {task.attachmentCount} attachment{task.attachmentCount > 1 ? 's' : ''}
        </div>
      )}
    </div>
  );
};
