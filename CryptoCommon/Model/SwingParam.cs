using Newtonsoft.Json;
using PortableCSharpLib.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CryptoCommon.DataTypes
{
    //public class GivenIdConfig
    //{
    //    //public List<SymbolDigits> Digits { get; set; }
    //    public Dictionary<int, string> IntervalMap { get; set; }
    //    public Dictionary<EnumType, string> TradeTypeMap { get; set; }
    //    public Dictionary<EnumType, string> TradeModelMap { get; set; }
    //    //var d = config.Digits.ToDictionary(t => t.Symbol, t => t);

    //    //Dictionary<int, string> _IndexToSymbolMap = new Dictionary<int, string>();
    //    //public Dictionary<int, string> IndexToSymbolMap
    //    //{
    //    //    get
    //    //    {
    //    //        if (_IndexToSymbolMap.Count > 0)
    //    //            return _IndexToSymbolMap;
    //    //        if (Digits != null)
    //    //            _IndexToSymbolMap = this.Digits.ToDictionary(t => t.Index, t => t.Symbol);
    //    //        return _IndexToSymbolMap;
    //    //    }
    //    //}

    //    Dictionary<string, int> _ReverseIntervalMap = new Dictionary<string, int>();
    //    public Dictionary<string, int> ReverseIntervalMap
    //    {
    //        get
    //        {
    //            if (_ReverseIntervalMap.Count > 0)
    //                return _ReverseIntervalMap;
    //            if (IntervalMap != null)
    //            {
    //                foreach (var k in IntervalMap.Keys)
    //                    _ReverseIntervalMap.Add(IntervalMap[k], k);
    //            }
    //            return _ReverseIntervalMap;
    //        }
    //    }

    //    Dictionary<string, EnumType> _ReverseTradeModelMap = new Dictionary<string, EnumType>();
    //    public Dictionary<string, EnumType> ReverseTradeModelMap
    //    {
    //        get
    //        {
    //            if (_ReverseTradeModelMap.Count > 0)
    //                return _ReverseTradeModelMap;
    //            if (TradeModelMap != null)
    //            {
    //                foreach (var k in TradeModelMap.Keys)
    //                    _ReverseTradeModelMap.Add(TradeModelMap[k], k);
    //            }
    //            return _ReverseTradeModelMap;
    //        }
    //    }

    //    Dictionary<string, EnumType> _ReverseTradeTypeMap = new Dictionary<string, EnumType>();
    //    public Dictionary<string, EnumType> ReverseTradeTypeMap
    //    {
    //        get
    //        {
    //            if (_ReverseTradeTypeMap.Count > 0)
    //                return _ReverseTradeTypeMap;
    //            if (TradeTypeMap != null)
    //            {
    //                foreach (var k in TradeTypeMap.Keys)
    //                    _ReverseTradeTypeMap.Add(TradeTypeMap[k], k);
    //            }
    //            return _ReverseTradeTypeMap;
    //        }
    //    }
    //}

    //public class SymbolDigits
    //{
    //    public string Symbol { get; set; }
    //    public int Digits { get; set; }
    //    public int Index { get; set; }
    //    public SymbolDigits(string symbol, int digits, int index)
    //    {
    //        this.Symbol = symbol;
    //        this.Digits = digits;
    //        this.Index = index;
    //    }
    //}

    public class SwingParam : IIdEqualCopy<SwingParam> //EqualAndCopyUseReflection<SwingParam>, 
    {
        public static (string c1, string c2) GetCryptoFromSymbol(string symbol)
        {
            var sp = symbol.Contains("SWAP") ? "-" : "_";
            var c1 = symbol.Split(sp)[0];
            var c2 = symbol.Split(sp)[1];
            return (c1, c2);
        }

        public static InstrumentType GetInstTypeFromSymbol(string symbol)
        {
            if (symbol.Contains("SWAP"))
                return InstrumentType.SWAP;

            if (symbol.Split('_').Length == 3 || symbol.Split('-').Length == 3)
                return InstrumentType.FUTURES;

            return InstrumentType.SPOT;
        }

        public static string CreateRefId(string symbol, int interval, EnumType tradeType, EnumType tradeMode)
        {
            //if (symbol.Contains("YFII"))
            //    Console.WriteLine();

            var instType = GetInstTypeFromSymbol(symbol);
            var r = GetCryptoFromSymbol(symbol);

            var instTypeStr = instType == InstrumentType.SPOT ? "p" : "m";
            var baseCcyStr = r.c1.ToLower();
            if (baseCcyStr.Length > 4)
                baseCcyStr = baseCcyStr.Substring(0, 4);

            var quoteCcyStr = r.c2 switch
            {
                "USDT" => "u",
                "BNB" => "b",
                "HT" => "h",
                _ => "x"
            };
            var intervalStr = interval switch
            {
                60 => "a",
                180 => "b",
                300 => "c",
                900 => "d",
                1800 => "i",
                3600 => "e",
                7200 => "f",
                14400 => "g",
                86400 => "h",
                _ => "x",
            };
            var tradeTypeStr = tradeType switch
            {
                EnumType.TraderShoot => "a",
                //EnumType.TraderTurn => "b",
                EnumType.TraderGrid => "c",
                EnumType.TraderDivertTurn => "d",
                EnumType.TraderShootTurn => "e",
                _ => "x",
            };
            var tradeModeStr = tradeMode switch
            {
                EnumType.BuyFirst => "a",
                EnumType.SellFirst => "b",
                _ => "x",
            };

            var id = $"{baseCcyStr}{quoteCcyStr}{tradeTypeStr}{tradeModeStr}{intervalStr}";
            return id;
        }

        /// <summary>
        /// symbol:interval:qtyEach:minProfit:percent:price:isCompound:isCloseOrder:isCheckStopLoss:potential:rsi1:rsi3                              => SpotTrade3
        /// symbol:interval:qtyEach:minProfit:percent:price:isCompound:isCloseOrder:isCheckStopLoss:rsi1:rsi3:ratio:minChgFromLast:minChgFromMa      => SpotSwingShoot
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="inputStr"></param>
        /// <param name="tradeType"></param>
        /// <param name="tradeModel"></param>
        /// <param name="givenIdConfig"></param>
        /// <returns></returns>
        public static SwingParam CreateSwingParamFromInputStr(string exchange, string inputStr, EnumType tradeType, EnumType tradeModel)
        {
        //    var p = CreateParamSpotTrader3(exchange, inputStr, tradeModel, givenIdConfig);
        //    p.Tradertype = tradeType;
        //    return p;
        //}
        //private static SwingParam CreateParamSpotTrader3(string exchange, string inputStr, EnumType tradeModel, GivenIdConfig givenIdConfig)
        //{
            var items = inputStr.Split(':');
            var symbol = items[0];
            var interval = int.Parse(items[1]);
            var qtyEach = double.Parse(items[2]);
            var minProfit = double.Parse(items[3]);
            var percent = double.Parse(items[4]);

            double? price = null;
            if (items.Length >= 6)
            {
                var str = items[5];
                if (str != "d")
                    price = double.Parse(str);
            }
            var isCompound = true;
            if (items.Length >= 7)
            {
                var str = items[6];
                if (str == "0")
                    isCompound = false;
            }

            bool isCloseOrder = true;
            if (items.Length >= 8)
            {
                var str = items[7];
                if (str == "0")
                    isCloseOrder = false;
            }

            bool isCheckStopLoss = true;
            if (items.Length >= 9)
            {
                var str = items[8];
                if (str == "0")
                    isCheckStopLoss = false;
            }

            //var config = givenIdConfig;
            //var d = config.Digits.ToDictionary(t => t.Symbol, t => t);

            var givenId = CreateRefId(symbol, interval, tradeType, tradeModel);
            var p = new SwingParam
            {
                Symbol = symbol,
                Interval = interval,
                //NumDigits = d[symbol].Digits,
                GivenId = givenId,
                Tradertype = tradeType,
                TradeModel = tradeModel,
                Exchange = exchange,
                QtyEach = qtyEach,
                MinProfit = minProfit,
                Percent = percent,
                IsCompound = isCompound,
                //MinChange = minPotential,
                IsCheckStopLoss = isCheckStopLoss,
                IsCloseOrder = isCloseOrder
            };

            //if (tradeType == EnumType.TraderTurn)
            //{
            //    double minPotential = interval switch
            //    {
            //        300 => 0.015,
            //        900 => 0.02,
            //        1800 => 0.02,
            //        3600 => 0.04,
            //        7200 => 0.06,
            //        14400 => 0.08,
            //        _ => throw new NotImplementedException(),
            //    };

            //    if (items.Length >= 10)
            //    {
            //        var str = items[9];
            //        if (str != "d")
            //            minPotential = double.Parse(str);
            //    }
            //    p.MinPotential = minPotential;


            //    double? rsi1_single = null;
            //    double? rsi3_single = null;
            //    double? rsi1_double1 = null;
            //    double? rsi3_double1 = null;
            //    double? rsi1_double2 = null;
            //    double? rsi3_double2 = null;
            //    int? numBars = 5;
            //    int? maxDist = 3;
            //    double? stopLoss = -0.12;

            //    if (items.Length >= 11)
            //    {
            //        var str = items[10];
            //        if (str != "d")
            //            rsi1_single = double.Parse(str);
            //    }

            //    if (items.Length >= 12)
            //    {
            //        var str = items[11];
            //        if (str != "d")
            //            rsi3_single = double.Parse(str);
            //    }

            //    if (items.Length >= 13)
            //    {
            //        var str = items[12];
            //        if (str != "d")
            //            rsi1_double1 = double.Parse(str);
            //    }

            //    if (items.Length >= 14)
            //    {
            //        var str = items[13];
            //        if (str != "d")
            //            rsi3_double1 = double.Parse(str);
            //    }
            //    if (items.Length >= 15)
            //    {
            //        var str = items[14];
            //        if (str != "d")
            //            rsi1_double2 = double.Parse(str);
            //    }
            //    if (items.Length >= 16)
            //    {
            //        var str = items[15];
            //        if (str != "d")
            //            rsi3_double2 = double.Parse(str);
            //    }
            //    if (items.Length >= 17)
            //    {
            //        var str = items[16];
            //        if (str != "d")
            //            numBars = int.Parse(str);
            //    }
            //    if (items.Length >= 18)
            //    {
            //        var str = items[17];
            //        if (str != "d")
            //            maxDist = int.Parse(str);
            //    }
            //    if (items.Length >= 19)
            //    {
            //        var str = items[18];
            //        if (str != "d")
            //            stopLoss = double.Parse(str);
            //    }

            //    p.NumBars = numBars.Value;
            //    p.MaxDist = maxDist.Value;
            //    p.StopLoss = stopLoss.Value;

            //    //////////////////////////////////////////////////////////////////////////////////////
            //    if (p.IsBuyFirst)
            //    {
            //        p.PriceOnlyBuy = price.HasValue ? price.Value : double.MaxValue;
            //        p.Rsi1Thd_S = rsi1_single.HasValue ? rsi1_single.Value : 20;
            //        p.Rsi3Thd_S = rsi3_single.HasValue ? rsi3_single.Value : 40;
            //        p.Rsi1Thd_D1 = rsi1_double1.HasValue ? rsi1_double1.Value : 20;
            //        p.Rsi3Thd_D1 = rsi3_double1.HasValue ? rsi3_double1.Value : 40;
            //        p.Rsi1Thd_D2 = rsi1_double2.HasValue ? rsi1_double2.Value : 20;
            //        p.Rsi3Thd_D2 = rsi3_double2.HasValue ? rsi3_double2.Value : 40;
            //    }
            //    else
            //    {
            //        p.PriceOnlySell = price.HasValue ? price.Value : double.MinValue;
            //        p.Rsi1Thd_S = rsi1_single.HasValue ? rsi1_single.Value : 80;
            //        p.Rsi3Thd_S = rsi3_single.HasValue ? rsi3_single.Value : 60;
            //        p.Rsi1Thd_D1 = rsi1_double1.HasValue ? rsi1_double1.Value : 80;
            //        p.Rsi3Thd_D1 = rsi3_double1.HasValue ? rsi3_double1.Value : 60;
            //        p.Rsi1Thd_D2 = rsi1_double2.HasValue ? rsi1_double2.Value : 80;
            //        p.Rsi3Thd_D2 = rsi3_double2.HasValue ? rsi3_double2.Value : 60;
            //    }
            //}
            if (tradeType == EnumType.TraderShoot)
            {
                double? rsi1 = null;
                double? rsi3 = null;
                double? ratio = null;
                double? minChgFromLast = null;
                double? minChgFromMa = null;

                if (items.Length >= 10)
                {
                    var str = items[9];
                    if (str != "d")
                        rsi1 = double.Parse(str);
                }

                if (items.Length >= 11)
                {
                    var str = items[10];
                    if (str != "d")
                        rsi3 = double.Parse(str);
                }

                if (items.Length >= 12)
                {
                    var str = items[11];
                    if (str != "d")
                        ratio = double.Parse(str);
                }

                if (items.Length >= 13)
                {
                    var str = items[12];
                    if (str != "d")
                        minChgFromLast = double.Parse(str);
                }

                if (items.Length >= 14)
                {
                    var str = items[13];
                    if (str != "d")
                        minChgFromMa = double.Parse(str);
                }


                if (p.IsBuyFirst)
                {
                    p.PriceOnlyBuy = price.HasValue ? price.Value : double.MaxValue;
                    p.Rsi1Thd_S = rsi1.HasValue ? rsi1.Value : 20;
                    p.Rsi3Thd_S = rsi3.HasValue ? rsi3.Value : 40;
                    p.Ratio = ratio.HasValue ? ratio.Value : -3;
                    p.MinChgFromLast = minChgFromLast.HasValue ? minChgFromLast.Value : -0.01;
                    p.MinChgFromMA = minChgFromMa.HasValue ? minChgFromMa.Value : -0.02;
                }
                else
                {
                    p.PriceOnlySell = price.HasValue ? price.Value : double.MinValue;
                    p.Rsi1Thd_S = rsi1.HasValue ? rsi1.Value : 80;
                    p.Rsi3Thd_S = rsi3.HasValue ? rsi3.Value : 60;
                    p.Ratio = ratio.HasValue ? ratio.Value : 3;
                    p.MinChgFromLast = minChgFromLast.HasValue ? minChgFromLast.Value : 0.01;
                    p.MinChgFromMA = minChgFromMa.HasValue ? minChgFromMa.Value : 0.02;
                }
            }
            return p;
        }

        //public static SwingParam CreateSwingParam(GivenIdConfig config, string symbol, int interval, EnumType tradeType, EnumType tradeModel, 
        //    double qtyEach, double minProfit, double percent, double? price=null, bool isCompound = true)
        //{
        //    //var config = JsonConvert.DeserializeObject<GivenIdConfig>(File.ReadAllText(givenIdConfigFile));
        //    var d = config.Digits.ToDictionary(t => t.Symbol, t => t);
        //    //var ids = new Dictionary<string, int>
        //    //{
        //    //    { "ETH_USDT_180", 10 },
        //    //    { "ETH_USDT_300", 11 },
        //    //    { "ETH_USDT_900", 12 },

        //    //    { "LTC_USDT_180", 20 },
        //    //    { "LTC_USDT_300", 21 },
        //    //    { "LTC_USDT_900", 22 },

        //    //    { "EOS_USDT_180", 30 },
        //    //    { "EOS_USDT_300", 31 },
        //    //    { "EOS_USDT_900", 32 },
        //    //    { "XRP_USDT_180", 40 },
        //    //    { "XRP_USDT_300", 41 },
        //    //    { "XRP_USDT_900", 42 },
        //    //    { "BCH_USDT_180", 50 },
        //    //    { "BCH_USDT_300", 51 },
        //    //    { "BCH_USDT_900", 52 },
        //    //    { "BSV_USDT_180", 60 },
        //    //    { "BSV_USDT_300", 61 },
        //    //    { "BSV_USDT_900", 62 },
        //    //    { "XLM_USDT_180", 70 },
        //    //    { "XLM_USDT_300", 71 },
        //    //    { "XLM_USDT_900", 72 },
        //    //    { "ADA_USDT_180", 80 },
        //    //    { "ADA_USDT_300", 81 },
        //    //    { "ADA_USDT_900", 82 },
        //    //     { "DOT_USDT_180", 90 },
        //    //     { "DOT_USDT_300", 91 },
        //    //     { "DOT_USDT_900", 92 },
        //    //    { "DOGE_USDT_180", 100 },
        //    //    { "DOGE_USDT_300", 101 },
        //    //    { "DOGE_USDT_900", 102 },
        //    //     { "TRX_USDT_180", 110 },
        //    //     { "TRX_USDT_300", 111 },
        //    //     { "TRX_USDT_900", 112 },
        //    //    { "LINK_USDT_180", 120 },
        //    //    { "LINK_USDT_300", 121 },
        //    //    { "LINK_USDT_900", 122 },
        //    //     { "ETC_USDT_180", 130 },
        //    //     { "ETC_USDT_300", 131 },
        //    //     { "ETC_USDT_900", 132 },
        //    //     { "UNI_USDT_180", 140 },
        //    //     { "UNI_USDT_300", 141 },
        //    //     { "UNI_USDT_900", 142 },
        //    //    { "DASH_USDT_180", 150 },
        //    //    { "DASH_USDT_300", 151 },
        //    //    { "DASH_USDT_900", 152 },
        //    //     { "XMR_USDT_180", 160 },
        //    //     { "XMR_USDT_300", 161 },
        //    //     { "XMR_USDT_900", 162 },
        //    //     { "NEO_USDT_180", 170 },
        //    //     { "NEO_USDT_300", 171 },
        //    //     { "NEO_USDT_900", 172 },
        //    //     { "FIL_USDT_180", 180 },
        //    //     { "FIL_USDT_300", 181 },
        //    //     { "FIL_USDT_900", 182 },
        //    //    { "QTUM_USDT_180", 190 },
        //    //    { "QTUM_USDT_300", 191 },
        //    //    { "QTUM_USDT_900", 192 },
        //    //     { "ONT_USDT_180", 200 },
        //    //     { "ONT_USDT_300", 201 },
        //    //     { "ONT_USDT_900", 202 },
        //    //    { "LUNA_USDT_180", 210 },
        //    //    { "LUNA_USDT_300", 211 },
        //    //    { "LUNA_USDT_900", 212 },
        //    //    { "IOST_USDT_180", 220 },
        //    //    { "IOST_USDT_300", 221 },
        //    //    { "IOST_USDT_900", 222 },
        //    //     { "ZEC_USDT_180", 230 },
        //    //     { "ZEC_USDT_300", 231 },
        //    //     { "ZEC_USDT_900", 232 },
        //    //     { "XTZ_USDT_180", 240 },
        //    //     { "XTZ_USDT_300", 241 },
        //    //     { "XTZ_USDT_900", 242 },
        //    //   { "SUSHI_USDT_180", 250 },
        //    //   { "SUSHI_USDT_300", 251 },
        //    //   { "SUSHI_USDT_900", 252 },
        //    //     { "BNT_USDT_180", 260 },
        //    //     { "BNT_USDT_300", 261 },
        //    //     { "BNT_USDT_900", 262 },
        //    //    { "ATOM_USDT_180", 270 },
        //    //    { "ATOM_USDT_300", 271 },
        //    //    { "ATOM_USDT_900", 272 },
        //    //     { "OKB_USDT_180", 280 },
        //    //     { "OKB_USDT_300", 281 },
        //    //     { "OKB_USDT_900", 282 },
        //    //     { "OMG_USDT_180", 290 },
        //    //     { "OMG_USDT_300", 291 },
        //    //     { "OMG_USDT_900", 292 },
        //    //    { "MANA_USDT_180", 300 },
        //    //    { "MANA_USDT_300", 301 },
        //    //    { "MANA_USDT_900", 302 },
        //    //    { "SC_USDT_180", 310 },
        //    //    { "SC_USDT_300", 311 },
        //    //    { "SC_USDT_900", 312 },

        //    //    { "THETA_USDT_300", 320 },
        //    //    { "THETA_USDT_900", 321 },
        //    //    { "THETA_USDT_180", 322 },
        //    //    { "BTT_USDT_300", 330 },
        //    //    { "BTT_USDT_900", 331 },
        //    //    { "BTT_USDT_180", 332 },
        //    //    { "AAVE_USDT_300", 340 },
        //    //    { "AAVE_USDT_900", 341 },
        //    //    { "AAVE_USDT_180", 342 },
        //    //    { "GRT_USDT_300", 350 },
        //    //    { "GRT_USDT_900", 351 },
        //    //    { "GRT_USDT_180", 352 },
        //    //    { "CRV_USDT_300", 360 },
        //    //    { "CRV_USDT_900", 361 },
        //    //    { "CRV_USDT_180", 362 },

        //    //    { "CFX_USDT_300", 380 },
        //    //    { "CFX_USDT_900", 381 },
        //    //    { "CFX_USDT_180", 382 },
        //    //    { "OKT_USDT_300", 390 },
        //    //    { "OKT_USDT_900", 391 },
        //    //    { "OKT_USDT_180", 392 },
        //    //    { "SRM_USDT_300", 400 },
        //    //    { "SRM_USDT_900", 401 },
        //    //    { "SRM_USDT_180", 402 },
        //    //    { "AVAX_USDT_300", 410 },
        //    //    { "AVAX_USDT_900", 411 },
        //    //    { "AVAX_USDT_180", 412 },
        //    //    { "MASK_USDT_300", 420 },
        //    //    { "MASK_USDT_900", 421 },
        //    //    { "MASK_USDT_180", 422 },
        //    //    { "ZIL_USDT_300", 430 },
        //    //    { "ZIL_USDT_900", 431 },
        //    //    { "ZIL_USDT_180", 432 },
        //    //    { "KLAY_USDT_300", 440 },
        //    //    { "KLAY_USDT_900", 441 },
        //    //    { "KLAY_USDT_180", 442 },
        //    //    { "GHST_USDT_300", 450 },
        //    //    { "GHST_USDT_900", 451 },
        //    //    { "GHST_USDT_180", 452 },
        //    //    { "FRONT_USDT_300", 460 },
        //    //    { "FRONT_USDT_900", 461 },
        //    //    { "FRONT_USDT_180", 462 },
        //    //    { "CHZ_USDT_300", 470 },
        //    //    { "CHZ_USDT_900", 471 },
        //    //    { "CHZ_USDT_180", 472 },
        //    //};

        //    //var digits = new Dictionary<string, int>
        //    //{
        //    //    { "ETH_USDT", 2 },
        //    //    { "LTC_USDT", 2 },
        //    //    { "EOS_USDT", 3 },
        //    //    { "XRP_USDT", 4 },
        //    //    { "BCH_USDT", 2 },
        //    //    { "BSV_USDT", 2 },
        //    //    { "XLM_USDT", 4 },
        //    //    { "ADA_USDT", 5 },
        //    //    { "DOT_USDT", 3 },
        //    //    { "DOGE_USDT", 6 },
        //    //    { "TRX_USDT", 5 },
        //    //    { "LINK_USDT", 3 },
        //    //    { "ETC_USDT", 3 },
        //    //    { "UNI_USDT", 3 },
        //    //    { "DASH_USDT", 2 },
        //    //    { "XMR_USDT", 2 },
        //    //    { "NEO_USDT", 2 },
        //    //    { "FIL_USDT", 2 },
        //    //    { "QTUM_USDT", 3 },
        //    //    { "ONT_USDT", 4 },
        //    //    { "LUNA_USDT", 3 },
        //    //    { "IOST_USDT", 6 },
        //    //    { "ZEC_USDT", 2 },
        //    //    { "XTZ_USDT", 3 },
        //    //    { "SUSHI_USDT", 3 },
        //    //    { "BNT_USDT", 3 },
        //    //    { "ATOM_USDT", 3 },
        //    //    { "OKB_USDT", 3 },
        //    //    { "OMG_USDT", 3 },
        //    //    { "MANA_USDT", 4 },
        //    //    { "SC_USDT", 5 },
        //    //    { "OKT_USDT", 2 },
        //    //    { "SRM_USDT", 3 },
        //    //    { "AVAX_USDT", 3 },
        //    //    { "THETA_USDT", 4 },
        //    //    { "BTT_USDT", 7 },
        //    //    { "AAVE_USDT", 2 },
        //    //    { "GRT_USDT", 4 },
        //    //    { "CRV_USDT", 3 },
        //    //    { "CFX_USDT", 4 },
        //    //    { "MASK_USDT", 4 },
        //    //    { "XEM_USDT", 3 },
        //    //    { "ZIL_USDT", 5 },
        //    //    { "KLAY_USDT", 4 },
        //    //    { "GHST_USDT", 4 },
        //    //    { "FRONT_USDT", 3 },
        //    //    { "CHZ_USDT", 5 }
        //    //};

        //    //var symbols = digits.Keys.ToList();
        //    //symbols.Sort();

        //    //digits.Add("KP3R_USDT", 2);
        //    //digits.Add("PHA_USDT", 4);
        //    //symbols.Add("KP3R_USDT");
        //    //symbols.Add("PHA_USDT");

        //    //var lst = new List<SymbolDigits>();
        //    //var count = 1000;
        //    //foreach(var sym in symbols)
        //    //    lst.Add(new SymbolDigits(sym, digits[sym], count++));


        //    //var intervalMap = new Dictionary<int, string>
        //    //{
        //    //    { 60, "a"},
        //    //    { 180, "b"},
        //    //    { 300, "c"},
        //    //    { 900, "d"},
        //    //    { 3600, "e"},
        //    //    { 7200, "f"},
        //    //    { 14400, "g"},
        //    //    { 86400, "h"}
        //    //};

        //    //var tradingTypeMap = new Dictionary<EnumType, string>
        //    //{
        //    //    {EnumType.SpotSwingFixedRatio, "a" },
        //    //    {EnumType.SpotGrid, "b" },
        //    //};

        //    //var tradingModelMap = new Dictionary<EnumType, string>
        //    //{
        //    //    {EnumType.BuyFirst3,  "a" },
        //    //    {EnumType.SellFirst3, "b" },
        //    //};

        //    //var config = new GivenIdConfig
        //    //{
        //    //    Digits = lst,
        //    //    IntervalMap = intervalMap,
        //    //    TradeTypeMap = tradingTypeMap,
        //    //    TradeModelMap = tradingModelMap
        //    //};

        //    //File.WriteAllText("1.json", JsonConvert.SerializeObject(config, Formatting.Indented));
        //    ////File.WriteAllText("2.json", JsonConvert.SerializeObject(intervalMap, Formatting.Indented));
        //    ////File.WriteAllText("3.json", JsonConvert.SerializeObject(tradingTypeMap, Formatting.Indented));
        //    ////File.WriteAllText("4.json", JsonConvert.SerializeObject(tradingModelMap, Formatting.Indented));


        //    ////symbol|interval|tradingType|tradeModel => givenId  1111

        //    ////(symbol, digits, givenId)
        //    ////tradeType

        //    //var intervals = new List<int> { 60, 300, 900, 3600 };
        //    //var givenIdBuyFirst3 = new Dictionary<string, int>();
        //    //var givenIdSellFirst3 = new Dictionary<string, int>();


        //    //var sid = 0;
        //    //foreach (var s in symbols)
        //    //{
        //    //    sid += 10;
        //    //    var id = sid;
        //    //    foreach (var i in intervals)
        //    //        givenIdBuyFirst3.Add($"{s}_{i}", id++);

        //    //    id = sid + 5;
        //    //    foreach (var i in intervals)
        //    //        givenIdSellFirst3.Add($"{s}_{i}", id++);
        //    //}

        //    ////File.WriteAllText("1.json", JsonConvert.SerializeObject(symbols, Formatting.Indented));
        //    ////File.WriteAllText("2.json", JsonConvert.SerializeObject(digits, Formatting.Indented));
        //    ////File.WriteAllText("3.json", JsonConvert.SerializeObject(givenIdBuyFirst3, Formatting.Indented));
        //    ////File.WriteAllText("4.json", JsonConvert.SerializeObject(givenIdSellFirst3, Formatting.Indented));

        //    ////var minProfit = 0.01;
        //    ////if (interval == 3600) minProfit = 0.02;
        //    //var ids = tradeModel == EnumType.BuyFirst3 ? givenIdBuyFirst3 : givenIdSellFirst3;
        //    var givenId = $"{d[symbol].Index}{config.IntervalMap[interval]}{config.TradeTypeMap[tradeType]}{config.TradeModelMap[tradeModel]}";
        //    var p = new SwingParam
        //    {
        //        Symbol = symbol,
        //        Interval = interval,
        //        NumDigits = d[symbol].Digits,
        //        GivenId = givenId,
        //        Tradertype = EnumType.SpotTrader3,
        //        TradeModel = tradeModel,
        //        Exchange = "Okex",
        //        QtyEach = qtyEach,
        //        MinProfit = minProfit, 
        //        Percent = percent,
        //        IsCompound = isCompound
        //    };

        //    var isBuyFirst = (tradeModel == EnumType.BuyFirst1 || tradeModel == EnumType.BuyFirst2 || tradeModel == EnumType.BuyFirst3);
        //    if (isBuyFirst)
        //    {
        //        p.PriceOnlyBuy = price.HasValue ? price.Value : double.MaxValue;
        //    }
        //    else
        //    {
        //        p.PriceOnlySell = price.HasValue ? price.Value : double.MinValue;
        //    }
        //    return p;
        //}

        //  LTC_ETH  3600  True False  2.00  2.50 -2.50 -3.40 0.018 0.025  6  4 0      
        //  LTC_ETH  3600 False True  2.00  2.50 -2.50 -3.40 0.018 0.025  6  4  0
        // BTC_USDT  3600  True False  1.50  3.50  0.60 -0.50 0.015 0.100  3  1 0
        // OKB_USDT  3600  True False  1.70  4.30  0.60 -0.20 0.028 0.350  3  4 0
        //  MCO_ETH  3600  True False  1.70  4.30  0.50 -0.40 0.045 0.300  8  4 0
        // XLM_USDT  3600  True False  1.65  4.30  0.50  0.00 0.015 0.200  8  4 0
        //IOST_USDT  3600  True False  1.55  3.20  0.50 -0.30 0.018 0.100 10  5 0
        //  SC_USDT  3600  True False  1.50  3.00  0.50 -0.30 0.015 0.100  4  5 0
        // DGB_USDT  3600  True False  1.55  3.00  0.00 -0.50 0.018 0.100  3  5 0
        //DOGE_USDT  3600  True False  1.50  3.20  0.50 -0.50 0.020 0.100  3  6 0
        //   OF_ETH  3600  True False  1.75  3.70  0.50 -0.20 0.043 0.250  8  8 0
        //   R_USDT  3600  True False  1.50  3.00  0.50  0.00 0.015 0.100  3  3 0
        //  BCX_BTC  7200  True False  1.70  3.00  0.00 -0.50 0.015 0.200  4  8 0
        // CAI_USDT  3600  True False  1.75  4.10  0.50 -0.20 0.048 0.100 10  7 0
        // XUC_USDT  3600  True False  1.50  3.00  0.50 -0.50 0.015 0.100  4  4 0
        public static void WriteParamsTxt(string filename, params SwingParam[] lst)
        {
            var str = new StringBuilder();
            foreach (var p in lst)
            {
                if (p.Tradertype == EnumType.FutureTakeProfit)
                {
                    var s = string.Format("{0,10} {1,10} {2,5} {3,8} {4,8} {5,5} {6,5} {7,8:0.000} {8,8:0.000} {9,8:0.0000} {10,8:0.0000} {11,3} {12,20} {13,3} {14,8}" +
                        " {15} {16} {17} {18} {19} {20} {21} {22} {23}\n",
                        p.Symbol1, p.Symbol2, p.Leverage, p.QtyMaxHoldLong, p.QtyMaxHoldShort, p.NumTrades, p.QtyEach, p.PriceStartOpenLong, p.PriceStartCloseLong, p.PriceStartOpenShort, p.PriceStartCloseShort,
                        p.TradeQtyCoef, p.PriceDelta2, p.Tradertype, p.GivenId, p.Exchange, p.MaxWaitingTimeForOrderToFinish, p.Symbol1_NumDigits, p.Symbol2_NumDigits,
                        p.PriceStrategy, p.Commision, p.QtyMaxUnhedged, p.IsCloseImmediate, p.MinProfit);
                    str.Append(s);
                }
                else if (p.Tradertype == EnumType.SpotTrader3 || p.Tradertype == EnumType.SpotSwingBand)
                {
                    var s = string.Format("{0,10} {1,5} {2,6} {3,6} {4,6:0.000} {5,6:0.000} {6,6:0.000} {7,6:0.000} {8,6:0.0000} {9,6:0.0000} {10,2} {11,2} {12,20} {13,5} {14,5:0.000} {15} {16}\n",
                        p.Symbol, p.Interval, p.IsSellFirst, p.IsBuyFirst, p.Ratio1_Overshoot, p.Ratio2_Overshoot, p.Ratio1_Undershoot, p.Ratio2_Undershoot, p.MinProfit, p.MaxProfit,
                        p.NumTrades, p.NumDigits, p.Tradertype, p.GivenId, p.QtyMin, p.Qty1, p.MinPotential);
                    str.Append(s);
                }
                else if (p.Tradertype == EnumType.SpotSwingShoot)
                {
                    var s = string.Format("{0,10} {1,5} {2,6} {3,6} {4,6:0.000} {5,6:0.000} {6,6:0.0000} {7,6:0.0000} {8,2} {9,2} {10,20} {11,5} {12,5:0.000} {13}\n",
                        p.Symbol, p.Interval, p.IsSellFirst, p.IsBuyFirst, p.Ratio1_Undershoot, p.Ratio2_Undershoot, p.MinProfit, p.MaxProfit,
                        p.NumTrades, p.NumDigits, p.Tradertype, p.GivenId, p.QtyMin, p.Qty1);
                    str.Append(s);
                }
                else if (p.Tradertype == EnumType.SpotSwingBidAsk)
                {
                    var s = string.Format("{0,10} {1,5} {2,6} {3,6} {4,6} \n", p.Symbol, p.Tradertype, p.GivenId, p.QtyMin, p.Qty1);
                    str.Append(s);
                }
                else if (p.Tradertype == EnumType.SpotGrid)
                {
                    var s = string.Format("{0,10} {1,6} {2,6} {3,6} {4,6:0.000} {5,6:0.000} {6,6:0.000} {7,6:0.000} {8,6:0.000} {9,6:0.000} {10}\n",
                        p.Symbol, p.Interval, p.Tradertype, p.GivenId, p.QtyEach, p.Qty1, p.Qty2, p.PriceOnlyBuy, p.PriceOnlySell, p.Percent, p.NumDigits);
                    str.Append(s);
                }
                else if (p.Tradertype == EnumType.SpotBandGrid)
                {
                    var s = string.Format("{0,10} {1,5} {2,6} {3,6} {4,6:0.000} {5,6:0.000} {6,6:0.000} {7,6:0.000} {8} {9} {10,6:0.000} {11,6:0.000} {12,6:0.000}\n",
                        p.Symbol, p.Interval, p.Tradertype, p.GivenId, p.QtyEach, p.Qty1, p.Qty2, p.Percent, p.NumDigits, p.NumTrades, p.MinPotential, p.Ratio1_Overshoot, p.Ratio1_Undershoot);
                    str.Append(s);
                }
            }
            File.WriteAllText(filename, str.ToString());
        }

        public static List<SwingParam> LoadParamsTxt(string filename)
        {
            var lines = File.ReadAllLines(filename);
            var lst = new List<SwingParam>();
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line) && !line.Contains("#"))
                {
                    //var items = Regex.Split(line.Trim(), @"\s");
                    var items = line.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (line.Contains("FutureTakeProfit"))
                    {
                        var p = new SwingParam
                        {
                            Symbol1 = items[0],
                            Symbol2 = items[1],
                            Leverage = double.Parse(items[2]),
                            QtyMaxHoldLong = int.Parse(items[3]),
                            QtyMaxHoldShort = int.Parse(items[4]),
                            NumTrades = int.Parse(items[5]),
                            QtyEach = int.Parse(items[6]),
                            PriceStartOpenLong = double.Parse(items[7]),
                            PriceStartCloseLong = double.Parse(items[8]),
                            PriceStartOpenShort = double.Parse(items[9]),
                            PriceStartCloseShort = double.Parse(items[10]),
                            TradeQtyCoef = double.Parse(items[11]),
                            PriceDelta2 = double.Parse(items[12]),
                            //NumDigits = int.Parse(items[11]),
                            Tradertype = (EnumType)Enum.Parse(typeof(EnumType), items[13]),
                            GivenId = items[14],
                            Exchange = items[15],
                            MaxWaitingTimeForOrderToFinish = int.Parse(items[16]),
                            Symbol1_NumDigits = int.Parse(items[17]),
                            Symbol2_NumDigits = int.Parse(items[18]),
                            PriceStrategy = (EnumType)Enum.Parse(typeof(EnumType), items[19]),
                            Commision = double.Parse(items[20]),
                            QtyMaxUnhedged = int.Parse(items[21]),
                            IsCloseImmediate = bool.Parse(items[22]),
                            MinProfit = double.Parse(items[23])
                        };
                        lst.Add(p);
                    }
                    else if (line.Contains("SpotSwingBand") || line.Contains("SpotSwingFixedRatio"))
                    {
                        var p = new SwingParam
                        {
                            Symbol = items[0],
                            Interval = int.Parse(items[1]),
                            //IsSellFirst = bool.Parse(items[2]),
                            //IsBuyFirst = bool.Parse(items[3]),
                            Ratio1_Overshoot = double.Parse(items[4]),
                            Ratio2_Overshoot = double.Parse(items[5]),
                            Ratio1_Undershoot = double.Parse(items[6]),
                            Ratio2_Undershoot = double.Parse(items[7]),
                            MinProfit = double.Parse(items[8]),
                            MaxProfit = double.Parse(items[9]),
                            NumTrades = int.Parse(items[10]),
                            NumDigits = int.Parse(items[11]),
                            Tradertype = (EnumType)Enum.Parse(typeof(EnumType), items[12]),
                            GivenId = items[13],
                            QtyMin = double.Parse(items[14]),
                            Qty1 = double.Parse(items[15]),
                            MinPotential = double.Parse(items[16])
                        };
                        lst.Add(p);
                    }
                    else if (line.Contains("SpotSwingShoot"))
                    {
                        var p = new SwingParam
                        {
                            Symbol = items[0],
                            Interval = int.Parse(items[1]),
                            //IsSellFirst = bool.Parse(items[2]),
                            //IsBuyFirst = bool.Parse(items[3]),
                            Ratio1_Undershoot = double.Parse(items[4]),
                            Ratio2_Undershoot = double.Parse(items[5]),
                            MinProfit = double.Parse(items[6]),
                            MaxProfit = double.Parse(items[7]),
                            NumTrades = int.Parse(items[8]),
                            NumDigits = int.Parse(items[9]),
                            Tradertype = (EnumType)Enum.Parse(typeof(EnumType), items[10]),
                            GivenId = items[11],
                            QtyMin = double.Parse(items[12]),
                            Qty1 = double.Parse(items[13])
                        };
                        lst.Add(p);
                    }
                    else if (line.Contains("SpotSwingBidAsk"))
                    {
                        var p = new SwingParam
                        {
                            Symbol = items[0],
                            Tradertype = (EnumType)Enum.Parse(typeof(EnumType), items[1]),
                            GivenId = items[2],
                            QtyMin = double.Parse(items[3]),
                            Qty1 = double.Parse(items[4])
                        };
                        lst.Add(p);
                    }
                    else if (line.Contains("SpotGrid"))
                    {
                        //p.Symbol, p.Interval, p.IsSellFirst, p.IsBuyFirst, p.Tradertype, p.GivenId, p.QtyMin, p.QtyMax, p.QtyEach, p.Qty1, p.Qty2, p.PriceOnlyBuy, p.PriceOnlySell, p.Percent, p.NumDigits
                        var p = new SwingParam
                        {
                            Symbol = items[0],
                            Interval = int.Parse(items[1]),
                            Tradertype = (EnumType)Enum.Parse(typeof(EnumType), items[2]),
                            GivenId = items[3],
                            QtyEach = double.Parse(items[4]),
                            Qty1 = double.Parse(items[5]),
                            Qty2 = double.Parse(items[6]),
                            PriceOnlyBuy = double.Parse(items[7]),
                            PriceOnlySell = double.Parse(items[8]),
                            Percent = double.Parse(items[9]),
                            NumDigits = int.Parse(items[10])
                        };
                        lst.Add(p);
                    }
                    else if (line.Contains("SpotBandGrid"))
                    {
                        var p = new SwingParam
                        {
                            Symbol = items[0],
                            Interval = int.Parse(items[1]),
                            Tradertype = (EnumType)Enum.Parse(typeof(EnumType), items[2]),
                            GivenId = items[3],
                            QtyEach = double.Parse(items[4]),
                            Qty1 = double.Parse(items[5]),
                            Qty2 = double.Parse(items[6]),
                            Percent = double.Parse(items[7]),
                            NumDigits = int.Parse(items[8]),
                            NumTrades = int.Parse(items[9]),
                            MinPotential = double.Parse(items[10]),
                            Ratio1_Overshoot = double.Parse(items[11]),
                            Ratio1_Undershoot = double.Parse(items[12])
                        };
                        lst.Add(p);
                    }
                }
            }
            return lst;
        }

        //public static List<SwingParam> LoadParamsJson(string filename)
        //{
        //    var lines = File.ReadAllLines(filename).Where(l => !l.Contains("#"));
        //    var concat = string.Join(" ", lines.ToArray());
        //    var lst = JsonConvert.DeserializeObject<List<SwingParam>>(concat, new JsonBooleanConverter());
        //    return lst;
        //}

        public Dictionary<string, double> Params { get; set; }
        public List<double> Ratios { get; set; }

        //public double StartPrice { get; set; }         //start to open 
        //public int StartQty { get; set; }              //number of qty to open at the start price
        //public double MaxSwingPrice { get; set; }      //max price to open long
        //public double MinSwingPrice { get; set; }      //min price to open long, at this price hold reaches maximum
        //public double MaxPrice { get; set; }           //max price to sell everything
        [JsonIgnore]
        public string FileName { get { return "SwingParam-" + this.ParamId + ".json"; } }
        [JsonIgnore]
        public string Id { get { return ParamId; } }
        [JsonIgnore]
        public string ParamId => $"{GivenId}".Replace("-", "").Replace("_", "");
        [JsonIgnore]
        public string QuoteId { get { return $"{Symbol}_{Interval}"; } }

        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public string GivenId { get; set; }

        public string SubAlgoIdStr { get; set; }
        //params for future grid trading
        [JsonIgnore]
        public int IntervalBetweenTwoOrder { get; set; } = 120;
        public double Percent { get; set; }
        public string AccountId { get; set; }
       
        //used for spot swing
        public int Interval { get; set; }

        //buy/sell fixed ratio
        public double MinProfit { get; set; }
        public double MaxProfit { get; set; }

        //used for detection of overshoot/undershoot
        [JsonIgnore]
        public bool IsDetectOvershoot { get; set; }
        [JsonIgnore]
        public bool IsDetectUndershoot { get; set; }

        public string UserId { get; set; }
        [JsonIgnore]
        public double MinRatio { get; set; }

        //public bool IsCheckPrev_US { get; set; }
        //public bool IsCheckPrev_OS { get; set; }

        [JsonIgnore]
        public bool IsCheckStopLoss { get; set; }
        [JsonIgnore]
        public bool IsAjustOpenOrder { get; set; }
        [JsonIgnore]
        public bool IsGuranteeProfit { get; set; }
        public double MinPotential { get; set; }

        [JsonIgnore]
        public bool IsCheckVolumeRatio_OS { get; set; }
        [JsonIgnore]
        public bool IsCheckVolumeRatio_US { get; set; }
        [JsonIgnore]
        public double MinVolumeRatio { get; set; }

        [JsonIgnore]
        public bool IsLimitBandRange_OS { get; set; }
        [JsonIgnore]
        public bool IsLimitBandRange_US { get; set; }
       
        [JsonIgnore]
        public double Rsi1Thd_S { get; set; }
        [JsonIgnore]
        public double Rsi3Thd_S { get; set; }

        [JsonIgnore]
        public double Rsi1Thd_D1 { get; set; }
        [JsonIgnore]
        public double Rsi3Thd_D1 { get; set; }

        [JsonIgnore]
        public double Rsi1Thd_D2 { get; set; }
        [JsonIgnore]
        public double Rsi3Thd_D2 { get; set; }
        [JsonIgnore]
        public int NumBars { get; set; }
        [JsonIgnore]
        public int MaxDist { get; set; }
        [JsonIgnore]
        public double StopLoss { get; set; }


        [JsonIgnore]
        public bool IsDetectLargeRatio { get; set; }
        [JsonIgnore]
        public double LargeRatio_MinChange { get; set; }

        public double MinChgFromLast { get; set; }
        public double MinChgFromMA { get; set; }

        //used for buy/sell shoot
        [JsonIgnore]
        public double Ratio { get; set; }
        public double Ratio1_Undershoot { get; set; }
        public double Ratio2_Undershoot { get; set; }
        public double Ratio1_Overshoot { get; set; }
        public double Ratio2_Overshoot { get; set; }
        [JsonIgnore]
        public bool IsAdjustRatio { get; set; }

        [JsonIgnore]
        public bool IsCheckPrevLowHigh { get; set; }
        public int NumTrades { get; set; }
        public double NetC1Adj { get; set; }
        public double NetC2Adj { get; set; }

        //for fixed ratio trader
        public bool IsSellFirst { get { return TradeModel == EnumType.SellFirst; } }
        public bool IsBuyFirst { get { return !IsSellFirst; } }
        public double QtyMin { get; set; }       


        [JsonIgnore]
        public double QtyMaxHold { get; set; }
        [JsonIgnore]
        public double QtyMinHold { get; set; }
        public double Qty1 { get; set; }
        public double Qty2 { get; set; }

        //public double QtySell{ get; set; }
        //used for spot shoot trader
        [JsonIgnore]
        public bool IsSellOvershoot { get; set; }
        [JsonIgnore]
        public bool IsBuyUndershoot { get; set; }

        //used for open/close future position
        //[JsonIgnore]
        //public bool IsLimitQtyEach{ get; set; }
        [JsonIgnore]
        public bool IsOpenLong { get; set; }
        [JsonIgnore]
        public bool IsCloseOrder { get; set; }
        [JsonIgnore]
        public bool IsOpenShort { get; set; }
        [JsonIgnore]
        public bool IsCompound { get; set; }

        public int NumDigits { get; set; } = 4;

        [JsonIgnore]
        public double Avg { get; set; }
        [JsonIgnore]
        public double Std { get; set; }
        [JsonIgnore]
        public double PriceDelta { get; set; }

        public string Symbol2 { get; set; }
        public string Symbol1 { get; set; }
        public EnumType Tradertype { get; set; }
        public EnumType TradeModel { get; set; }

        public double Leverage { get; set; }
        public double Commision { get; internal set; } = 0.001;
        public double TradeQtyCoef { get; set; }
        public double PriceDelta2 { get; set; }
        public int QtyMaxHoldLong { get; set; }
        public int QtyMaxHoldShort { get; set; }
        public double PriceStartOpenShort { get; set; }
        public double PriceStartOpenLong { get; set; }
        public double PriceStartCloseShort { get; set; }
        public double PriceStartCloseLong { get; set; }
        public double QtyEach { get; set; }
        public int MaxWaitingTimeForOrderToFinish { get; set; }
        public double Symbol1_NumDigits { get; set; }
        public double Symbol2_NumDigits { get; set; }
        
        public string PriceStrategyStr
        {
            get
            {
                return PriceStrategy.ToString();
            }
        }
        public EnumType PriceStrategy { get; set; }
        public int QtyMaxUnhedged { get; set; }
        public bool IsCloseImmediate { get; set; } = false;

        public double QtyFactor { get; set; }
        public double QtyMax { get; set; }
        public double PriceOnlyBuy { get; set; }
        public double PriceOnlySell { get; set; }

        //public int NumSubOrders { get; internal set; }
        //public double QtyMinHoldShort { get; set; }
        //public bool IsPlaceOrder { get; set; }
        //MaxHoldQty, Percent, QtyEach, 
        //PriceOnlySell: above this price we only sell and never buy back
        //PriceOnlyBuy: below this price we only buy and never sell
        //1. When PositionClosed, create a closeLong order if balance sufficient and price > PriceOnlybuy; create a openLong order if maxQty not reached and price < PriceOnlySell
        //2. when PositionOpened, create a closeLong order if balance sufficient and price > PriceOnlybuy. create a openLong order if maxQty not reached and price < PriceOnlySell
        public SwingParam()
        {
        }

        public SwingParam(string fullname)
        {
            this.LoadParamFromFile(fullname);
        }

        public SwingParam(SwingParam other)
        {
            this.Copy(other);
        }

        public void SaveToFile(string filename)
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, str);
        }

        public void LoadParamFromFile(string filename)
        {
            var p = LoadSwingParamFromFile(filename);
            this.Copy(p);
        }

        public static SwingParam LoadSwingParamFromFile(string filename)
        {
            return JsonConvert.DeserializeObject<SwingParam>(File.ReadAllText(filename));
        }

        public void Copy(SwingParam other)
        {
            if (other == null) return;

            this.Exchange = other.Exchange;
            this.Symbol = other.Symbol;
            this.GivenId = other.GivenId;
            this.SubAlgoIdStr = other.SubAlgoIdStr;
            this.Percent = other.Percent;
            this.QtyEach = other.QtyEach;
            this.PriceStartOpenLong = other.PriceStartOpenLong;
            this.PriceStartOpenShort = other.PriceStartOpenShort;
            this.PriceStartCloseShort = other.PriceStartCloseShort;
            this.PriceStartCloseLong = other.PriceStartCloseLong;

            this.AccountId = other.AccountId;
            this.Interval = other.Interval;
            this.NumDigits = other.NumDigits;
            this.Tradertype = other.Tradertype;
            this.TradeModel = other.TradeModel;
            this.MinProfit = other.MinProfit;
            this.MaxProfit = other.MaxProfit;

            this.Symbol1         = other.Symbol1;
            this.Symbol2         = other.Symbol2        ;
            this.Leverage        = other.Leverage       ;
            this.Commision       = other.Commision      ;
            this.TradeQtyCoef     = other.TradeQtyCoef    ;
            this.PriceDelta2     = other.PriceDelta2    ;
            this.QtyMaxHoldLong  = other.QtyMaxHoldLong ;
            this.QtyMaxHoldShort = other.QtyMaxHoldShort;
            this.PriceOnlyBuy = other.PriceOnlyBuy;
            this.PriceOnlySell = other.PriceOnlySell;
            this.MaxProfit       = other.MaxProfit      ;
            this.MaxProfit = other.MaxProfit;
            this.MaxWaitingTimeForOrderToFinish = other.MaxWaitingTimeForOrderToFinish;
            this.Symbol1_NumDigits = other.Symbol1_NumDigits;
            this.Symbol2_NumDigits = other.Symbol2_NumDigits;
            this.PriceStrategy = other.PriceStrategy;
            this.QtyMaxUnhedged = other.QtyMaxUnhedged;
            this.IsCloseImmediate = other.IsCloseImmediate;
            this.UserId = other.UserId;
            //this.IsPlaceOrder = other.IsPlaceOrder;            

            //this.IsBuyFirst = other.IsBuyFirst;
            //this.IsSellFirst = other.IsSellFirst;
            this.IsDetectOvershoot = other.IsDetectOvershoot;
            this.IsDetectUndershoot = other.IsDetectUndershoot;

            this.IsCheckStopLoss = other.IsCheckStopLoss;
            this.IsAjustOpenOrder = other.IsAjustOpenOrder;
            this.IsGuranteeProfit = other.IsGuranteeProfit;
            this.MinRatio = other.MinRatio;
            this.MinPotential = other.MinPotential;
            this.IsCheckVolumeRatio_OS = other.IsCheckVolumeRatio_OS;
            this.IsCheckVolumeRatio_US = other.IsCheckVolumeRatio_US;
            this.MinVolumeRatio = other.MinVolumeRatio;
            this.IsLimitBandRange_OS = other.IsLimitBandRange_OS;
            this.IsLimitBandRange_US = other.IsLimitBandRange_US;
            this.Rsi1Thd_S = other.Rsi1Thd_S;
            this.Rsi3Thd_S = other.Rsi3Thd_S;

            this.Rsi1Thd_D1 = other.Rsi1Thd_D1;
            this.Rsi3Thd_D1 = other.Rsi3Thd_D1;
            this.Rsi1Thd_D2 = other.Rsi1Thd_D2;
            this.Rsi3Thd_D2 = other.Rsi3Thd_D2;
            this.NumBars = other.NumBars;
            this.MaxDist = other.MaxDist;
            this.StopLoss = other.StopLoss;

            this.IsDetectLargeRatio = other.IsDetectLargeRatio;
            this.Avg = other.Avg;
            this.LargeRatio_MinChange = other.LargeRatio_MinChange;
            this.MinChgFromLast = other.MinChgFromLast;
            this.MinChgFromMA = other.MinChgFromMA;

            this.Ratio1_Undershoot = other.Ratio1_Undershoot;
            this.Ratio2_Undershoot = other.Ratio2_Undershoot;
            this.Ratio1_Overshoot = other.Ratio1_Overshoot;
            this.Ratio2_Overshoot = other.Ratio2_Overshoot;
            this.Ratio = other.Ratio;

            this.QtyMaxHold = other.QtyMaxHold;
            this.QtyMinHold = other.QtyMinHold;
            this.QtyMin = other.QtyMin;
            this.Qty1 = other.Qty1;
            this.Qty2 = other.Qty2;
            this.QtyMax = other.QtyMax;
            this.QtyFactor = other.QtyFactor;
            
            //this.QtyMinHoldLong = other.QtyMinHoldLong;
            this.QtyMaxHoldLong = other.QtyMaxHoldLong;
            this.QtyMaxHoldShort = other.QtyMaxHoldShort;
            //this.QtyMinHoldShort = other.QtyMinHoldShort;

            this.NumTrades = other.NumTrades;
            this.NetC1Adj = other.NetC1Adj;
            this.NetC2Adj = other.NetC2Adj;

            this.IsAdjustRatio = other.IsAdjustRatio;
            this.IsCheckPrevLowHigh = other.IsCheckPrevLowHigh;
            this.IsSellOvershoot = other.IsSellOvershoot;
            this.IsBuyUndershoot = other.IsBuyUndershoot;
            this.IsOpenLong = other.IsOpenLong;
            this.IsOpenShort = other.IsOpenShort;
            this.IsCloseOrder = other.IsCloseOrder;
            this.IsCompound = other.IsCompound;
        }

        public bool Equals(SwingParam other)
        {
            if (other == null) return false;
            return (this.Exchange == other.Exchange &&
                    this.Symbol == other.Symbol &&
                    this.GivenId == other.GivenId &&
                    this.SubAlgoIdStr == other.SubAlgoIdStr && 
                    this.Percent == other.Percent &&
                    this.QtyEach == other.QtyEach &&
                    this.PriceStartOpenLong == other.PriceStartOpenLong &&
                    this.PriceStartOpenShort == other.PriceStartOpenShort &&
                    this.PriceStartCloseShort == other.PriceStartCloseShort &&
                    this.PriceStartCloseLong == other.PriceStartCloseLong &&
                    
                    this.AccountId == other.AccountId &&
                    this.Interval == other.Interval &&
                    this.NumDigits == other.NumDigits &&
                    this.Tradertype == other.Tradertype &&
                    this.TradeModel == other.TradeModel &&

                    this.MinProfit == other.MinProfit &&
                    this.MaxProfit == other.MaxProfit &&
                    this.NetC1Adj == other.NetC1Adj &&
                    this.NetC2Adj == other.NetC2Adj &&
                    
                    this.Symbol1 == other.Symbol1 &&
                    this.Symbol2 == other.Symbol2 &&
                    this.Leverage == other.Leverage &&
                    this.Commision == other.Commision &&
                    this.TradeQtyCoef == other.TradeQtyCoef &&
                    this.PriceDelta2 == other.PriceDelta2 &&
                    this.QtyMaxHoldLong == other.QtyMaxHoldLong &&
                    this.QtyMaxHoldShort == other.QtyMaxHoldShort &&
                    this.PriceOnlyBuy == other.PriceOnlyBuy && 
                    this.PriceOnlySell == other.PriceOnlySell &&
                    this.MaxProfit == other.MaxProfit &&
                    this.MaxProfit == other.MaxProfit &&
                    this.MaxWaitingTimeForOrderToFinish == other.MaxWaitingTimeForOrderToFinish &&
                    this.Symbol1_NumDigits == other.Symbol1_NumDigits &&
                    this.Symbol2_NumDigits == other.Symbol2_NumDigits &&
                    this.PriceStrategy == other.PriceStrategy &&
                    this.QtyMaxUnhedged == other.QtyMaxUnhedged &&
                    this.IsCloseImmediate == other.IsCloseImmediate &&
                    
                    this.IsBuyFirst == other.IsBuyFirst &&
                    this.IsSellFirst == other.IsSellFirst &&
                    this.UserId == other.UserId &&
                    //this.IsPlaceOrder == other.IsPlaceOrder &&
                    this.MinChgFromLast == other.MinChgFromLast &&
                    this.MinChgFromMA == other.MinChgFromMA &&                    
                    
                    this.IsDetectOvershoot == other.IsDetectOvershoot &&
                    this.IsDetectUndershoot == other.IsDetectUndershoot &&

                    this.MinRatio == other.MinRatio &&
                    this.Ratio1_Undershoot == other.Ratio1_Undershoot &&
                    this.Ratio2_Undershoot == other.Ratio2_Undershoot &&
                    this.Ratio1_Overshoot == other.Ratio1_Overshoot &&
                    this.Ratio2_Overshoot == other.Ratio2_Overshoot &&
                    this.Rsi3Thd_S == other.Rsi3Thd_S &&
                    this.Ratio == other.Ratio &&
                    this.MinPotential == other.MinPotential &&
                    this.IsCheckStopLoss == other.IsCheckStopLoss &&
                    this.IsAjustOpenOrder == other.IsAjustOpenOrder &&
                    this.IsGuranteeProfit == other.IsGuranteeProfit &&
                    this.IsCheckVolumeRatio_OS == other.IsCheckVolumeRatio_OS &&
                    this.IsCheckVolumeRatio_US == other.IsCheckVolumeRatio_US &&
                    this.MinVolumeRatio == other.MinVolumeRatio &&
                    this.IsLimitBandRange_OS == other.IsLimitBandRange_OS &&
                    this.IsLimitBandRange_US == other.IsLimitBandRange_US &&
                    this.Rsi1Thd_S == other.Rsi1Thd_S &&
                    this.Rsi1Thd_D1 == other.Rsi1Thd_D1 &&
                    this.Rsi3Thd_D1 == other.Rsi3Thd_D1 &&
                    this.Rsi1Thd_D2 == other.Rsi1Thd_D2 &&
                    this.Rsi3Thd_D2 == other.Rsi3Thd_D2 &&
                    this.NumBars == other.NumBars && 
                    this.MaxDist == other.MaxDist &&
                    this.StopLoss == other.StopLoss &&
                    
                    this.IsDetectLargeRatio == other.IsDetectLargeRatio &&
                    this.IsDetectLargeRatio == other.IsDetectLargeRatio &&
                    this.Avg == other.Avg &&
                    this.LargeRatio_MinChange == other.LargeRatio_MinChange &&
                    
                    this.QtyMaxHold == other.QtyMaxHold &&
                    this.QtyMinHold == other.QtyMinHold &&
                    this.QtyMin == other.QtyMin &&
                    this.Qty1 == other.Qty1 &&
                    this.Qty2 == other.Qty2 &&
                    this.QtyMax == other.QtyMax &&
                    this.QtyFactor == other.QtyFactor &&

                    //this.QtyBuy == other.QtyBuy &&
                    //this.QtySell == other.QtySell &&                    
                    //this.QtyMinHoldLong == other.QtyMinHoldLong &&
                    this.QtyMaxHoldLong == other.QtyMaxHoldLong &&
                    this.QtyMaxHoldShort == other.QtyMaxHoldShort &&
                    //this.QtyMinHoldShort == other.QtyMinHoldShort &&                    
                    
                    this.NumTrades == other.NumTrades &&
                    this.IsAdjustRatio == other.IsAdjustRatio &&
                    this.IsCheckPrevLowHigh == other.IsCheckPrevLowHigh &&
                    this.IsSellOvershoot == other.IsSellOvershoot &&
                    this.IsBuyUndershoot == other.IsBuyUndershoot &&
                    this.IsOpenLong == other.IsOpenLong &&
                    this.IsOpenShort == other.IsOpenShort &&
                    this.IsCompound == other.IsCompound &&
                    this.IsCloseOrder == other.IsCloseOrder);
        }
    }
}
