// PDV.Infrastructure/Data/UnitOfWork.cs
using Microsoft.EntityFrameworkCore;
using PDV.Domain.Entities;
using PDV.Domain.Interfaces;
using PDV.Infrastructure.Data.Contexts;
using PDV.Infrastructure.Data.Repositories;

namespace PDV.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LocalDbContext _context;
        private bool disposed = false;

        public IRepository<Product> Products { get; private set; }
        public IRepository<Employee> Employees { get; private set; }

        public UnitOfWork(LocalDbContext context)
        {
            _context = context;
            Products = new Repository<Product>(_context);
            Employees = new Repository<Employee>(_context);
        }

        public async Task<bool> CompleteAsync()
        {
            // Marcar entidades modificadas para sincronização
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("SyncState").CurrentValue = "New";
                    entry.Property("LastSyncTimestamp").CurrentValue = DateTime.MinValue;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property("SyncState").CurrentValue = "Modified";
                }
                else if (entry.State == EntityState.Deleted)
                {
                    entry.Property("SyncState").CurrentValue = "Deleted";
                    entry.State = EntityState.Modified; // Não exclui, apenas marca como deletado
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}