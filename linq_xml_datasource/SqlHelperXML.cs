using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using System.IO;
using System.Web.Hosting;

namespace DataLayer
{
	public class EntryPoint
	{
		public int Id { get; set; }					// id of the entry point
		public int YieldCurveId { get; set; }		// id of the yield curve for which entry point is set
		public bool Enabled { get; set; }			// entry rate can be disabled manually or automatically if it breaks callibration 
		public string Type { get; set; }            // type=deposit, swap or bond
		public int Length { get; set; }             // Rate only, not for bonds. Length + timeUnit = rate maturity
		public string TimeUnit { get; set; }        // Rate only, not for bonds. timeUnit = {"Days", "Weeks", "Months" ,"Years"}
		public Instrument Instrument { get; set; }	// each point is either the rate or the bond
		public int DataProviderId { get; set; }
		public string DataReference { get; set; }
		public DateTime ValidDateBegin { get; set; }  //if the yc definition has been changed it indicates from what date the entry rate is valid
		public DateTime ValidDateEnd { get; set; }

		public HistoricValue epValue = null;		// to put here the last historic value for this point on/before the provided settlementDate

		private int duration;
		public int Duration
		{
			get
			{
				if (duration == 0)
				{
					if (0 == String.Compare(Type, "bond", StringComparison.OrdinalIgnoreCase))
					{
						DateTime dt1 = DateTime.Today;
						DateTime dt2 = (Instrument as Bond).MaturityDate;
						TimeSpan days = dt2.Subtract(dt1);
						duration = days.Days;
					}
					else
					{
						if (0 == String.Compare(TimeUnit, "Days", StringComparison.OrdinalIgnoreCase))
						{
							duration = Length;
						}
						else if (0 == String.Compare(TimeUnit, "Weeks", StringComparison.OrdinalIgnoreCase))
						{
							duration = Length * 7;
						}
						else if (0 == String.Compare(TimeUnit, "Months", StringComparison.OrdinalIgnoreCase))
						{
							DateTime dt1 = DateTime.Today;
							DateTime dt2 = dt1.AddMonths(Length);
							TimeSpan days = dt2.Subtract(dt1);
							duration = days.Days;
							//duration = Length * 30;
						}
						else if (0 == String.Compare(TimeUnit, "Years", StringComparison.OrdinalIgnoreCase))
						{
							DateTime dt1 = DateTime.Today;
							DateTime dt2 = dt1.AddYears(Length);
							TimeSpan days = dt2.Subtract(dt1);
							duration = days.Days;
							//  duration = Length * 365;
						}
					}
				}
				return duration;
			}

			set { duration = Duration; }
		}
	}

	public class EntryPointCompare : System.Collections.Generic.IComparer<EntryPoint>
	{
		public int Compare(EntryPoint p1, EntryPoint p2)
		{
			return p1.Duration - p2.Duration;
		}
	}

	//
	// the same but with whole History
	//

	public class HistoricValue
	{
		public DateTime Date { get; set; }
		public double Value { get; set; }
	}

	public class HistoricValueComparer : IEqualityComparer<HistoricValue>
	{
		public bool Equals(HistoricValue b1, HistoricValue b2)
		{
			return DateTime.Equals(b1.Date, b2.Date);
		}
		public int GetHashCode(HistoricValue item)
		{
			return item.Date.GetHashCode();
		}
	}

	public class EntryPointHistory : EntryPoint
	{
		public HashSet<HistoricValue> epValueHistory;
	}

	public class EntryPointHistoryCompare : System.Collections.Generic.IComparer<EntryPointHistory>
	{
		public int Compare(EntryPointHistory p1, EntryPointHistory p2)
		{
			return p1.Duration - p2.Duration;
		}
	}
	
	public static class Extensions		// used by ToHashSet() in linq
	{
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		{
			return new HashSet<T>(source);
		}
	}

    internal class SqlHelperXml
    {
		 static XDocument EnumCompoundingXml = XDocument.Load(path + "EnumCompounding.xml");
        static XDocument DayCounterXml = XDocument.Load(path + "DayCounter.xml");
        static XDocument EnumFrequencyXml = XDocument.Load(path + "EnumFrequency.xml");
        static XDocument EnumBusinessDayConventionXml = XDocument.Load(path + "EnumBusinessDayConvention.xml");
        static XDocument EnumDateGenerationXml = XDocument.Load(path + "EnumDateGeneration.xml");

        static XDocument CurrencyXml = XDocument.Load(path + "Currency.xml");
        static XDocument BondXml = XDocument.Load(path + "Bond.xml");
        static XDocument RateXml = XDocument.Load(path + "Rate.xml");

		//
		static XDocument YcEntryXml = XDocument.Load(path + "YcEntry.xml");
        static XDocument YcEntryHistoryXml = XDocument.Load(path + "YcEntryHistory.xml");
        
		// YieldCurve consists of the series EntryPoints (out List<EntryPoint> parameter), and this LINQ procedure will read the above XMLs to get all historic
		// data (out List<EntryPoint>) on a particular YieldCurve (idYc) which are the closest market values for the specified date (settlementDate) - e.g. on/before
		// this date:

		internal static void SelectYieldCurveEntryDataCommand(out List<EntryPoint> res, long? idYc, DateTime? settlementDate)
		{
			IEnumerable<EntryPoint> res0 =

			from yce in YcEntryXml.Descendants("ycentry")
			where ((idYc == null ? true : yce.Element("YieldCurveId").Value == idYc.ToString())
					&& (settlementDate == null 
						? true 
						: (Convert.ToDateTime(yce.Element("DateStart").Value) >= settlementDate 
								&& Convert.ToDateTime(yce.Element("DateFinish").Value) <= settlementDate)
						)
					)
			orderby (int)yce.Element("YieldCurveId") ascending

			select new EntryPoint
			{
				YieldCurveId = Convert.ToInt32(yce.Element("YieldCurveId").Value),
				Id = Convert.ToInt32(yce.Element("Id").Value),	// entry id, e.g. single point which will have several dates in ycentryhistory
				Type = yce.Element("Type").Value,
				Length = String.IsNullOrEmpty(yce.Element("Length").Value) ? 0 : Convert.ToInt32(yce.Element("Length").Value),
				TimeUnit = yce.Element("TimeUnit").Value,
				DataProviderId = String.IsNullOrEmpty(yce.Element("DataProviderId").Value) ? 0 : Convert.ToInt32(yce.Element("DataProviderId").Value),
				DataReference = yce.Element("DataReference").Value,
				Instrument = (0 == String.Compare(yce.Element("Type").Value, "bond", StringComparison.OrdinalIgnoreCase)
									? (Instrument)GetBond(Convert.ToInt32(yce.Element("RateId").Value))
									: (Instrument)GetRate(Convert.ToInt32(yce.Element("RateId").Value))
									),
				Enabled = true, //default
				ValidDateBegin = Convert.ToDateTime(yce.Element("DateStart").Value),
				ValidDateEnd = Convert.ToDateTime(yce.Element("DateFinish").Value),
				
				epValue = (
								from yceh in YcEntryHistoryXml.Descendants("ycentryhistory")
								where
								(
									(yceh.Element("YcEntryId").Value == yce.Element("Id").Value)
									&& (settlementDate == null
										? (0 == String.Compare(yce.Element("Type").Value, "bond", StringComparison.OrdinalIgnoreCase)
											? Convert.ToDateTime(yceh.Element("Date").Value) <= DateTime.Now
												&& GetBond(Convert.ToInt32(yce.Element("RateId").Value)).MaturityDate > DateTime.Now
											: Convert.ToDateTime(yceh.Element("Date").Value) <= DateTime.Now)
										: (0 == String.Compare(yce.Element("Type").Value, "bond", StringComparison.OrdinalIgnoreCase)
											? Convert.ToDateTime(yceh.Element("Date").Value) <= settlementDate.Value
												&& GetBond(Convert.ToInt32(yce.Element("RateId").Value)).MaturityDate > settlementDate.Value
											: Convert.ToDateTime(yceh.Element("Date").Value) <= settlementDate.Value)
										)
								)
								orderby (DateTime)yceh.Element("Date") descending

								select new HistoricValue
								{
									Date = Convert.ToDateTime(yceh.Element("Date").Value),
									Value = Convert.ToDouble(yceh.Element("Value").Value)
								}
							).ToList().Max()
			};

			res = res0.ToList();
			res.Sort(new EntryPointCompare());
		}
        
		// the same as above but instead of returning list of last entry points on or before settlement date 
		// now, along with entry point, we return its historic values as well (the HashSet) where we accumulate
		// all historic values on/before the settlement date
		
		// HashSet is because we have only single value on a particular date (Duration) and it is sorted by date
		
		internal static void SelectYieldCurveEntryDataHistoryCommand(out List<EntryPointHistory> res, long? idYc, DateTime? settlementDate)
		{
			IEnumerable<EntryPointHistory> res0 =

			from yce in YcEntryXml.Descendants("ycentry")
			where (idYc == null ? true : yce.Element("YieldCurveId").Value == idYc.ToString())
			orderby (int)yce.Element("YieldCurveId") ascending

			select new EntryPointHistory
			{
				YieldCurveId = Convert.ToInt32(yce.Element("YieldCurveId").Value),
				Id = Convert.ToInt32(yce.Element("Id").Value),	// entry id, e.g. single point which will have several dates in ycentryhistory
				Type = yce.Element("Type").Value,
				Length = String.IsNullOrEmpty(yce.Element("Length").Value) ? 0 : Convert.ToInt32(yce.Element("Length").Value),
				TimeUnit = yce.Element("TimeUnit").Value,
				DataProviderId = String.IsNullOrEmpty(yce.Element("DataProviderId").Value) ? 0 : Convert.ToInt32(yce.Element("DataProviderId").Value),
				DataReference = yce.Element("DataReference").Value,
				Instrument = (0 == String.Compare(yce.Element("Type").Value, "bond", StringComparison.OrdinalIgnoreCase)
									? (Instrument)GetBond(Convert.ToInt32(yce.Element("RateId").Value))
									: (Instrument)GetRate(Convert.ToInt32(yce.Element("RateId").Value))
									),

				epValueHistory = (
								from yceh in YcEntryHistoryXml.Descendants("ycentryhistory")
								where 
								(
									(yceh.Element("YcEntryId").Value == yce.Element("Id").Value)
									&& (settlementDate == null
										? (0 == String.Compare(yce.Element("Type").Value, "bond", StringComparison.OrdinalIgnoreCase)
											? Convert.ToDateTime(yceh.Element("Date").Value) <= DateTime.Now
												&& GetBond(Convert.ToInt32(yce.Element("RateId").Value)).MaturityDate > DateTime.Now
											: Convert.ToDateTime(yceh.Element("Date").Value) <= DateTime.Now)
										: (0 == String.Compare(yce.Element("Type").Value, "bond", StringComparison.OrdinalIgnoreCase)
											? Convert.ToDateTime(yceh.Element("Date").Value) <= settlementDate.Value
												&& GetBond(Convert.ToInt32(yce.Element("RateId").Value)).MaturityDate > settlementDate.Value
											: Convert.ToDateTime(yceh.Element("Date").Value) <= settlementDate.Value)
										)
								)
								orderby (DateTime)yceh.Element("Date") descending

								select new HistoricValue
								{
									Date = Convert.ToDateTime(yceh.Element("Date").Value),
									Value = Convert.ToDouble(yceh.Element("Value").Value)
								}
							).ToHashSet()
			};

			res = res0.ToList();
			res.Sort(new EntryPointHistoryCompare());
		}
		
		internal static Rate GetRate(long? idRate)
		{
            if (idRate != null && Repository.RateDic.ContainsKey(idRate.Value))
                return Repository.RateDic[idRate.Value];

			List<Rate> res = null;
			SelectRatesCommand(out res, idRate);
			return res[0];
		}

		internal static void SelectRatesCommand(out List<Rate> res, long? idRate)
        {
			IEnumerable<Rate> res0 =

			from r in RateXml.Descendants("rate")
			// left join --start
			join c in EnumCompoundingXml.Descendants("enumcompounding") on r.Element("CompoundingId").Value equals c.Element("Id").Value
				into leftJointC
				from c in leftJointC.DefaultIfEmpty()
				/*
			join b in DayCounterXml.Descendants("daycounter") on r.Element("BasisId").Value equals b.Element("Id").Value
				into leftJointB
				from b in leftJointB.DefaultIfEmpty
				 */
			join f in EnumFrequencyXml.Descendants("enumfrequency") on r.Element("FrequencyId").Value equals f.Element("Id").Value
				into leftJointF
				from f in leftJointF.DefaultIfEmpty()
			join r1 in RateXml.Descendants("rate") on r.Element("IndexId").Value equals r1.Element("Id").Value
				into leftJointR1
				from lr1 in leftJointR1.DefaultIfEmpty()  
			// the same: compounding/basis/frequency, but for Rate1
			join c1 in EnumCompoundingXml.Descendants("enumcompounding") 
				on (lr1 == null ? "" : lr1.Element("CompoundingId").Value) equals c1.Element("Id").Value
				into leftJointC1
				from lc1 in leftJointC1.DefaultIfEmpty()
				/*
			join b1 in DayCounterXml.Descendants("daycounter") 
				on (lr1 == null ? "" : lr1.Element("BasisId").Value) equals b1.Element("Id").Value
				into leftJointB1
				from lb1 in leftJointB1.DefaultIfEmpty() 
				 */
			join f1 in EnumFrequencyXml.Descendants("enumfrequency") 
				on (lr1 == null ? "" : lr1.Element("FrequencyId").Value) equals f1.Element("Id").Value
				into leftJointF1
				from lf1 in leftJointF1.DefaultIfEmpty() 
				// left join --end
			where (idRate == null ? true : r.Element("Id").Value == idRate.ToString())
			select new Rate 
			{
				Id = Convert.ToInt32(r.Element("Id").Value),
				Name = r.Element("Name").Value,
				ClassName = r.Element("ClassName").Value,
				Type = r.Element("Type").Value,
				DataProviderId = String.IsNullOrEmpty(r.Element("DataProviderId").Value) ? 0 : Convert.ToInt32(r.Element("DataProviderId").Value),
				DataReference = r.Element("DataReference").Value,
				IdCcy = String.IsNullOrEmpty(r.Element("CurrencyId").Value) ? 0 : Convert.ToInt32(r.Element("CurrencyId").Value),
				Duration = String.IsNullOrEmpty(r.Element("Length").Value) ? 0 : Convert.ToInt32(r.Element("Length").Value),
				TimeUnit = r.Element("TimeUnit").Value,
				Accuracy = String.IsNullOrEmpty(r.Element("Accuracy").Value) ? 0 : Convert.ToInt32(r.Element("Accuracy").Value),
				Spread = String.IsNullOrEmpty(r.Element("Spread").Value) ? 0 : Convert.ToDouble(r.Element("Spread").Value),
				SettlementDays = String.IsNullOrEmpty(r.Element("SettlementDays").Value) ? 0 : Convert.ToInt32(r.Element("SettlementDays").Value),
				//YieldCurveId = String.IsNullOrEmpty(r.Element("YieldCurveId").Value) ? 0 : Convert.ToInt32(r.Element("YieldCurveId").Value),
				FixingPlace = r.Element("FixingPlace").Value, //??? temporary solution. fixing place in database is id whereas here we are looking for quantlibclassname of calendar
				//IdFixingPlace=reader.GetInt32(reader.GetOrdinal("FixingPlace")),
				IdIndex = String.IsNullOrEmpty(r.Element("IndexId").Value) ? 0 : Convert.ToInt32(r.Element("IndexId").Value),
				/*
				Basis = (b == null ? null : new DayCounter
				{
					Id = Convert.ToInt32(b.Element("Id").Value),
					Name = b.Element("Name").Value,
					ClassName = b.Element("ClassName").Value
				}),*/
				BasisId = (String.IsNullOrEmpty(r.Element("BasisId").Value) ? -1 : Convert.ToInt32(r.Element("BasisId").Value)),

				Frequency = (f == null ? "" : f.Element("Name").Value),
				Compounding = (c == null ? "" : c.Element("Name").Value),
					
				IndexDuration = (lr1 == null || String.IsNullOrEmpty(lr1.Element("Length").Value)) ? 0 : Convert.ToInt32(lr1.Element("Length").Value),
				IndexTimeUnit = (lr1 == null ? "" : lr1.Element("TimeUnit").Value),
                IndexName = (lr1 == null ? "" : lr1.Element("Name").Value),
                ClassNameIndex = (lr1 == null ? "" : lr1.Element("ClassName").Value),
				/*
				BasisIndex = (lb1 == null ? null : new DayCounter
				{
					Id = Convert.ToInt32(lb1.Element("Id").Value),
					Name = lb1.Element("Name").Value,
					ClassName = lb1.Element("ClassName").Value
				}),*/
				BasisIndexId = (lr1 == null || String.IsNullOrEmpty(lr1.Element("BasisId").Value)) ? -1 : Convert.ToInt32(lr1.Element("BasisId").Value),

                FrequencyIndex = (lf1 == null ? "" : lf1.Element("Name").Value),
                CompoundingIndex = (lc1 == null ? "" : lc1.Element("Name").Value)
			};

			res = res0.ToList();
        }
	}
}
