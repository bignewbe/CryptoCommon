using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptoCommon.DataBase.Interface;
using LogInterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using PortableCSharpLib;

namespace CryptoCommon.DataBase
{
    public class Repo<T1, T2> : IRepo<T1> where T1 : class, IIdAndName<T1>, new() where T2: DbContext
    {
        //private ApplicationDbContext _dbContext;
        //private DbContextOptions _options;
        //private DbContext _dbContext;
        //private string _tableName;
        //private DbSet<T> _table;
        //public Repo(ApplicationDbContext applicationDbContext)

        //public string ConnectionStr { get; private set; }
        private Func<DbContext> _funcCreateDbContext;
        //public Repo(Func<DbContext> funcCreateDbContext)
        //{
        //    //this.ConnectionStr = connectionStr;
        //    _funcCreateDbContext = funcCreateDbContext;

        //    ////make sure database created
        //    //using (var dbContext = _funcCreateDbContext(ConnectionStr))
        //    //{
        //    //    dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        //    //    dbContext.Database.EnsureCreated();
        //    //}

        //    //_dbContext = dbContext;
        //    //_connectionStr = connectionStr;
        //    //_options = new DbContextOptionsBuilder().UseSqlServer(_connectionStr).Options;
        //    //_dbContext = new ApplicationDbContext(_options);
        //    //_table = _dbContext.Set<T>();

        //    //_dbContext = applicationDbContext;
        //    //if (tableName == "Cameras")
        //    //    _table = _dbContext.Set<Camera>();
        //    ////_table = (DbSet<T>)Convert.ChangeType(_dbContext.Cameras, typeof(DbSet<T>));
        //    ////_table = (DbSet<T>)Convert.ChangeType(_dbContext.Cameras, typeof(DbSet<T>));
        //    //else
        //    //    throw new Exception($"{tableName} is not supported");
        //}

        public Repo(string connectionStr, bool isCheckDbContext=true)
        {
            _funcCreateDbContext = () =>
            {
                var options = new DbContextOptionsBuilder().UseSqlServer(connectionStr).Options;
                var dbContext = Activator.CreateInstance(typeof(T2), options) as DbContext;
                return dbContext;
            };

            //check dbContext
            if (isCheckDbContext)
            {
                using (var dbContext = _funcCreateDbContext())
                {
                    //Log.Information("=== check DbContext ===");
                    var count = 0;
                    var isValid = false;
                    while (count++ < 20)
                    {
                        try
                        {
                            dbContext.Database.EnsureCreated();
                            isValid = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(1000);
                        }
                    };
                    if (!isValid)
                    {
                        //Log.Information("=== DbContext check failed ===");
                        throw new MyException("dbError", "failed to connect to dababase after 20 seconds");
                    }
                    //Log.Information("=== DbContext check succeeded ===");
                };
            }
        }

        //public bool CheckDbContextValid()
        //{
        //    using (var _dbContext = _funcCreateDbContext())
        //    {
        //        var count = 0;
        //        var isValid = false;
        //        while (!isValid && count++ < 20)
        //        {
        //            try
        //            {
        //                isValid = _dbContext.Database.EnsureCreated();
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine(DateTime.Now);
        //                Thread.Sleep(1000);
        //            }
        //        };
        //        return isValid;
        //    }
        //}

        /// <summary>
        /// no id should be specifed for both parent and child entity. otherwise, exception will be raised.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task AddAsync(T1 item)
        {
            if (item == null)
                throw new MyException("ApiStatusCode.ARGUMENT_NULL_ERROR", "error");
            if (item.Id > 0)
                throw new MyException("ApiStatusCode.IdAlreadyExistError", "cannot add entity when id already exist");

            using (var _dbContext = _funcCreateDbContext())
            {
                var _table = _dbContext.Set<T1>();
                await _table.AddAsync(item);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task AddItemsAsync(params T1[] items)
        {
            if (items == null || items.Length == 0) return;
                    
            using (var _dbContext = _funcCreateDbContext())
            {
                var _table = _dbContext.Set<T1>();
                foreach(var item in items)
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
        public async Task UpdateAsync(T1 item)
        {
            if (item == null)
                throw new MyException("ApiStatusCode.ARGUMENT_NULL_ERROR", "error");

            if (item.Id <= 0)
                throw new MyException("ApiStatusCode.IdNotGivenWhenUpdateEntity", "cannot update entity if id not given");

            using (var _dbContext = _funcCreateDbContext())
            {
                _dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                var _table = _dbContext.Set<T1>();
                var c = await _table.FindAsync(item.Id);
                if (c == null)
                    throw new MyException("ApiStatusCode.Error_EntityNotExist", "cannot update entity if entity not in table");
                _table.Update(item);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateByIdAsync(int id, Expression<Func<SetPropertyCalls<T1>, SetPropertyCalls<T1>>> setPropertyCalls)
        {
            if (id <= 0)
                throw new MyException("UpdateByIdAsync", "cannot update entity if id not given");

            using (var _dbContext = _funcCreateDbContext())
            {
                var _table = _dbContext.Set<T1>();
                await _table.Where(x => x.Id == id).ExecuteUpdateAsync(setPropertyCalls);
            }
        }
        public async Task UpdateByNameAsync(string name, Expression<Func<SetPropertyCalls<T1>, SetPropertyCalls<T1>>> setPropertyCalls)
        {
            if (string.IsNullOrEmpty(name))
                throw new MyException("UpdateByNameAsync", "name cannot be null");

            using (var _dbContext = _funcCreateDbContext())
            {
                var _table = _dbContext.Set<T1>();
                await _table.Where(x => x.Name == name).ExecuteUpdateAsync(setPropertyCalls);
            }
        }

        public async Task<int> DeleteByIdAsync(int id) => await this.DeleteByIdAsync(id);

        public async Task<int> DeleteByIdsAsync(params int[] ids)
        {
            if (ids.Length == 0) 
                return 0;

            using (var _dbContext = _funcCreateDbContext())
            {
                var _table = _dbContext.Set<T1>();
                var rows = _table.Where(x => ids.Contains(x.Id)).ExecuteDelete();
                return await Task.FromResult(rows);
                //foreach (var id in ids)
                //{
                //    var c = await _table.FindAsync(id);
                //    _table.Remove(c);
                //}
                //await _dbContext.SaveChangesAsync();
            }
        }
        public async Task<int> DeleteByNamesAsync(params string[] names)
        {
            if (names.Length == 0)
                return 0;

            using (var _dbContext = _funcCreateDbContext())
            {
                var _table = _dbContext.Set<T1>();
                var rows = _table.Where(x => names.Contains(x.Name)).ExecuteDelete();
                return await Task.FromResult(rows);
            }
        }

        public async Task<List<T1>> GetAllAsync(Expression<Func<T1, object>> navigationPropertyPath = null, Expression<Func<T1, bool>> predicate = null)
        {
            using (var _dbContext = _funcCreateDbContext())
            {
                var _table = _dbContext.Set<T1>();

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

        public async Task<List<T1>> GetAllAsync(bool isEager)
        {
            using (var _dbContext = _funcCreateDbContext())
            {
                var query = Query(_dbContext, isEager);
                return await query.ToListAsync();
            }
        }

        public async Task<List<int>> GetAllIdsAsync()
        {
            using (var _dbContext = _funcCreateDbContext())
            {
                var _table = _dbContext.Set<T1>();
                return await _table.Select(c => c.Id).ToListAsync();
            }
        }

        private IQueryable<T1> Query(DbContext _dbContext, bool eager = false)
        {
            var query = _dbContext.Set<T1>().AsQueryable();
            if (eager)
            {
                var navigations = _dbContext.Model.FindEntityType(typeof(T1))
                    .GetDerivedTypesInclusive()
                    .SelectMany(type => type.GetNavigations())
                    .Distinct();

                foreach (var property in navigations)
                    query = query.Include(property.Name);
            }
            return query;
        }

        public async Task<T1> GetByIdAsync(int id, bool isEager = false)
        {
            using (var _dbContext = _funcCreateDbContext())
            {
                var query = Query(_dbContext, isEager);
                return await query.SingleOrDefaultAsync(t => t.Id == id);
            }
        }

        //navigationPropertyPath indicates whether to include related object
        public async Task<T1> GetByNameAsync(string name, Expression<Func<T1, object>> navigationPropertyPath = null)
        {
            using (var _dbContext = _funcCreateDbContext())
            {
                var _table = _dbContext.Set<T1>();

                if (navigationPropertyPath == null)
                    return await _table.SingleOrDefaultAsync(o => o.Name.Equals(name));
                else
                    return await _table.Include(navigationPropertyPath).SingleOrDefaultAsync(o => o.Name.Equals(name));
            }
        }
    }
}
