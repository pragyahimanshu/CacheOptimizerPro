using System.Data;

namespace Domain.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    IDbTransaction BeginTransaction();
}