using System;
using System.Threading.Tasks;

namespace CryptoCommon.DataTypes
{
    public class ServiceResult<T>
    {
        public static async Task<ServiceResult<T>> CallAsyncFunction(Func<Task<T>> func)
        {
            try
            {
                var r = await func();
                return new ServiceResult<T> { Result = true, Data = r };
            }
            catch (Exception ex)
            {
                var r = new ServiceResult<T> { Result = false, Message = ex.ToString() };
                Console.WriteLine(r.Message);
                return r;
            }
        }

        public bool Result { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
