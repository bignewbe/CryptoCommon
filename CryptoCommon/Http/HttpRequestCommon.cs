using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Facility
{
    public class HttpRequestCommon : IHttpRequestCommon
    {
        protected HttpClientHandler _handler;
        protected CookieContainer _cookieContainer;
        protected HttpClient _client;

        public HttpRequestCommon()
        {
            _cookieContainer = new CookieContainer();
            _handler = new HttpClientHandler();
            _handler.CookieContainer = _cookieContainer;
            _client = new HttpClient(_handler);
        }

        public void AddHeader(string key, string value)
        {
            //throw new NotImplementedException();
            _client.DefaultRequestHeaders.Add(key, value);
        }

        public async Task<HttpResponseMessage> GetResponseWithHeaderReturnedAsync(string url, int timeout = 5000)
        {
            var task1 = _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (await Task.WhenAny(task1, Task.Delay(timeout)) == task1)
            {
                var response = task1.Result;
                response.EnsureSuccessStatusCode();
                return response;
            }
            else
            {
                throw new TimeoutException("Timeout occur in GetResponseWithHeaderReturnedAsync");
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string url, int timeout = 5000)
        {
            var task = _client.GetAsync(url);
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task) {
                var response = task.Result;
                response.EnsureSuccessStatusCode();
                return response;
            }
            else {
                throw new TimeoutException("Timeout occur in GetAsync");
            }
        }

        public async Task<HttpResponseMessage> GetWithParamAsync(string url, string paramName, string paramValue, int timeout = 5000)
        {
            url = string.Format("{0}?{1}={2}", url, paramName, paramValue);
            var task = _client.GetAsync(url);
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                var response = task.Result;
                response.EnsureSuccessStatusCode();
                return response;
            }
            else
            {
                throw new TimeoutException("Timeout occur in GetWithParamAsync");
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string url, string postData, int timeout = 5000)
        {
            var task = _client.PostAsync(url, new StringContent(postData, Encoding.UTF8, "application/json"));
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task) {
                var response = task.Result;
                response.EnsureSuccessStatusCode();
                return response;
            }
            else {
                throw new TimeoutException("Timeout occur in PostAsync");
            }
        }

        public async Task<HttpResponseMessage> PostFormAsync(string url, IEnumerable<KeyValuePair<string, string>> formData, int timeout = 5000)
        {
            var task = _client.PostAsync(url, new FormUrlEncodedContent(formData));
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                var response = task.Result;
                response.EnsureSuccessStatusCode();
                return response;
            }
            else
            {
                throw new TimeoutException("Timeout occur in PostFormAsync");
            }
        }
    }
}
