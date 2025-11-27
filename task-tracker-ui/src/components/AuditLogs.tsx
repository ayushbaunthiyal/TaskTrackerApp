import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { auditLogApi } from '../api/auditLogApi';
import { AuditLogListDto, AuditLogFilters } from '../types';
import { ArrowLeft, Search, Filter, ChevronLeft, ChevronRight, Calendar, User, FileText, RotateCcw } from 'lucide-react';
import toast from 'react-hot-toast';

export const AuditLogs = () => {
  const [logs, setLogs] = useState<AuditLogListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState<AuditLogFilters>({
    searchTerm: '',
    sortBy: 'Timestamp',
    sortDescending: true,
    pageNumber: 1,
    pageSize: 7,
  });
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [showFilters, setShowFilters] = useState(false);
  const navigate = useNavigate();

  // Get unique values for filter dropdowns
  const [entityTypes, setEntityTypes] = useState<string[]>([]);
  const [actions, setActions] = useState<string[]>([]);

  useEffect(() => {
    loadAuditLogs();
  }, [filters]);

  const loadAuditLogs = async () => {
    try {
      setLoading(true);
      const response = await auditLogApi.getAllAuditLogs(filters);
      setLogs(response.items || []);
      setTotalPages(response.totalPages || 1);
      setTotalCount(response.totalCount || 0);

      // Extract unique entity types and actions
      const uniqueEntityTypes = [...new Set(response.items.map(log => log.entityType))];
      const uniqueActions = [...new Set(response.items.map(log => log.action))];
      setEntityTypes(uniqueEntityTypes);
      setActions(uniqueActions);
    } catch (error: any) {
      console.error('Error loading audit logs:', error);
      toast.error('Failed to load audit logs');
      setLogs([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSearchChange = (value: string) => {
    setFilters({ ...filters, searchTerm: value, pageNumber: 1 });
  };

  const handleRefresh = () => {
    setFilters({ ...filters, pageNumber: 1 });
  };

  const handleFilterChange = (field: keyof AuditLogFilters, value: any) => {
    setFilters({ ...filters, [field]: value, pageNumber: 1 });
  };

  const clearFilters = () => {
    setFilters({
      searchTerm: '',
      sortBy: 'Timestamp',
      sortDescending: true,
      pageNumber: 1,
      pageSize: 7,
    });
  };

  const handlePageChange = (newPage: number) => {
    if (newPage >= 1 && newPage <= totalPages) {
      setFilters({ ...filters, pageNumber: newPage });
    }
  };

  const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp);
    return date.toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getActionBadgeColor = (action: string) => {
    if (action.includes('Created')) return 'bg-green-100 text-green-800';
    if (action.includes('Updated')) return 'bg-blue-100 text-blue-800';
    if (action.includes('Deleted')) return 'bg-red-100 text-red-800';
    if (action.includes('Login')) return 'bg-purple-100 text-purple-800';
    if (action.includes('Password')) return 'bg-yellow-100 text-yellow-800';
    return 'bg-gray-100 text-gray-800';
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-purple-50 to-pink-50">
      <div className="max-w-[2000px] mx-auto px-3 sm:px-4 lg:px-6 py-4">
        {/* Header */}
        <div className="mb-4">
          <button
            onClick={() => navigate('/tasks')}
            className="flex items-center text-gray-600 hover:text-gray-900 mb-3 transition-colors text-sm"
          >
            <ArrowLeft className="w-4 h-4 mr-1" />
            Back to Tasks
          </button>
          <div className="flex items-center gap-4">
            <div className="bg-gradient-to-br from-purple-500 to-pink-600 p-3 rounded-xl shadow-lg">
              <FileText className="w-8 h-8 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold bg-gradient-to-r from-purple-600 to-pink-600 bg-clip-text text-transparent mb-1">
                Audit Logs
              </h1>
              <p className="text-sm text-gray-600 flex items-center gap-1.5">
                <span className="inline-block w-2 h-2 bg-blue-500 rounded-full animate-pulse"></span>
                View all system activity and changes
              </p>
            </div>
          </div>
        </div>

        {/* Search and Filter Bar */}
        <div className="bg-white rounded-lg shadow-md p-3 mb-3">
          <div className="flex flex-col md:flex-row gap-4">
            {/* Search */}
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <input
                type="text"
                placeholder="Search by user, action, or details..."
                value={filters.searchTerm || ''}
                onChange={(e) => handleSearchChange(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
              />
            </div>

            <div className="flex gap-2">
              {/* Refresh Button */}
              <button
                onClick={handleRefresh}
                className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
                title="Refresh audit logs"
              >
                <RotateCcw className="w-5 h-5" />
                <span className="hidden sm:inline">Refresh</span>
              </button>

              {/* Filter Toggle */}
              <button
                onClick={() => setShowFilters(!showFilters)}
                className={`flex items-center gap-2 px-4 py-2 rounded-lg transition-colors ${
                  showFilters
                    ? 'bg-purple-600 text-white'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                <Filter className="w-5 h-5" />
                Filters
              </button>
            </div>
          </div>

          {/* Advanced Filters */}
          {showFilters && (
            <div className="mt-4 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 pt-4 border-t">
              {/* User Email Filter */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  <User className="w-4 h-4 inline mr-1" />
                  User Email
                </label>
                <input
                  type="text"
                  placeholder="Filter by email..."
                  value={filters.userEmail || ''}
                  onChange={(e) => handleFilterChange('userEmail', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent text-sm"
                />
              </div>

              {/* Entity Type Filter */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  <FileText className="w-4 h-4 inline mr-1" />
                  Entity Type
                </label>
                <select
                  value={filters.entityType || ''}
                  onChange={(e) => handleFilterChange('entityType', e.target.value || undefined)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent text-sm"
                >
                  <option value="">All Types</option>
                  {entityTypes.map((type) => (
                    <option key={type} value={type}>
                      {type}
                    </option>
                  ))}
                </select>
              </div>

              {/* Action Filter */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Action</label>
                <select
                  value={filters.action || ''}
                  onChange={(e) => handleFilterChange('action', e.target.value || undefined)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent text-sm"
                >
                  <option value="">All Actions</option>
                  {actions.map((action) => (
                    <option key={action} value={action}>
                      {action}
                    </option>
                  ))}
                </select>
              </div>

              {/* Date From */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  <Calendar className="w-4 h-4 inline mr-1" />
                  Date From
                </label>
                <input
                  type="date"
                  value={filters.dateFrom || ''}
                  onChange={(e) => handleFilterChange('dateFrom', e.target.value || undefined)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent text-sm"
                />
              </div>

              {/* Date To */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  <Calendar className="w-4 h-4 inline mr-1" />
                  Date To
                </label>
                <input
                  type="date"
                  value={filters.dateTo || ''}
                  onChange={(e) => handleFilterChange('dateTo', e.target.value || undefined)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent text-sm"
                />
              </div>

              {/* Clear Filters */}
              <div className="flex items-end">
                <button
                  onClick={clearFilters}
                  className="w-full px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition-colors text-sm font-medium"
                >
                  Clear Filters
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Results Count */}
        <div className="mb-2 text-sm text-gray-600">
          Showing {logs.length > 0 ? ((filters.pageNumber - 1) * filters.pageSize) + 1 : 0} - {Math.min(filters.pageNumber * filters.pageSize, totalCount)} of {totalCount} audit logs
        </div>

        {/* Audit Logs Table */}
        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          {loading ? (
            <div className="flex justify-center items-center h-64">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600"></div>
            </div>
          ) : logs.length === 0 ? (
            <div className="text-center py-16">
              <FileText className="w-16 h-16 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">No audit logs found</h3>
              <p className="text-gray-500">Try adjusting your filters</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gradient-to-r from-purple-50 to-pink-50 border-b-2 border-purple-200">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-bold text-gray-700 uppercase tracking-wider">
                      Timestamp
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-bold text-gray-700 uppercase tracking-wider">
                      User
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-bold text-gray-700 uppercase tracking-wider">
                      Action
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-bold text-gray-700 uppercase tracking-wider">
                      Entity Type
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-bold text-gray-700 uppercase tracking-wider">
                      Details
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {logs.map((log) => (
                    <tr key={log.id} className="hover:bg-purple-50 transition-colors">
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-semibold text-gray-800">
                        {formatTimestamp(log.timestamp)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm font-bold text-gray-900">
                          {log.userName || 'System'}
                        </div>
                        <div className="text-sm font-medium text-gray-600">{log.userEmail}</div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span
                          className={`px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full ${getActionBadgeColor(
                            log.action
                          )}`}
                        >
                          {log.action}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-semibold text-gray-800">
                        {log.entityType}
                      </td>
                      <td className="px-6 py-4 text-sm font-medium text-gray-700 max-w-md truncate">
                        {log.details}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="sticky bottom-0 bg-white border-t border-gray-200 py-3 mt-3 flex items-center justify-center gap-2 shadow-lg rounded-t-lg flex-wrap">
            <button
              onClick={() => handlePageChange(1)}
              disabled={filters.pageNumber === 1}
              className="px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              First
            </button>
            <button
              onClick={() => handlePageChange(filters.pageNumber - 1)}
              disabled={filters.pageNumber === 1}
              className="flex items-center px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              <ChevronLeft className="w-5 h-5" />
              Previous
            </button>
            
            {/* Page numbers */}
            <div className="flex gap-1">
              {(() => {
                const pages = [];
                const showPages = 5; // Number of page buttons to show
                let startPage = Math.max(1, filters.pageNumber - Math.floor(showPages / 2));
                let endPage = Math.min(totalPages, startPage + showPages - 1);
                
                // Adjust start if we're near the end
                if (endPage - startPage < showPages - 1) {
                  startPage = Math.max(1, endPage - showPages + 1);
                }
                
                // Show first page if not in range
                if (startPage > 1) {
                  pages.push(
                    <button
                      key={1}
                      onClick={() => handlePageChange(1)}
                      className="px-3 py-2 border border-gray-300 rounded-lg hover:bg-purple-50 hover:border-purple-300 transition"
                    >
                      1
                    </button>
                  );
                  if (startPage > 2) {
                    pages.push(<span key="ellipsis1" className="px-2 py-2">...</span>);
                  }
                }
                
                // Show page numbers in range
                for (let i = startPage; i <= endPage; i++) {
                  pages.push(
                    <button
                      key={i}
                      onClick={() => handlePageChange(i)}
                      className={`px-3 py-2 border rounded-lg transition ${
                        filters.pageNumber === i
                          ? 'bg-purple-600 text-white border-purple-600'
                          : 'border-gray-300 hover:bg-purple-50 hover:border-purple-300'
                      }`}
                    >
                      {i}
                    </button>
                  );
                }
                
                // Show last page if not in range
                if (endPage < totalPages) {
                  if (endPage < totalPages - 1) {
                    pages.push(<span key="ellipsis2" className="px-2 py-2">...</span>);
                  }
                  pages.push(
                    <button
                      key={totalPages}
                      onClick={() => handlePageChange(totalPages)}
                      className="px-3 py-2 border border-gray-300 rounded-lg hover:bg-purple-50 hover:border-purple-300 transition"
                    >
                      {totalPages}
                    </button>
                  );
                }
                
                return pages;
              })()}
            </div>

            <button
              onClick={() => handlePageChange(filters.pageNumber + 1)}
              disabled={filters.pageNumber === totalPages}
              className="flex items-center px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              Next
              <ChevronRight className="w-5 h-5" />
            </button>
            <button
              onClick={() => handlePageChange(totalPages)}
              disabled={filters.pageNumber === totalPages}
              className="px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              Last
            </button>
          </div>
        )}
      </div>
    </div>
  );
};
