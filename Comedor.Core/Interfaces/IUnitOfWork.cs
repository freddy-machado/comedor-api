using Comedor.Core.Entities;

namespace Comedor.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IComensalRepository Comensales { get; }
    IGenericRepository<Turno> Turnos { get; }
    IDespachoRepository Despachos { get; }
    IGenericRepository<Log> Logs { get; }

    Task<int> SaveAsync();
}