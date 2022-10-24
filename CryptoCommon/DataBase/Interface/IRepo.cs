using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataBase.Interface
{
    public interface IRepo<T>  where T : class, IIdAndName<T>
    {
        //string ConnectionStr { get; }
        Task<T> GetByIdAsync(int id, bool isEager = false);
        Task DeleteByIdAsync(int id);
        Task DeleteByIdsAsync(params int[] ids);
        Task AddAsync(T item);
        Task UpdateAsync(T item);
        Task<List<T>> GetAllAsync(Expression<Func<T, object>> navigationPropertyPath = null, Expression<Func<T, bool>> predicate = null);
        Task<List<int>> GetAllIdsAsync();
        Task<T> GetByNameAsync(string name, Expression<Func<T, object>> navigationPropertyPath = null);
        //Task AddUpdateAsync(T item, bool isCheckName=true);
    }
}
