using ImportRenewals.Contexts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ImportRenewals.Repositories
{
    public class GenericRepository<TEntity> : IDisposable where TEntity : class
    {
        protected PrionContext DbContext;

        public GenericRepository(PrionContext _DbContext)
        {
            this.DbContext = _DbContext;
        }

        public virtual void Add(TEntity entity)
        {
            DbContext.Set<TEntity>().Add(entity);
            DbContext.SaveChanges();
        }

        public virtual void Update(TEntity entity)
        {
            DbContext.Entry(entity).State = EntityState.Modified;
            DbContext.SaveChanges();
        }

        public TEntity Find(int id)
        {
            return DbContext.Set<TEntity>().Find(id);
        }

        public virtual IEnumerable<TEntity> ListAll()
        {
            return DbContext.Set<TEntity>().ToList();
        }

        public virtual void Remove(TEntity entity)
        {
            DbContext.Set<TEntity>().Remove(entity);
            DbContext.SaveChanges();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}