using CryptoCommon.DataTypes;
using CryptoCommon.Interfaces;
using PortableCSharpLib.TechnicalAnalysis;
using System.Collections.Generic;
using System.IO;

namespace CryptoCommon.Models
{
    public class QuoteLoader : IQuoteLoader
    {
        Dictionary<string, Dictionary<int, List<QuoteBasicFileInfo>>> _qbFileInfo;
        Dictionary<string, List<QuoteCaptureFileInfo>> _qcFileInfo;
        //ICache<IQuoteBasic> _cache = new Cache<IQuoteBasic>(50);

        string CreateQuoteBasicKey(string symbol, int interval, long stime, long etime)
        {
            var key = string.Format("{0}_{1}_{2}_{3}", symbol, interval, stime, etime);
            return key;
        }
        string CreateQuoteCaptureKey(string symbol, long stime, long etime)
        {
            var key = string.Format("{0}_{1}_{2}", symbol, stime, etime);
            return key;
        }

        QuoteBasic LoadQuoteBasic(string filename)
        {
            var readtream = new FileStream(filename, FileMode.Open);
            var q1 = QuoteBasic.InitByStream(readtream);
            readtream.Close();
            return q1;
        }

        QuoteCapture LoadQuoteCapture(string filename)
        {
            var q = new QuoteCapture();
            var readtream = new FileStream(filename, FileMode.Open);
            q.LoadStream(readtream);
            readtream.Close();
            return q;
        }

        public QuoteBasic LoadQuote(string symbol, int interval, long startTime, int maxCount)
        {
            QuoteBasic quote = new QuoteBasic(symbol, interval);

            if (_qbFileInfo.ContainsKey(symbol) && _qbFileInfo[symbol].ContainsKey(interval))
            {
                var lst = _qbFileInfo[symbol][interval];
                var timeToSearch = startTime;
                var index = lst.FindIndex(t => t.EndTime >= timeToSearch);
                if (index != -1)
                {
                    for (int i = index; i < lst.Count; i++)
                    {
                        var q = this.LoadQuoteBasic(lst[i].FullFileName);
                        quote.Append(q);
                        if (quote.Count >= maxCount)
                            break;
                    }
                }
            }

            if (quote.Count < maxCount)
            {
                if (_qcFileInfo.ContainsKey(symbol))
                {
                    var lst = _qcFileInfo[symbol];
                    var timeToSearch = quote.Count == 0 ? startTime : quote.LastTime;
                    var index = lst.FindIndex(t => t.EndTime > timeToSearch);
                    if (index != -1)
                    {
                        for (int i = index; i < lst.Count; i++)
                        {
                            var q = this.LoadQuoteCapture(lst[i].FullFileName);
                            quote.Append(q);
                            if (quote.Count >= maxCount)
                                break;
                        }
                    }
                }
            }

            return quote;
        }

        //public static List<string> FindFilesContainingRegx(string folder, string pattern)
        //{
        //    if (!Directory.Exists(folder)) return new List<string>();
        //    var files = FileOperation.FindAllFiles(folder);
        //    if (files == null || files.Count == 0) return new List<string>();
        //    return files.Where(f => Regex.Match(f, pattern).Success).ToList();
        //}

        //public static string GetIntervalStr(int interval)
        //{
        //    var strInterval = string.Empty;
        //    if (interval < 60)
        //        strInterval = string.Format("{0}s", interval);
        //    else if (interval < 3600)
        //        strInterval = string.Format("{0}m", interval / 60);
        //    else if (interval < 86400)
        //        strInterval = string.Format("{0}h", interval / 3600);
        //    else if (interval == 86400)
        //        strInterval = "1D";
        //    else
        //        throw new NotSupportedException("Bar size not supported");
        //    return strInterval;
        //}

        //public static string GetSingleQuoteFileName(string symbol, int interval)
        //{
        //    return string.Format("{0}_{1}.json", symbol, QuoteMeta.GetIntervalStr(interval));
        //}
        //public static string GetCurrentWeekQuoteFileName(string symbol, int interval)
        //{
        //    var date = DateTime.UtcNow;
        //    var year = date.GetIsoYear();
        //    var week = date.GetIso8601WeekOfYear();
        //    return string.Format("{0}_{1}_{2}_{3}_current.rat", year, week, symbol, GetIntervalStr(interval));
        //}
        //static public string GetWeeklyQuoteFileName(string symbol, int interval, DateTime endDate)
        //{
        //    //var strInterval = string.Empty;
        //    //if (interval < 60)
        //    //    strInterval = string.Format("{0}s", interval);
        //    //else if (interval < 3600)
        //    //    strInterval = string.Format("{0}m", interval / 60);
        //    //else if (interval < 86400)
        //    //    strInterval = string.Format("{0}h", interval / 3600);
        //    //else if (interval == 86400)
        //    //    strInterval = "1D";
        //    //else
        //    //    throw new NotSupportedException("Bar size not supported");

        //    var weekNo = endDate.GetIso8601WeekOfYear();
        //    var yearNo = endDate.GetIsoYear() % 100;
        //    var filename = string.Format("{0:D2}{1:D2}_{2}_{3}.json", yearNo, weekNo, symbol, GetIntervalStr(interval));
        //    return filename;
        //}

        //public static IQuoteBasic LoadSingleQuote(string filename)
        //{
        //    return null;
        //    //return ExtensionQuoteBasic.DeSerializeFromJsonFile(filename);
        //    //return BinarySerializer.DeSerializeObject(filename, typeof(QuoteCommon));
        //}
        //public static void SaveSingleQuote(string filename, IQuoteBasic quote)
        //{
        //    //quote.SerializeToJsonFile(filename);
        //}
        //public static void CreateQuoteFile(string dataFolder, string metaFolder, string symbol, int interval)
        //{
        //    var pattern = string.Format("{0}_{1}.*?json", symbol, GetIntervalStr(interval));
        //    var listFiles = FindFilesContainingRegx(dataFolder, pattern);
        //    if (listFiles == null || listFiles.Count == 0) return;
        //    var meta = new List<QuoteFile>();
        //    foreach (var f in listFiles)
        //    {
        //        Console.WriteLine("creating quotefile for {0}", f);
        //        try
        //        {
        //            var q = LoadSingleQuote(f);
        //            if (q != null)
        //            {
        //                var qf = new QuoteFile(symbol, q.Interval, q.FirstTime, q.LastTime, f);
        //                meta.Add(qf);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex);
        //        }
        //    }
        //    var fileName = Path.Combine(metaFolder, string.Format("meta_{0}_{1}.xml", symbol, GetIntervalStr(interval)));
        //    ObjectSerializer.SerializeToXML(fileName, meta);
        //}
        //public static void ConvertFormat()
        //{
        //    var folder = @"E:\Data\IBTrader";
        //    var files = FileOperation.FindFilesContainingStr(folder, ".rat");
        //    foreach (var f in files)
        //    {
        //        var filename = f.Replace(".rat", ".json");
        //        if (!File.Exists(filename))
        //        {
        //            Console.WriteLine(f);
        //            var q = LoadSingleQuote(f);
        //            var quote = new QuoteBasic(q);
        //            SaveSingleQuote(filename, q);
        //        }

        //        //var q =  ExtensionQuoteBasic.DeSerializeFromJsonFile(f);
        //        //BinarySerializer.SerializeObject(f, q.ConvertToQuoteCommon());
        //    }
        //}



        //public string MetaFolder { get; private set; }

        //Dictionary<string, HashSet<int>> _needToSave = new Dictionary<string, HashSet<int>>();
        //Dictionary<string, Dictionary<int, List<QuoteFile>>> _meta = new Dictionary<string, Dictionary<int, List<QuoteFile>>>();
        //Dictionary<string, List<int>> _symbolToIntervals = new Dictionary<string, List<int>>();

        //public QuoteMeta(string pathForMetaData)
        //{
        //    this.MetaFolder = pathForMetaData;
        //    var filenames = QuoteMeta.FindFilesContainingRegx(this.MetaFolder, @"meta_.*?\.xml");
        //    this.Load(filenames.ToArray());
        //    foreach (var key in _meta.Keys)
        //    {
        //        var lst = _meta[key].Keys.ToList();
        //        lst.Sort();
        //        _symbolToIntervals.Add(key, lst);
        //        _needToSave.Add(key, new HashSet<int>());
        //    }
        //}
        //~QuoteMeta()
        //{
        //    this.Save();
        //}

        //void Load(params string[] quotefiles)
        //{
        //    foreach (var f in quotefiles)
        //    {
        //        if (!File.Exists(f)) continue;
        //        var listFiles = ObjectSerializer.DeserializeFromXML<List<QuoteFile>>(f);
        //        if (listFiles == null || listFiles.Count == 0) continue;
        //        var symbol = listFiles[0].Symbol;
        //        var interval = listFiles[0].Interval;
        //        if (!_meta.ContainsKey(symbol))
        //            _meta.Add(symbol, new Dictionary<int, List<QuoteFile>>());
        //        if (!_meta[symbol].ContainsKey(interval))
        //            _meta[symbol].Add(interval, new List<QuoteFile>());
        //        _meta[symbol][interval].AddRange(listFiles.Where(fn => File.Exists(fn.FullFileName)).OrderBy(fn => fn.StartTime));
        //    }
        //}

        //void Save()
        //{
        //    foreach (var symbol in _meta.Keys)
        //    {
        //        foreach (var interval in _needToSave[symbol])
        //        {
        //            if (_meta[symbol][interval] != null && _meta[symbol][interval].Count > 0)
        //            {
        //                var filename = Path.Combine(this.MetaFolder, string.Format("meta_{0}_{1}s.xml", symbol, interval));
        //                ObjectSerializer.SerializeToXML(filename, _meta[symbol][interval]);
        //            }
        //        }
        //        _needToSave[symbol].Clear();
        //    }
        //}

        //#region static

        //static IQuoteBasic ConvertInterval(IQuoteBasic q, int newInterval, int subInterval = -1)
        //{
        //    if (q == null || q.Count <= 0 || newInterval < q.Interval) return null;
        //    var newQuote = new QuoteBasic(q.Symbol, newInterval);
        //    newQuote.Append(q, subInterval);
        //    return newQuote;
        //}
        //static IQuoteBasic ConvertInterval(List<IQuoteBasic> quotes, int newInterval, int subInterval = -1)
        //{
        //    if (quotes == null || quotes.Count <= 0) return null;
        //    var newQuote = new QuoteBasic(quotes[0].Symbol, newInterval);

        //    foreach (var q in quotes) newQuote.Append(q, subInterval);
        //    return newQuote;
        //}
        //#endregion

        //#region create/update quotefiles
        //public void CreateQuoteFileForMissingQuote(string folder, string symbol, int interval)
        //{
        //    //var path = Path.Combine(folder, symbol);
        //    //if (!Directory.Exists(path)) return;
        //    //var fileName = Path.Combine(path, string.Format("{0}_{1}s.xml", symbol, interval));
        //    //CLoadQuotes.CreateQuoteFileForMissingQuote(fileName);

        //    var pattern = string.Format("{0}_{1}.json", symbol, GetIntervalStr(interval));
        //    var listFiles = FileOperation.FindFilesContainingStr(folder, pattern);

        //    if (!_meta.ContainsKey(symbol)) _meta.Add(symbol, new Dictionary<int, List<QuoteFile>>());
        //    if (!_meta[symbol].ContainsKey(interval)) _meta[symbol].Add(interval, new List<QuoteFile>());

        //    var listRecords = _meta[symbol][interval];
        //    var count = listRecords.Count;
        //    foreach (var f in listFiles)
        //    {
        //        if (listRecords.All(q => q.FullFileName != f))
        //        {
        //            Console.WriteLine(string.Format("creating QuoteFile for {0}", f));
        //            var quote = LoadSingleQuote(f);
        //            var qf = new QuoteFile(quote.Symbol, quote.Interval, quote.FirstTime, quote.LastTime, f);
        //            listRecords.Add(qf);
        //        }
        //    }

        //    if (listRecords.Count > count)
        //    {
        //        _needToSave[symbol].Add(interval);
        //        _meta[symbol][interval] = listRecords.OrderBy(q => q.StartTime).ToList();
        //        this.Save();
        //    }
        //}
        ////update only the folder in record file
        ////when we copy quote file to different location, we need to update the folder
        //public void UpdateFolderInQuoteFile(string folder, string symbol, int interval)
        //{
        //    if (!_meta.ContainsKey(symbol) || !_meta[symbol].ContainsKey(interval)) return;
        //    var quoteFiles = _meta[symbol][interval];

        //    foreach (var qf in quoteFiles) qf.Folder = folder;
        //    _needToSave[symbol].Add(interval);
        //    this.Save();
        //}

        //public IQuoteBasic LoadQuote(string filename, int mergePeriod = -1)
        //{
        //    var quote = LoadSingleQuote(filename);
        //    if (quote == null) return null;
        //    if (mergePeriod > 0)
        //    {
        //        var qf1 = this.FindQuoteFilePrevious(quote.Symbol, quote.Interval, quote.FirstTime);
        //        var qf2 = this.FindQuoteFileNext(quote.Symbol, quote.Interval, quote.FirstTime);
        //        var q1 = qf1 == null ? null : LoadSingleQuote(qf1.FullFileName);
        //        var q2 = qf2 == null ? null : LoadSingleQuote(qf2.FullFileName);
        //        quote = MergeQuote(quote, q1, q2, mergePeriod);
        //    }
        //    return quote;
        //}
        //public List<IQuoteBasic> LoadLatestQuoteBeforeGivenDate(string symbol, int interval, DateTime edate, int numWeeks, int mergePeriod = -1)
        //{
        //    if (!_meta.ContainsKey(symbol) || !_meta[symbol].ContainsKey(interval) || _meta[symbol][interval].Count == 0) return null;

        //    int eindex = -1;
        //    if (edate >= _meta[symbol][interval].Last().EndDate)
        //        eindex = _meta[symbol][interval].Count - 1;
        //    else
        //        eindex = this.FindIndexQuoteFileContainingTime(symbol, interval, edate.GetUnixTimeFromUTC());

        //    if (eindex == -1) return null;
        //    var sindex = Math.Max(0, eindex - numWeeks + 1);
        //    var qfs = _meta[symbol][interval].GetRange(sindex, eindex - sindex + 1);
        //    return LoadQuotes(qfs.Select(f => f.FullFileName).ToList(), mergePeriod);
        //}
        //public List<IQuoteBasic> LoadQuoteBetweenDates(string symbol, int interval, DateTime sdate, DateTime edate, int mergePeriod = -1)
        //{
        //    if (!_meta.ContainsKey(symbol) || !_meta[symbol].ContainsKey(interval)) return null;
        //    //var syear = sdate.GetIsoYear();
        //    //var sweek = sdate.GetIso8601WeekOfYear();
        //    //var eyear = edate.GetIsoYear();
        //    //var eweek = edate.GetIso8601WeekOfYear();
        //    //return this.LoadQuotes(symbol, interval, syear, sweek, eyear, eweek, mergePeriod);
        //    var sindex = this.FindIndexQuoteFileContainingTime(symbol, interval, sdate.GetUnixTimeFromUTC());
        //    var eindex = this.FindIndexQuoteFileContainingTime(symbol, interval, edate.GetUnixTimeFromUTC());
        //    if (sindex == -1 || eindex == -1) return null;
        //    if (sindex == -1) sindex = 0;
        //    if (eindex == -1) eindex = _meta[symbol][interval].Count - 1;
        //    var qfs = _meta[symbol][interval].GetRange(sindex, eindex - sindex + 1);
        //    return LoadQuotes(qfs.Select(f => f.FullFileName).ToList(), mergePeriod);
        //}
        //public void SaveQuoteForCurrentWeek(IQuoteBasic quote)
        //{
        //    if (quote == null || quote.Count == 0) return;
        //    var q = quote;

        //    var filename = GetCurrentWeekQuoteFileName(quote.Symbol, quote.Interval);
        //    var fullFileName = Path.Combine(this.MetaFolder, filename);
        //    if (File.Exists(fullFileName))
        //    {
        //        var existQuote = LoadSingleQuote(fullFileName);
        //        if (existQuote != null && existQuote.Count > 0)
        //        {
        //            existQuote.Append(q);
        //            q = existQuote;
        //        }
        //    }
        //    //q.SerializeToJsonFile(fullFileName);
        //    SaveSingleQuote(fullFileName, q);

        //    //////////////////////////////////////////////////////////////////////////////////
        //    var qf = new QuoteFile(q.Symbol, q.Interval, q.FirstTime, q.LastTime, fullFileName);
        //    var symbol = qf.Symbol;
        //    var interval = qf.Interval;

        //    if (!_meta.ContainsKey(symbol)) _meta.Add(symbol, new Dictionary<int, List<QuoteFile>>());
        //    if (!_meta[symbol].ContainsKey(interval)) _meta[symbol].Add(interval, new List<QuoteFile>());

        //    var count = _meta[symbol][interval].Count;
        //    if (count == 0)
        //        _meta[symbol][interval].Add(qf);
        //    else
        //    {
        //        var last = _meta[symbol][interval][count - 1];
        //        if (last.Year == qf.Year && last.WeekNo == qf.WeekNo)
        //            _meta[symbol][interval][count - 1] = qf;
        //        else
        //            _meta[symbol][interval].Add(qf);
        //    }

        //    _needToSave[symbol].Add(interval);
        //    this.Save();
        //}
        //public IQuoteBasic LoadQuoteForCurrentWeek(string symbol, int interval, int mergePeriod = -1)
        //{
        //    if (!_meta.ContainsKey(symbol) || !_meta[symbol].ContainsKey(interval) || _meta[symbol][interval].Count == 0) return null;

        //    var utcNow = DateTime.UtcNow;
        //    var year = utcNow.GetIsoYear();
        //    var week = utcNow.GetIso8601WeekOfYear();
        //    return this.LoadQuoteForGivenWeek(symbol, interval, year, week, mergePeriod);
        //}

        //#endregion

        //public void ConvertInterval(string symbol, int oldInterval, int newInterval)
        //{
        //    if (oldInterval >= newInterval) return;
        //    if (!_meta.ContainsKey(symbol) || !_meta[symbol].ContainsKey(oldInterval)) return;

        //    if (!_meta[symbol].ContainsKey(newInterval))
        //        _meta[symbol].Add(newInterval, new List<QuoteFile>());

        //    var isNeedToSave = false;
        //    foreach (var f in _meta[symbol][oldInterval])
        //    {
        //        if (_meta[symbol][newInterval].Any(q => q.Year == f.Year && q.WeekNo == f.WeekNo)) continue;
        //        isNeedToSave = true;

        //        Console.WriteLine("converting {0}...", f.FullFileName);
        //        var quote = LoadSingleQuote(f.FullFileName);
        //        var newQuote = ConvertInterval(quote, newInterval);
        //        var qf = new QuoteFile(newQuote.Symbol, newQuote.Interval, newQuote.FirstTime, newQuote.LastTime, f.Folder);
        //        BinarySerializer.SerializeObject<QuoteCommon>(qf.FullFileName, newQuote.ConvertToQuoteCommon());
        //        _meta[symbol][newInterval].Add(qf);
        //        _needToSave[symbol].Add(newInterval);
        //    }

        //    if (isNeedToSave) this.Save();
        //}

        //#region find quote
        //QuoteFile FindQuoteFileContainingTime(string symbol, int interval, long time)
        //{
        //    var index = this.FindIndexQuoteFileContainingTime(symbol, interval, time);
        //    return index == -1 ? null : _meta[symbol][interval][index];
        //}
        //List<QuoteFile> FindQuoteFilesSpanningTime(string symbol, int interval, long timeStart, long timeEnd)
        //{
        //    var sindex = this.FindIndexQuoteFileContainingTime(symbol, interval, timeStart);
        //    if (sindex == -1) return null;
        //    var eindex = this.FindIndexQuoteFileContainingTime(symbol, interval, timeEnd);
        //    if (eindex == -1) return null;
        //    return _meta[symbol][interval].GetRange(sindex, eindex - sindex + 1).ToList();
        //}
        //QuoteFile FindQuoteFilePrevious(string symbol, int interval, long time)
        //{
        //    var index = this.FindIndexQuoteFileContainingTime(symbol, interval, time);
        //    if (index == -1 || index == 0) return null;
        //    return _meta[symbol][interval][index - 1];
        //}
        //QuoteFile FindQuoteFileNext(string symbol, int interval, long time)
        //{
        //    var index = this.FindIndexQuoteFileContainingTime(symbol, interval, time);
        //    if (index == -1 || index == _meta[symbol][interval].Count - 1) return null;
        //    return _meta[symbol][interval][index + 1];
        //}
        //int FindIndexQuoteFileCurrentWeek(string symbol, int interval, long time)
        //{
        //    if (!_meta.ContainsKey(symbol) || !_meta[symbol].ContainsKey(interval)) return -1;
        //    var quoteFiles = _meta[symbol][interval];
        //    if (quoteFiles == null || quoteFiles.Count <= 0) return -1;
        //    var weekNo = time.GetUTCFromUnixTime().GetIso8601WeekOfYear();
        //    var year = time.GetUTCFromUnixTime().Year;
        //    var index = quoteFiles.FindLastIndex(f => f.WeekNo == weekNo && f.Year == year);
        //    return index;
        //}
        //int FindIndexQuoteFileContainingTime(string symbol, int interval, long time)
        //{
        //    if (!_meta.ContainsKey(symbol) || !_meta[symbol].ContainsKey(interval)) return -1;
        //    var quoteFiles = _meta[symbol][interval];
        //    if (quoteFiles == null || quoteFiles.Count <= 0) return -1;
        //    return quoteFiles.FindLastIndex(q => CFacility.WithinRange(time, q.StartTime, q.EndTime));
        //}
        //#endregion

        //#region static load/merge quotes   
        //static IQuoteBasic MergeQuote(IQuoteBasic current, IQuoteBasic prev, IQuoteBasic next, int mergePeriod = -1)
        //{
        //    if (current == null) return null;

        //    //computer interval as the maximum of three
        //    var interval = prev != null ? Math.Max(prev.Interval, current.Interval) : current.Interval;
        //    interval = next != null ? Math.Max(next.Interval, interval) : interval;

        //    var quote = new QuoteCommon(current.Symbol, interval);
        //    if (prev != null && prev.Count > 0)
        //        quote.AppendQuote(prev, false);   //alwasy merge entire previous quote

        //    quote.AppendQuote(current, false);

        //    if (next != null && next.Count > 0)
        //    {
        //        //by default we merge whole quotes by assingning a large value to duration
        //        if (mergePeriod != -1)
        //            quote.AppendQuote(next.Extract(next.FirstTime, Math.Min(next.LastTime, next.FirstTime + mergePeriod)), false);
        //        else
        //            quote.AppendQuote(next, false);
        //    }
        //    return quote;
        //}
        //static List<IQuoteBasic> MergeQuote(List<IQuoteBasic> quotes, int mergePeriod)
        //{
        //    if (quotes == null || quotes.Count <= 1) return quotes;

        //    var mergedQuotes = new List<IQuoteBasic>();
        //    mergedQuotes.Add(MergeQuote(quotes[0], null, quotes[1], mergePeriod));

        //    for (int i = 1; i < quotes.Count - 1; i++)
        //        mergedQuotes.Add(MergeQuote(quotes[i], quotes[i - 1], quotes[i + 1], mergePeriod));

        //    mergedQuotes.Add(MergeQuote(quotes[quotes.Count - 1], quotes[quotes.Count - 2], null, mergePeriod));

        //    return mergedQuotes;
        //}
        //static List<IQuoteBasic> LoadQuotes(List<string> filenames, int mergePeriod = -1)
        //{
        //    var listQuote = new List<IQuoteBasic>();
        //    foreach (var f in filenames)
        //        listQuote.Add(LoadSingleQuote(f));
        //    if (mergePeriod > 0)
        //        listQuote = MergeQuote(listQuote, mergePeriod);
        //    return listQuote;
        //}
        //List<IQuoteBasic> LoadQuotesBetweenWeeks(string symbol, int interval, int syear, int sweek, int eyear, int eweek, int mergePeriod = -1)
        //{
        //    if (!_meta.ContainsKey(symbol) || !_meta[symbol].ContainsKey(interval)) return null;

        //    if (syear == eyear)
        //    {
        //        var qfs = _meta[symbol][interval].Where(f => (f.Year == syear && f.WeekNo >= sweek && f.WeekNo <= eweek));
        //        if (qfs == null) return null;
        //        return LoadQuotes(qfs.Select(f => f.FullFileName).ToList(), mergePeriod);
        //    }
        //    else if (eyear == syear + 1)
        //    {
        //        var qfs = new List<QuoteFile>();
        //        var f1 = _meta[symbol][interval].Where(f => (f.Year == syear && f.WeekNo >= sweek)).ToList();
        //        var f2 = _meta[symbol][interval].Where(f => (f.Year == eyear && f.WeekNo <= eweek)).ToList();
        //        if (f1 != null) qfs.AddRange(f1);
        //        if (f2 != null) qfs.AddRange(f2);
        //        return LoadQuotes(qfs.Select(f => f.FullFileName).ToList(), mergePeriod);
        //    }
        //    else if (eyear > syear)
        //    {
        //        var qfs = new List<QuoteFile>();
        //        var f1 = _meta[symbol][interval].Where(f => (f.Year == syear && f.WeekNo >= sweek)).ToList();
        //        var f2 = _meta[symbol][interval].Where(f => (f.Year > syear && f.Year < eyear)).ToList();
        //        var f3 = _meta[symbol][interval].Where(f => (f.Year == eyear && f.WeekNo <= eweek)).ToList();
        //        if (f1 != null) qfs.AddRange(f1);
        //        if (f2 != null) qfs.AddRange(f2);
        //        if (f3 != null) qfs.AddRange(f3);
        //        return LoadQuotes(qfs.Select(f => f.FullFileName).ToList(), mergePeriod);
        //    }
        //    return null;
        //}
        //List<IQuoteBasic> LoadLatestQuoteBeforeGivenWeek(string symbol, int interval, int year, int week, int numWeeks, int mergePeriod = -1)
        //{
        //    if (!_meta.ContainsKey(symbol) || !_meta[symbol].ContainsKey(interval) || _meta[symbol][interval].Count == 0) return null;

        //    var last = _meta[symbol][interval].Last();
        //    var eindex = -1;
        //    if (year > last.Year || (year == last.Year && week >= last.WeekNo))
        //        eindex = _meta[symbol][interval].Count;
        //    else
        //        eindex = _meta[symbol][interval].FindIndex(f => f.Year == year && f.WeekNo == week);

        //    if (eindex == -1) return null;
        //    var sindex = Math.Max(0, eindex - numWeeks - 1);
        //    var qfs = _meta[symbol][interval].GetRange(sindex, eindex - sindex + 1);
        //    return LoadQuotes(qfs.Select(f => f.FullFileName).ToList(), mergePeriod);
        //}
        //IQuoteBasic LoadQuoteForGivenWeek(string symbol, int interval, int year, int week, int mergePeriod = -1)
        //{
        //    if (!_meta.ContainsKey(symbol) || !_meta[symbol].ContainsKey(interval)) return null;

        //    var qf = _meta[symbol][interval].Find(f => f.Year == year && f.WeekNo == week);
        //    if (qf == null) return null;

        //    return this.LoadQuote(qf.FullFileName, mergePeriod);
        //}
        //#endregion
    }

}
