using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Reporting.Models;

namespace Reporting.TripsByDurDistCatListBuilder
{
    public class DistanceCategoryDimension : OlapVariable
    {
        public DistanceCategoryDimension(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Station Pair Distance].[Mile Categories].[Mile Categories]") { }

        public string GetValue()
        {
            return MemberCaption;
        }
    }

    public class SubscriberDimension : OlapVariable
    {

        public SubscriberDimension(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Subscribers].[Subscriber Info].[Subscriber Info]") { }

        public string GetValue()
        {
            return MemberCaption;
        }
    }

    public class DurationCategoryDimension : OlapVariable
    {
        public DurationCategoryDimension(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[TripCat].[Trip Category].[Trip Category]") { }

        public string GetValue()
        {
            return MemberCaption;
        }
    }

    public class WeekdayNameDimension : OlapVariable
    {
        public WeekdayNameDimension(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Time Table].[Nameofday].[Nameofday]") { }

        public string GetValue()
        {
            return MemberCaption;
        }
    }

    public class DistanceQtrMileDimension : OlapVariable
    {
        public DistanceQtrMileDimension(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Station Pair Distance].[Qtr Miles].[Qtr Miles]") { }

        public float GetValue()
        {
            return Convert.ToSingle(MemberCaption);
        }
    }

    public class Hour2OfDayDimension : OlapVariable
    {
        public Hour2OfDayDimension(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Time Table].[Hour2ofday].[Hour2ofday]") { }

        public int GetValue()
        {
            return Convert.ToInt32(MemberCaption);
        }
    }

    public class BikesMeasure : OlapVariable
    {
        public BikesMeasure(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Measures].[Bikes]") { }

        public float GetValue()
        {
            return Convert.ToSingle(MemberCaption);
        }

        internal void IncValue(float v)
        {
            string newcaption = (GetValue() + v).ToString();
            MemberUniqueName.Replace(MemberCaption, newcaption);
            MemberCaption = newcaption;
        }
    }


    /// <summary>
    /// Rows for Bike trips by hour
    /// </summary>
    /// <remarks>
    /// TBD: Refactor into separate namespace as each report will have a unique population of objects
    /// </remarks>
    public class TripsByHourRow
    {
        /// <summary>
        /// Time periods of day in 2-hour intervals
        /// Goal to populate column chart to visualize diurnal pattern
        /// </summary>
        /// <remarks>
        /// Reasoning is traffic periods in 4-hour intervals (such as commute periods)
        /// Cut in half to allow flexibility in defining start of 4-hour block
        /// </remarks>
        public Hour2OfDayDimension Hour2OfDay;

        /// <summary>
        /// Bikes summarized at hour level
        /// </summary>
        public BikesMeasure Bikes;

        /// <summary>
        /// Reference back to owner of this list
        /// </summary>
        public TripsBySubscriberRow Subscriber;

        /// <summary>
        /// Constructor for hour record
        /// </summary>
        /// <param name="dr">
        /// Dataset row
        /// </param>
        /// <param name="colnbr">
        /// Dictionary of column headings in dataset
        /// </param>
        /// <param name="subs">
        /// back reference to subscriber with this list
        /// </param>
        public TripsByHourRow(TripsBySubscriberRow subs, DataRow dr, Dictionary<string, int> colnbr)
        {
            Hour2OfDay = new Hour2OfDayDimension(dr, colnbr);
            Bikes = new BikesMeasure(dr, colnbr);
            Subscriber = subs;
        }
    }

    /// <summary>
    /// Hold query results for Trips by hour
    /// </summary>
    public class TripsByHourList : List<TripsByHourRow>
    {
        internal void UpdateForDR(TripsBySubscriberRow subs, DataRow dr, Dictionary<string, int> colnbr)
        {
            TripsByHourRow hour = new TripsByHourRow(subs, dr, colnbr);
            TripsByHourRow selhour;
            if (Exists(x => x.Hour2OfDay.MemberUniqueName == hour.Hour2OfDay.MemberUniqueName))
            {
                selhour = Find(x => x.Hour2OfDay.MemberUniqueName == hour.Hour2OfDay.MemberUniqueName);
                selhour.Bikes.IncValue(hour.Bikes.GetValue()); // Should not occur not deeper hierarchy to accumulate
            }
            else
            {
                Add(hour);
                selhour = hour;
            }
        }
    }


    /// <summary>
    /// Traffic for subscribers
    /// </summary>
    public class TripsBySubscriberRow
    {
        /// <summary>
        /// Populate Subscriber data
        /// </summary>
        /// <param name="dr">
        /// rowset for query
        /// </param>
        /// <param name="colnbr">
        /// dictionary mapping column labels to numbers
        /// </param>
        public TripsBySubscriberRow(TripsByWeekdayRow weekday, DataRow dr, Dictionary<string, int> colnbr)
        {
            Weekday = weekday;
            Subscriber = new SubscriberDimension(dr, colnbr);
            Bikes = new BikesMeasure(dr, colnbr);
            TripHours = new TripsByHourList();
        }

        /// <summary>
        /// Parent weekday record that will include list of subscribers
        /// </summary>
        public TripsByWeekdayRow Weekday;

        /// <summary>
        /// Subscriber type registered or casual
        /// </summary>
        public SubscriberDimension Subscriber;

        /// <summary>
        /// Bikes summarized at hour level
        /// </summary>
        public BikesMeasure Bikes;

        /// <summary>
        /// List of TripsByHourRow
        /// </summary>
        public TripsByHourList TripHours;
    }

    public class TripsBySubscriberList : List<TripsBySubscriberRow>
    {
        internal void UpdateForDR(TripsByWeekdayRow weekday, DataRow dr, Dictionary<string, int> colnbr)
        {
            TripsBySubscriberRow subs = new TripsBySubscriberRow(weekday, dr, colnbr);
            TripsBySubscriberRow selsubs;
            if (Exists(x => x.Subscriber.MemberUniqueName == subs.Subscriber.MemberUniqueName))
            {
                selsubs = Find(x => x.Subscriber.MemberUniqueName == subs.Subscriber.MemberUniqueName);
                selsubs.Bikes.IncValue(subs.Bikes.GetValue());
            }
            else
            {
                Add(subs);
                selsubs = subs;
            }
            selsubs.TripHours.UpdateForDR(subs, dr, colnbr);
        }
    }

    public class TripsByWeekdayRow
    {
        public TripsByWeekdayRow(TripsByDistDurCatRow distanceDurationCategory, DataRow dr, Dictionary<string, int> colnbr)
        {
            DistanceDurationCategory = distanceDurationCategory;
            WeekdayName = new WeekdayNameDimension(dr, colnbr);
            Bikes = new BikesMeasure(dr, colnbr);
            SubscriberList = new TripsBySubscriberList();
        }

        /// <summary>
        /// Parent row needing weekdays populated
        /// </summary>
        public TripsByDistDurCatRow DistanceDurationCategory;

        /// <summary>
        /// WeekdayName
        /// </summary>
        /// <remarks>
        /// Will rely on query to use DayOfWeek number to order weekdays appropriately
        /// </remarks>
        public WeekdayNameDimension WeekdayName;

        /// <summary>
        /// Bike trips
        /// </summary>
        public BikesMeasure Bikes;

        /// <summary>
        /// Subscribers for this particular day
        /// </summary>
        public TripsBySubscriberList SubscriberList;
    }

    public class TripsByWeekdayList : List<TripsByWeekdayRow>
    {
        internal void UpdateForDR(TripsByDistDurCatRow distdurcat, DataRow dr, Dictionary<string, int> colnbr)
        {
            TripsByWeekdayRow weekday = new TripsByWeekdayRow(distdurcat, dr, colnbr);
            TripsByWeekdayRow selweekday;
            if (Exists(x =>
                x.WeekdayName.MemberUniqueName == weekday.WeekdayName.MemberUniqueName))
            {
                selweekday = Find(x =>
                x.WeekdayName.MemberUniqueName == weekday.WeekdayName.MemberUniqueName);
                selweekday.Bikes.IncValue(weekday.Bikes.GetValue());
            }
            else
            {
                Add(weekday);
                selweekday = weekday;
            }
            selweekday.SubscriberList.UpdateForDR(weekday, dr, colnbr);

        }
    }

    public class TripsByDistDurCatRow
    {
        /// <summary>
        /// Constructor for row
        /// </summary>
        /// <param name="dr">
        /// rowset for record
        /// </param>
        /// <param name="colnbr">
        /// dictionary mapping column names to numbers
        /// </param>
        public TripsByDistDurCatRow(DataRow dr, Dictionary<string, int> colnbr)
        {
            DistanceCategory = new DistanceCategoryDimension(dr, colnbr);
            DurationCategory = new DurationCategoryDimension(dr, colnbr);
            DistanceQtrMi = new DistanceQtrMileDimension(dr, colnbr);
            Bikes = new BikesMeasure(dr, colnbr);

            WeekdayList = new TripsByWeekdayList();
        }

        /// <summary>
        /// Distance category in broad terms
        /// </summary>
        public DistanceCategoryDimension DistanceCategory;

        /// <summary>
        /// Duration category depending on whether trip involved a stop
        /// </summary>
        public DurationCategoryDimension DurationCategory;

        /// <summary>
        /// Bike trips
        /// </summary>
        public BikesMeasure Bikes;

        /// <summary>
        /// Distance in 1/4 mile increments
        /// </summary>
        public DistanceQtrMileDimension DistanceQtrMi;

        /// <summary>
        /// Weekdays for this category
        /// </summary>
        public TripsByWeekdayList WeekdayList;

    }

    public class TripsByDistDurCatList : List<TripsByDistDurCatRow>
    {
        public string CommandText()
        {
            string cmd = @"SELECT 
NON EMPTY { [Measures].[Bikes] } ON COLUMNS, 
NON EMPTY { ([Station Pair Distance].[Mile Categories].[Mile Categories].ALLMEMBERS * 
             [TripCat].[Trip Category].[Trip Category].ALLMEMBERS * 
             [Station Pair Distance].[Qtr Miles].[Qtr Miles].ALLMEMBERS * 
             [Time Table].[Nameofday].[Nameofday].ALLMEMBERS * 
             [Subscribers].[Subscriber Info].[Subscriber Info].ALLMEMBERS * 
             [Time Table].[Hour2ofday].[Hour2ofday].ALLMEMBERS ) } 
    DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS 
FROM [Bikeshare]
";
            return (cmd);
        }

        public void GetData()
        {
            DataSet ds = new DataSet();
            using (AdomdConnection conn = new AdomdConnection("Data Source=miranda;Initial Catalog=bikesMD2"))
            {
                conn.Open();
                using (AdomdCommand cmd = new AdomdCommand(CommandText(), conn))
                {
                    AdomdDataAdapter adapter = new AdomdDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                conn.Close();
            }
            // Single query returns one table
            DataTable dt = ds.Tables[0];
            // read column names to use to select correct fields for data
            // TBD Inspect here to find column names for pivoted Dayname-bikes
            Dictionary<string, int> colnbr = new Dictionary<string, int>();
            foreach (DataColumn dc in dt.Columns)
            {
                colnbr.Add(dc.ColumnName, dc.Ordinal);
            }
            foreach (DataRow dr in dt.Rows)
            {
                UpdateListForDR(dr, colnbr);
            }


        }

        public void UpdateListForDR(DataRow dr, Dictionary<string, int> colnbr)
        {
            TripsByDistDurCatRow distdurcat = new TripsByDistDurCatRow(dr, colnbr);
            TripsByDistDurCatRow seldistdurcat;
            // if record already exists, then use existing, else add this one
            if (Exists(x =>
                  ((x.DistanceCategory.MemberUniqueName == distdurcat.DistanceCategory.MemberUniqueName) &
                   (x.DurationCategory.MemberUniqueName == distdurcat.DurationCategory.MemberUniqueName) &
                   (x.DistanceQtrMi.MemberUniqueName == distdurcat.DistanceQtrMi.MemberUniqueName))))
            {
                seldistdurcat = Find(x =>
                  ((x.DistanceCategory.MemberUniqueName == distdurcat.DistanceCategory.MemberUniqueName) &
                   (x.DurationCategory.MemberUniqueName == distdurcat.DurationCategory.MemberUniqueName) &
                   (x.DistanceQtrMi.MemberUniqueName == distdurcat.DistanceQtrMi.MemberUniqueName)));
                seldistdurcat.Bikes.IncValue(distdurcat.Bikes.GetValue());
            }
            else
            {
                Add(distdurcat);
                seldistdurcat = distdurcat;
            }
            seldistdurcat.WeekdayList.UpdateForDR(distdurcat, dr, colnbr);
        }
    }
}