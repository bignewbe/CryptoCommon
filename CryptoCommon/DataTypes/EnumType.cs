using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataTypes
{
    public enum OrderType
    {
        none,
        buy_limit,
        buy_market,
        sell_limit,
        sell_market
    }

    public enum OrderStatus
    {
        none,
        cancel_pending,
        pending,
        open,
        closed,
        partial_closed,
        cancelled,
        expired,
        failed,
        not_exist
    }

    //okCoin: （-3:撤销中;-2:已撤销;-1:失败;0:等待提现;1:提现中;2:已汇出;3:邮箱确认;4:人工审核中5:等待身份认

    //Kraken status:
    //cancel-pending = cancelation requested
    //canceled = canceled
    //cancel-denied = cancelation requested but was denied  => failed
    //return = a return transaction initiated by Kraken; it cannot be canceled => transferred
    //onhold = withdrawal is on hold pending review => confirm_needed
    public enum FundingStatus
    {
        none,
        deposit_failed,
        deposit_wait_confirm,
        deposit_succeed,

        withdraw_cancel_denied,
        withdraw_cancel_pending,
        withdraw_canceled,
        withdraw_failed,          //cancel denied, failed, etc
        withdraw_initial,
        withdraw_pending,         //waitting to be transferred
        withdraw_transferred,     //done
        withdraw_confirm_needed,  //need further confirmation
        withdraw_wait_identity_verification
    }

    public enum FundingType
    {
        none,
        deposit,
        withdraw
    }

    public enum EnumInterval
    {
        one_miniute,
        five_minitue,
        fifteen_minute,
        one_hour,
        day
    }
}
