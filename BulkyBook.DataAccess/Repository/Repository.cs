using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {

        private readonly ApplicationDbContext rdb;
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
                rdb= db;
            this.dbSet = rdb.Set<T>(); 

        }

        public void Add(T entity)
        {
           dbSet.Add(entity);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? pro = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            { 
                query = query.Where(filter);
            }
            if (pro != null)
            {
                foreach (var includepro in pro.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {

                    query = query.Include(includepro);
                   
                }
            }

            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter , string? pro = null ,bool tracked = true)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query  = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();
            }
            query = query.Where(filter);
            if (pro != null)
            {
                foreach (var includepro in pro.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {

                    query = query.Include(includepro);
                }
            }

            return query.FirstOrDefault();

        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
