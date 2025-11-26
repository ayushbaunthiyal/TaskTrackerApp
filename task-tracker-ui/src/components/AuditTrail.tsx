import { useState, useEffect } from 'react';
import { auditLogApi, AuditLog } from '../api/auditLogApi';
import { Clock, FileUp, FileDown, Edit, Trash2, Plus, ChevronDown, ChevronUp } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import toast from 'react-hot-toast';

interface AuditTrailProps {
  taskId: string | null;
}

export const AuditTrail = ({ taskId }: AuditTrailProps) => {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);

  useEffect(() => {
    if (taskId && isExpanded) {
      loadAuditLogs();
    }
  }, [taskId, isExpanded]);

  const loadAuditLogs = async () => {
    if (!taskId) return;
    
    setLoading(true);
    try {
      const data = await auditLogApi.getTaskAuditLogs(taskId);
      setLogs(data);
    } catch (error) {
      toast.error('Failed to load audit trail');
    } finally {
      setLoading(false);
    }
  };

  const getActionIcon = (action: string) => {
    switch (action) {
      case 'Created':
        return <Plus className="w-4 h-4 text-green-600" />;
      case 'Updated':
        return <Edit className="w-4 h-4 text-blue-600" />;
      case 'Deleted':
        return <Trash2 className="w-4 h-4 text-red-600" />;
      case 'AttachmentUploaded':
        return <FileUp className="w-4 h-4 text-indigo-600" />;
      case 'AttachmentDeleted':
        return <FileDown className="w-4 h-4 text-orange-600" />;
      default:
        return <Clock className="w-4 h-4 text-gray-600" />;
    }
  };

  const getActionColor = (action: string) => {
    switch (action) {
      case 'Created':
        return 'bg-green-100 border-green-300';
      case 'Updated':
        return 'bg-blue-100 border-blue-300';
      case 'Deleted':
        return 'bg-red-100 border-red-300';
      case 'AttachmentUploaded':
        return 'bg-indigo-100 border-indigo-300';
      case 'AttachmentDeleted':
        return 'bg-orange-100 border-orange-300';
      default:
        return 'bg-gray-100 border-gray-300';
    }
  };

  if (!taskId) {
    return (
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 text-center">
        <p className="text-yellow-700 text-sm">
          Save the task first to view its audit trail
        </p>
      </div>
    );
  }

  return (
    <div className="border border-gray-300 rounded-lg overflow-hidden">
      <button
        type="button"
        onClick={() => setIsExpanded(!isExpanded)}
        className="w-full flex items-center justify-between bg-gray-50 px-4 py-3 hover:bg-gray-100 transition"
      >
        <div className="flex items-center gap-2">
          <Clock className="w-5 h-5 text-gray-600" />
          <span className="font-medium text-gray-900">Audit Trail</span>
          {logs.length > 0 && (
            <span className="bg-gray-200 text-gray-700 text-xs px-2 py-1 rounded-full">
              {logs.length}
            </span>
          )}
        </div>
        {isExpanded ? (
          <ChevronUp className="w-5 h-5 text-gray-600" />
        ) : (
          <ChevronDown className="w-5 h-5 text-gray-600" />
        )}
      </button>

      {isExpanded && (
        <div className="p-4 bg-white">
          {loading ? (
            <div className="text-center py-8">
              <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
              <p className="text-gray-500 mt-2">Loading audit trail...</p>
            </div>
          ) : logs.length === 0 ? (
            <div className="text-center py-8">
              <Clock className="w-12 h-12 text-gray-300 mx-auto mb-2" />
              <p className="text-gray-500">No audit logs yet</p>
            </div>
          ) : (
            <div className="space-y-3">
              {logs.map((log) => (
                <div
                  key={log.id}
                  className={`flex gap-3 p-3 rounded-lg border ${getActionColor(log.action)}`}
                >
                  <div className="flex-shrink-0 mt-1">
                    {getActionIcon(log.action)}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-start justify-between gap-2">
                      <div className="flex-1">
                        <p className="font-medium text-gray-900">{log.action}</p>
                        {log.details && (
                          <p className="text-sm text-gray-600 mt-1">{log.details}</p>
                        )}
                      </div>
                    </div>
                    <div className="flex items-center gap-3 mt-2 text-xs text-gray-500">
                      {log.userEmail && (
                        <span className="flex items-center gap-1">
                          <span className="font-medium">{log.userEmail}</span>
                        </span>
                      )}
                      <span className="flex items-center gap-1">
                        <Clock className="w-3 h-3" />
                        {formatDistanceToNow(new Date(log.timestamp), { addSuffix: true })}
                      </span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};
