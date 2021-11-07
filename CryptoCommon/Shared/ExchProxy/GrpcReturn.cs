using CryptoCommon.Grpc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    //public class GrpcReturn<T>
    //{
    //    public int Code { get; set; }
    //    public string Msg { get; set; }
    //    public T Data { get; set; }

    //    public GrpcReturn(int errorCode, string errorMessage = "", T data = default)
    //    {
    //        Code = errorCode;
    //        Msg = errorMessage;
    //        Data = data;
    //    }

    //    public GrpcReturn(GrpcMsg msg)
    //    {
    //        this.Code = msg.Code;
    //        this.Msg = msg.Msg;
    //        this.Data = string.IsNullOrEmpty(msg.Data) ? default(T) : JsonConvert.DeserializeObject<T>(msg.Data);
    //    }
    //}
}
