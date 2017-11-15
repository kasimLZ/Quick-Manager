using Database.Base.Interface;
using Database.Base.Interface.Infrastructure;
using Database.Base.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Database.Base.Service.Infrastructure
{
    public abstract class RepositoryBase<T> where T : class
    {
        // Fields
        private readonly IDatabaseFactory _databaseFactory;
        private readonly IApplicationDb _dataContext;
        private readonly DbSet<T> _dbset;
        private readonly CurrentUserInterface _user;

        protected RepositoryBase(IDatabaseFactory databaseFactory, CurrentUserInterface user)
        { 
            this._databaseFactory = databaseFactory;
            this._dataContext = databaseFactory.DataContext;
            this._dbset = this._dataContext.Set<T>();
            this._user = user;
        }

        public virtual void Add(T entity)
        {
            IDbSetBase base2 = entity as IDbSetBase;
            if (base2 == null)
            {
                this._dbset.Add(entity);
                return;
            }
            base2.CreatedDate = DateTime.Now;
            this._dbset.Add(base2 as T);
        }
        
        public int Commit()
        {
            return this._dataContext.Commit();
        }

        public Task<int> CommitAsync()
        {
            return this._dataContext.CommitAsync();
        }

        public virtual void Delete(long id)
        {
            T byId = this.GetById(id);
            this.Delete(byId);
        }

        public virtual void Delete(T item)
        {
            IDbSetBase base2 = item as IDbSetBase;
            if (base2 != null)
            {
                base2.Deleted = true;
            }
        }

        public virtual void Delete(Expression<Func<T, bool>> where)
        {
            foreach (T local in GetAll(where))
            {
                this.Delete(local);
            }
        }

        public virtual IQueryable<T> GetAll()
        {
            return GetAll(false);
        }

        public virtual IQueryable<T> GetAll(bool deleted)
        {
            ParameterExpression expression = Expression.Parameter(typeof(T), "a");
            ParameterExpression[] parameters = new ParameterExpression[] { expression };
            Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Property(expression, "Deleted"), Expression.Constant(deleted)), parameters);
            ParameterExpression[] expressionArray2 = new ParameterExpression[] { expression };
            Expression<Func<T, DateTime>> keySelector = Expression.Lambda<Func<T, DateTime>>(Expression.Property(expression, "CreatedDate"), expressionArray2);
            return _dbset.Where(predicate).OrderByDescending(keySelector);
        }

        public virtual IQueryable<T> GetAll(Expression<Func<T, bool>> where)
        {
            return this.GetAll().Where(where);
        }

        public virtual T GetById(long id)
        {
            object[] keyValues = new object[] { id };
            return this._dbset.Find(keyValues);
        }
        

        public virtual void Remove(T item)
        {
            this._dbset.Remove(item);
        }

        public virtual void Remove(Expression<Func<T, bool>> where)
        {
            foreach (T local in this.GetAll(where))
            {
                this.Remove(local);
            }
        }

        public virtual void Save(long? id, T entity)
        {
            IDbSetBase base2 = entity as IDbSetBase;
            if (base2 == null)
            {
                if (id.HasValue)
                {
                    this.Update(entity);
                }
                else
                {
                    this.Add(entity);
                }
                return;
            }
            if (!id.HasValue)
            {
                this.Add(base2 as T);
                return;
            }
            base2.CreatedDate = DateTime.Now;
            this.Update(base2 as T);
        }


        public virtual void Update(T entity)
        {
            this._dbset.Attach(entity);
            this._dataContext.Entry<T>(entity).State = EntityState.Modified;
            (entity as IDbSetBase).UpdatedDate = DateTime.Now;
            if (this._dataContext.Entry<T>(entity).Property("CreatedDate") != null)
            {
                this._dataContext.Entry<T>(entity).Property("CreatedDate").IsModified = false;
            }

        }
    }
}
