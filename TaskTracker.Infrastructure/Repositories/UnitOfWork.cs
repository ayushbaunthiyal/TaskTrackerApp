using Microsoft.EntityFrameworkCore.Storage;
using TaskTracker.Application.Interfaces.Repositories;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public ITaskRepository Tasks { get; }
    public IUserRepository Users { get; }
    public IAuditLogRepository AuditLogs { get; }
    public IAttachmentRepository Attachments { get; }
    public IRefreshTokenRepository RefreshTokens { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        ITaskRepository taskRepository,
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        IAttachmentRepository attachmentRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _context = context;
        Tasks = taskRepository;
        Users = userRepository;
        AuditLogs = auditLogRepository;
        Attachments = attachmentRepository;
        RefreshTokens = refreshTokenRepository;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
