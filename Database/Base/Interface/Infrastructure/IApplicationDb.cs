using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Database.Base.Interface.Infrastructure
{
    public interface IApplicationDB : IDisposable
    {
        int Commit();
        Task<int> CommitAsync();
        EntityEntry Entry(object entity);
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        int SaveChanges();
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}
