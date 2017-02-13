using ImportRenewals.Contexts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
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

        public void IgnoreEntry<T>(T entity) where T : class
        {
            int keyValue = int.Parse(GetKeyValue<T>(entity).ToString());
            if (entity != null && keyValue > 0)
            {
                try
                {
                    DbContext.Entry(entity).State = EntityState.Unchanged;
                }
                catch
                {
                    DbContext.Entry(entity).State = EntityState.Added;
                }
            }

            
        }

        private object GetKeyValue<T>(T entity) where T : class
        {
            PropertyInfo key =
                typeof(T)
                .GetProperties()
                .FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Length != 0);

            return key != null ? key.GetValue(entity, null) : null;
        }
    }
}