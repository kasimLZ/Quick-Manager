using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Database.Base.Interface.Infrastructure
{
    public interface IRepository<T> where T : class
    {
        // Methods
        void Add(T entity);
        int Commit();
        Task<int> CommitAsync();
        void Delete(long id);
        void Delete(T item);
        void Delete(Expression<Func<T, bool>> where);
        IQueryable<T> GetAll();
        IQueryable<T> GetAll(bool deleted);
        IQueryable<T> GetAll(Expression<Func<T, bool>> where);
        T GetById(long id);
        void Remove(T item);
        void Remove(Expression<Func<T, bool>> where);
        void Save(long? id, T entity);
        void Update(T entity);
    }
}
