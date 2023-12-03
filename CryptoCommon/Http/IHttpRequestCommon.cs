using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CryptoCommon.Facility
{
    public interface IHttpRequestCommon
    {
        Task<HttpResponseMessage> GetResponseWithHeaderReturnedAsync(string url, int timeout = 5000);
        Task<HttpResponseMessage> GetAsync(string url, int timeout = 5000);
        Task<HttpResponseMessage> GetWithParamAsync(string url, string paramName, string paramValue, int timeout = 5000);
        Task<HttpResponseMessage> PostAsync(string url, string postData, int timeout = 5000);
        Task<HttpResponseMessage> PostFormAsync(string url, IEnumerable<KeyValuePair<string, string>> formData, int timeout = 5000);
        void AddHeader(string key, string value); 
    }
}