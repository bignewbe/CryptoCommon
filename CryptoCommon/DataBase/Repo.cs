using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CryptoCommon.DataBase.Interface;
using Microsoft.EntityFrameworkCore;
using PortableCSharpLib;

namespace CryptoCommon.DataBase
{

    public class Repo<T> : IRepo<T>, IDisposable where T : class, IIdAndName<T>, new()
    {
        //private ApplicationDbContext _dbContext;
        //private string _connectionStr;
        //private DbContextOptions _options;
        private DbContext _dbContext;

        //private string _tableName;
        //private DbSet<T> _table;
        //public Repo(ApplicationDbContext applicationDbContext)
        public Repo(DbContext dbContext)
        {
            _dbContext = dbContext;

            //_connectionStr = connectionStr;
            //_options = new DbContextOptionsBuilder().UseSqlServer(_connectionStr).Options;
            //_dbContext = new ApplicationDbContext(_options);
            //_table = _dbContext.Set<T>();

            //_dbContext = applicationDbContext;
            //if (tableName == "Cameras")
            //    _table = _dbContext.Set<Camera>();
            ////_table = (DbSet<T>)Convert.ChangeType(_dbContext.Cameras, typeof(DbSet<T>));
            ////_table = (DbSet<T>)Convert.ChangeType(_dbContext.Cameras, typeof(DbSet<T>));
            //else
            //    throw new Exception($"{tableName} is not supported");
        }

        /// <summary>
        /// no id should be specifed for both parent and child entity. otherwise, exception will be raised.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task AddAsync(T item)
        {
            if (item == null)
                throw new MyException("ApiStatusCode.ARGUMENT_NULL_ERROR", "error");
            if (item.Id > 0)
                throw new MyException("ApiStatusCode.IdAlreadyExistError", "cannot add entity when id already exist");

            //using (var _dbContext = new ApplicationDbContext(_options))
            {
                var _table = _dbContext.Set<T>();
                await _table.AddAsync(item);
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// suppose we are operating a
        ///    navigation: a -> b, i.e., a contains id of b
        ///    reference:  a <- b, i.e., b contains id of a 
        /// in case of navigation:
        ///    - b will be added if no id is given
        ///    - b will be updated if valid id is given
        ///    - error when id is invalid
        /// in case of reference (overall replace):
        /// - existing b with valid id given will be updated 
        /// - existing b without id given will be removed
        /// - error for invalid id
        /// - new b without id given will be added        
        /// </summary>
        /// <param name="item"></param>
        /// <returns>
        /// </returns>
        public async Task UpdateAsync(T item)
        {
            if (item == null)
                throw new MyException("ApiStatusCode.ARGUMENT_NULL_ERROR", "error");

            if (item.Id <= 0)
                throw new MyException("ApiStatusCode.IdNotGivenWhenUpdateEntity", "cannot update entity if id not given");

            //using (var _dbContext = new ApplicationDbContext(_options))
            {
                _dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                var _table = _dbContext.Set<T>();
                var c = await _table.FindAsync(item.Id);
                if (c == null)
                    throw new MyException("ApiStatusCode.Error_EntityNotExist", "cannot update entity if entity not in table");
                _table.Update(item);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteByIdAsync(int id)
        {
            //using (var _dbContext = new ApplicationDbContext(_options))
            {
                var _table = _dbContext.Set<T>();
                var c = await _table.FindAsync(id);
                _table.Remove(c);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, object>> navigationPropertyPath = null, Expression<Func<T, bool>> predicate = null)
        {
            //using (var _dbContext = new ApplicationDbContext(_options))
            {
                var _table = _dbContext.Set<T>();

                if (navigationPropertyPath == null)
                {
                    if (predicate == null)
                        return await _table.ToListAsync();
                    else
                        return await _table.Where(predicate).ToListAsync();
                }
                else
                {
                    var query = _table.Include(navigationPropertyPath);
                    if (predicate != null)
                        return await query.Where(predicate).ToListAsync();
                    else
                        return await query.ToListAsync();
                }
            }
        }

        public async Task<List<T>> GetAllAsync(bool isEager)
        {
            //using (var _dbContext = new ApplicationDbContext(_options))
            {
                var query = Query(_dbContext, isEager);
                return await query.ToListAsync();
            }
        }

        public async Task<List<int>> GetAllIdsAsync()
        {
            //using (var _dbContext = new ApplicationDbContext(_options))
            {
                var _table = _dbContext.Set<T>();
                return await _table.Select(c => c.Id).ToListAsync();
            }
        }


        public IQueryable<T> Query(DbContext _dbContext, bool eager = false)
        {
            var query = _dbContext.Set<T>().AsQueryable();
            if (eager)
            {
                var navigations = _dbContext.Model.FindEntityType(typeof(T))
                    .GetDerivedTypesInclusive()
                    .SelectMany(type => type.GetNavigations())
                    .Distinct();

                foreach (var property in navigations)
                    query = query.Include(property.Name);
            }
            return query;
        }

        public async Task<T> GetByIdAsync(int id, bool isEager = false)
        {
            //using (var _dbContext = new ApplicationDbContext(_options))
            {
                var query = Query(_dbContext, isEager);
                return await query.SingleOrDefaultAsync(t => t.Id == id);
            }
        }

        public async Task<T> GetByNameAsync(string name, Expression<Func<T, object>> navigationPropertyPath = null)
        {
            //using (var _dbContext = new ApplicationDbContext(_options))
            {
                var _table = _dbContext.Set<T>();

                if (navigationPropertyPath == null)
                    return await _table.SingleOrDefaultAsync(o => o.Name.Equals(name));
                else
                    return await _table.Include(navigationPropertyPath).SingleOrDefaultAsync(o => o.Name.Equals(name));
            }
        }

        public void Dispose()
        {
        }
    }
}
