using Microsoft.EntityFrameworkCore.Query;
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
        Task<int> DeleteByIdAsync(int id);
        Task<int> DeleteByIdsAsync(params int[] ids);
        Task<int> DeleteByNamesAsync(params string[] names);
        Task AddAsync(T item);
        Task AddItemsAsync(params T[] items);
        Task UpdateAsync(T item);
        Task UpdateByIdAsync(int id, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls);
        Task UpdateByNameAsync(string name, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls);
        Task<List<T>> GetAllAsync(Expression<Func<T, object>> navigationPropertyPath = null, Expression<Func<T, bool>> predicate = null);
        Task<List<int>> GetAllIdsAsync();
        Task<T> GetByNameAsync(string name, Expression<Func<T, object>> navigationPropertyPath = null);
        //Task AddUpdateAsync(T item, bool isCheckName=true);
    }
}
