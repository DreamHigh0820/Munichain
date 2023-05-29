using Microsoft.JSInterop;
using Newtonsoft.Json;
using Shared.Models.DealComponents;
using Shared.Models.Users;
using System.Reflection;

namespace Shared.Helpers
{
    public static class DealExtensions
    {
        public static DealModel SetDefaults(this DealModel deal, User userData)
        {
            deal.Status = "Draft";
            deal.CreatedBy = userData.Id;
            deal.Series = new List<Series>();
            deal.LastModifiedBy = userData.Id;
            deal.LastModifiedByDisplayName = userData.DisplayName;
            deal.IsLocked = false;
            return deal;
        }

        public static List<ConcurrencyItem> ConcurrencyCompare<T>(this T updated, T fromDB, string baseName = "Deal", string[] addlRules = null)
        {
            List<ConcurrencyItem> variances = new List<ConcurrencyItem>();
            PropertyInfo[] fi = updated?.GetType()?.GetProperties() ?? new PropertyInfo[0];
            foreach (PropertyInfo f in fi)
            {
                ConcurrencyItem v = new ConcurrencyItem();
                v.Prop = f.Name;

                var rules = new string[] { "History", "LastModified", "Id", "Created", "AutoFill", "IsPublished", "Formatted" };
                if (addlRules != null && addlRules.Any())
                {
                    rules = rules.Concat(addlRules).ToArray();
                }

                // Dont count diffs of last modified
                if (!v.Prop.ContainsAny(rules))
                {
                    v.Updated = f.GetValue(updated);
                    v.FromDB = f.GetValue(fromDB);
                    v.EventType = ChangeEventType.Modified;
                    if (!string.IsNullOrEmpty(baseName))
                    {
                        v.BaseModelName = baseName;
                    }
                    if (f.PropertyType.IsValueType)
                    {
                        if (!Equals(v.Updated, v.FromDB))
                            variances.Add(v);
                    }
                    else if (f.PropertyType == typeof(string))
                    {
                        if (!Equals(v.Updated, v.FromDB))
                            variances.Add(v);
                    }
                    else if (f.PropertyType == typeof(List<string>))
                    {
                        List<string>? updatedLst = null;
                        List<string>? fromDBLst = null;
                        if (v.Updated is not null)
                            updatedLst = (List<string>)v.Updated;
                        if (v.FromDB is not null)
                            fromDBLst = (List<string>)v.FromDB;
                        if (updatedLst is not null && fromDBLst is not null)
                        {
                            v.Updated = string.Join(",", updatedLst);
                            v.FromDB = string.Join(",", fromDBLst);
                            var updatedListNotfromDBList = updatedLst.Except(fromDBLst).ToList();
                            var fromDBListNotupdatedList = fromDBLst.Except(updatedLst).ToList();
                            if (updatedListNotfromDBList is not null && updatedListNotfromDBList.Any())
                                variances.Add(v);
                            else if (fromDBListNotupdatedList is not null && fromDBListNotupdatedList.Any())
                                variances.Add(v);
                        }
                        else if (updatedLst is not null && fromDBLst is null)
                        {
                            v.Updated = string.Join(",", updatedLst);
                            v.FromDB = string.Empty;
                            variances.Add(v);
                        }
                        else if (updatedLst is null && fromDBLst is not null)
                        {
                            v.Updated = string.Empty;
                            v.FromDB = string.Join(",", fromDBLst);
                            variances.Add(v);
                        }
                    }
                    else if (f.PropertyType == typeof(Performance))
                    {
                        Performance? updatedPerf = null;
                        Performance? fromDBPerf = null;
                        if (v.Updated is not null)
                            updatedPerf = (Performance)v.Updated;
                        if (v.FromDB is not null)
                            fromDBPerf = (Performance)v.FromDB;
                        if (updatedPerf is not null && fromDBPerf is not null)
                        {
                            List<ConcurrencyItem> diffs = updatedPerf.ConcurrencyCompare(fromDBPerf, "Performance");
                            if (diffs.Any())
                                diffs.ForEach(f => variances.Add(f));
                        }
                    }
                    else if (f.PropertyType == typeof(List<Series>))
                    {
                        List<Series>? updatedSeries = new List<Series>();
                        List<Series>? fromDbSeries = new List<Series>();
                        if (v.Updated is not null)
                            updatedSeries = (List<Series>)v.Updated;
                        if (v.FromDB is not null)
                            fromDbSeries = (List<Series>)v.FromDB;

                        List<Series> deletedList = (from o in fromDbSeries
                                                    join p in updatedSeries
                                                             on o.GlobalSeriesID equals p.GlobalSeriesID into t
                                                    from od in t.DefaultIfEmpty()
                                                    where od == null
                                                    select o).ToList();

                        List<Series> addedList = (from o in updatedSeries
                                                  join p in fromDbSeries
                                                                 on o.GlobalSeriesID equals p.GlobalSeriesID into t
                                                  from od in t.DefaultIfEmpty()
                                                  where od == null
                                                  select o).ToList();

                        List<Series> inBoth = (from o in updatedSeries
                                               join p in fromDbSeries
                                                            on o.GlobalSeriesID equals p.GlobalSeriesID into t
                                               from od in t.DefaultIfEmpty()
                                               where od != null
                                               select od).ToList();



                        foreach (var series in deletedList)
                        {
                            ConcurrencyItem variance = new ConcurrencyItem()
                            {
                                BaseModelName = "Deal",
                                Prop = "Series - " + series.Name,
                                EventType = ChangeEventType.Removed
                            };
                            variances.Add(variance);
                        }
                        foreach (var series in addedList)
                        {
                            ConcurrencyItem variance = new ConcurrencyItem()
                            {
                                BaseModelName = "Deal",
                                Prop = "Series - " + series.Name,
                                EventType = ChangeEventType.Added
                            };
                            variances.Add(variance);
                        }
                        foreach (var series in inBoth)
                        {
                            var fromDb = fromDbSeries.FirstOrDefault(x => x.GlobalSeriesID == series.GlobalSeriesID);
                            var updatedSeriesValue = updatedSeries.FirstOrDefault(x => x.GlobalSeriesID == series.GlobalSeriesID);

                            List<ConcurrencyItem> diffs = updatedSeriesValue.ConcurrencyCompare(fromDb, "Series " + series.Name);

                            if (diffs != null && diffs.Any())
                            {
                                foreach (var diff in diffs)
                                {
                                    ConcurrencyItem variance = new ConcurrencyItem()
                                    {
                                        BaseModelName = series.Name,
                                        Prop = diff.Prop,
                                        EventType = diff.EventType != null ? diff.EventType : ChangeEventType.Modified,
                                        FromDB = diff.FromDB,
                                        Updated = diff.Updated
                                    };
                                    variances.Add(variance);
                                }
                            }
                        }
                    }
                    else if (f.PropertyType == typeof(List<Maturity>))
                    {
                        List<Maturity>? updatedMaturities = new List<Maturity>();
                        List<Maturity>? fromDbMaturities = new List<Maturity>();
                        if (v.Updated is not null)
                            updatedMaturities = (List<Maturity>)v.Updated;
                        if (v.FromDB is not null)
                            fromDbMaturities = (List<Maturity>)v.FromDB;

                        List<Maturity> deletedList = (from o in fromDbMaturities
                                                      join p in updatedMaturities on o.GlobalMaturityID equals p.GlobalMaturityID into t
                                                      from od in t.DefaultIfEmpty()
                                                      where od == null
                                                      select o).ToList();

                        List<Maturity> addedList = (from o in updatedMaturities
                                                    join p in fromDbMaturities on o.GlobalMaturityID equals p.GlobalMaturityID into t
                                                    from od in t.DefaultIfEmpty()
                                                    where od == null
                                                    select o).ToList();

                        List<Maturity> inBoth = (from o in updatedMaturities
                                                 join p in fromDbMaturities on o.GlobalMaturityID equals p.GlobalMaturityID into t
                                                 from od in t.DefaultIfEmpty()
                                                 where od != null
                                                 select od).ToList();


                        foreach (var maturity in deletedList)
                        {
                            var dateString = maturity.MaturityDateUTC.HasValue ? maturity.MaturityDateUTC.Value.ToShortDateString() : "";

                            ConcurrencyItem variance = new ConcurrencyItem()
                            {
                                BaseModelName = baseName,
                                Prop = "Maturity " + dateString,
                                MaturityDate = dateString,
                                EventType = ChangeEventType.Removed
                            };
                            variances.Add(variance);
                        }
                        foreach (var maturity in addedList)
                        {
                            var dateString = maturity.MaturityDateUTC.HasValue ? maturity.MaturityDateUTC.Value.ToShortDateString() : "";
                            ConcurrencyItem variance = new ConcurrencyItem()
                            {
                                BaseModelName = baseName,
                                Prop = "Maturity " + dateString,
                                MaturityDate = dateString,
                                EventType = ChangeEventType.Added
                            };
                            variances.Add(variance);
                        }
                        foreach (var maturity in inBoth)
                        {
                            var dateString = maturity.MaturityDateUTC.HasValue ? maturity.MaturityDateUTC.Value.ToShortDateString() : "";
                            var fromDb = fromDbMaturities.FirstOrDefault(x => x.GlobalMaturityID == maturity.GlobalMaturityID);
                            var updatedMaturity = updatedMaturities.FirstOrDefault(x => x.GlobalMaturityID == maturity.GlobalMaturityID);

                            List<ConcurrencyItem> diffs = updatedMaturity.ConcurrencyCompare(fromDb, baseName);

                            if (diffs != null && diffs.Any())
                            {
                                foreach (var diff in diffs)
                                {
                                    ConcurrencyItem variance = new ConcurrencyItem()
                                    {
                                        BaseModelName = baseName,
                                        Prop = "Maturity " + dateString + " " + diff.Prop,
                                        MaturityDate = dateString,
                                        EventType = ChangeEventType.Modified,
                                        FromDB = diff.FromDB,
                                        Updated = diff.Updated
                                    };
                                    variances.Add(variance);
                                }
                            }
                        }
                    }
                }
            }
            return variances;
        }

        public static ValueTask<object> SaveAs(this IJSRuntime js, string filename, byte[] data)
       => js.InvokeAsync<object>(
           "saveAsFile",
           filename,
           Convert.ToBase64String(data));

        public static T Clone<T>(this T toClone)
        {
            T ret;
            string tmpStr = JsonConvert.SerializeObject(toClone);
            ret = JsonConvert.DeserializeObject<T>(tmpStr);
            return ret;
        }

        public static void SetPropertyByName(this object obj, string name, object value)
        {
            var prop = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (null == prop || !prop.CanWrite) return;
            prop.SetValue(obj, value, null);
            return;
        }

        public static Dictionary<List<Maturity>, bool> TermMaturities(this List<Maturity> maturities)
        {
            maturities = maturities.OrderBy(x => x.MaturityDateUTC).ToList();
            Dictionary<List<Maturity>, bool> createTerms = new();
            List<Maturity> maturityProcess = new();

            // Nothing will be checked so only have to group by term id and termed
            for (int i = 0; i < maturities.Count; i++)
            {
                var current = maturities.ElementAt(i);

                if (current.IsTermed)
                {
                    if (maturityProcess?.Any() == true && current.TermId != maturityProcess?.FirstOrDefault()?.TermId)
                    {
                        createTerms.Add(maturityProcess, true);
                        maturityProcess = new();
                    }

                    maturityProcess?.Add(current);
                    if (current.Id == maturities.Last().Id)
                    {
                        createTerms.Add(maturityProcess, true);
                    }
                }
                else
                {
                    if (maturityProcess?.Any() == true)
                    {
                        createTerms.Add(maturityProcess, true);
                    }
                    createTerms.Add(new List<Maturity>() { current }, false);
                    maturityProcess = new();
                }
            }
            return createTerms;
        }
    }
}