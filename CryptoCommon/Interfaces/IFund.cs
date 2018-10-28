using CryptoCommon.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IFund
    {
        string Exchange { get; }

        Task<Assets> GetAssets(int timeout = 5000);
        Task<string> Withdraw(string currency, double amount, string address, int timeout = 5000);

        //Task<Funding> CheckWithdraw(string withdrawId, int timeout = 5000);
        
        //Task<bool> CancelWithdraw(string withdrawId, int timeout = 5000);
        //Task<List<Funding>> GetDepositRecords(string currency, int timeout = 5000);
        //Task<List<Funding>> GetWithdrawRecords(string currency, int timeout = 5000);
        
        //bool IsStarted { get; }
        //Task Start(int miliseconds);
        //void Stop();
        //event EventHandlers.CaptureStateChangedHandler OnCaptureStateChanged;
        //event EventHandlers.DepositAddedEventHandler OnNewDeposit;
        //event EventHandlers.DepositStatusChangedEventHandler OnDepositStatusChanged;
        //event EventHandlers.WithdrawAddeddEventHandler OnNewWithraw;
        //event EventHandlers.WithdrawStatusChangedEventHandler OnWithdrawStatusChanged;
    }
}
