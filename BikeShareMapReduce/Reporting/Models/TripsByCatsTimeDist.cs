using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using Reporting.Models;

namespace Reporting.Models
{
    public class TripsByCatsTimeDistDistanceCategory : OlapVariable
    {
        public TripsByCatsTimeDistDistanceCategory(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Station Pair Distance].[Mile Categories].[Mile Categories]") { }

        public string GetValue()
        {
            return MemberCaption;
        }
    }

    public class TripsByCatsTimeDistSubscriber : OlapVariable
    {

        public TripsByCatsTimeDistSubscriber(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Subscribers].[Subscriber Info].[Subscriber Info]") { }

        public string GetValue()
        {
            return MemberCaption;
        }
    }

    public class TripsByCatsTimeDistDurationCategory : OlapVariable
    {
        public TripsByCatsTimeDistDurationCategory(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Subscribers].[Subscriber Info].[Subscriber Info]") { }

        public string GetValue()
        {
            return MemberCaption;
        }
    }

    public class TripsByCatsTimeDistWeekdayName : OlapVariable
    {
        public TripsByCatsTimeDistWeekdayName(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Time Table].[Nameofday].[Nameofday]") { }

        public string GetValue()
        {
            return MemberCaption;
        }
    }

    public class TripsByCatsTimeDistQtrMi : OlapVariable
    {
        public TripsByCatsTimeDistQtrMi(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Station Pair Distance].[Qtr Miles].[Qtr Miles]") { }

        public float GetValue()
        {
            return Convert.ToSingle(MemberCaption);
        }
    }

    public class TripsByCatsTimeDistHour2OfDay : OlapVariable
    {
        public TripsByCatsTimeDistHour2OfDay(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Time Table].[Hour2ofday].[Hour2ofday]") { }

        public int GetValue()
        {
            return Convert.ToInt32(MemberCaption);
        }
    }

    public class TripsByCatsTimeDistBikes : OlapVariable
    {
        public TripsByCatsTimeDistBikes(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Measures].[Bikes]") { }

        public float GetValue()
        {
            return Convert.ToSingle(MemberCaption);
        }
    }


    /// <summary>
    /// Recordset for Summary of trips by distance and duration over days and hours
    /// </summary>
    /// <remarks>
    /// Class to capture rows returned from MDX query
    /// Intended to facilitate populating the View using Razor
    /// TBD: want both value and the unique name, especially for numeric dimensions
    /// </remarks>
    public class TripByCatTimeDistRow
    {
        /// <summary>
        /// Categories distinquish neighborhood, last-mile, commute, excursion
        /// </summary>
        public TripsByCatsTimeDistDistanceCategory DistanceCategory;

        /// <summary>
        /// Subscriber type registered or casual
        /// </summary>
        public TripsByCatsTimeDistSubscriber Subscriber;

        /// <summary>
        /// Durations below threshold labeled as nonstop, else split into begin and end
        /// </summary>
        public TripsByCatsTimeDistDurationCategory DurationCategory;

        /// <summary>
        /// Name of day of week
        /// </summary>
        public TripsByCatsTimeDistWeekdayName WeekdayName;

        /// <summary>
        /// Computed distances between stations quantized to quarter miles
        /// </summary>
        /// <remarks>
        /// Most trips at or below 1.5 miles or 6 bins
        /// Some trips up to 10 miles or 40 bins
        /// </remarks>
        public TripsByCatsTimeDistQtrMi DistanceQtrMi;

        /// <summary>
        /// Quantize time of day into 2-hour intervals
        /// </summary>
        /// <remarks>
        /// Peak periods generally around 4-hour blocks
        /// 2-hour split allow for defining when 4-hour blocks begin
        /// </remarks>
        public TripsByCatsTimeDistHour2OfDay Hour2OfDay;

        /// <summary>
        /// Bike trips
        /// </summary>
        /// <remarks>
        /// View must pivot days as columns using Linq
        /// </remarks>
        public TripsByCatsTimeDistBikes Bikes;

        /// <summary>
        /// Constructor for row based on single recordset row
        /// </summary>
        /// <param name="dr">
        /// dataset row of query result values
        /// </param>
        /// <param name="colnbr">
        /// dictionary to give indexes for column names
        /// </param>
        public TripByCatTimeDistRow(DataRow dr, Dictionary<string, int> colnbr)
        {
            Subscriber = new TripsByCatsTimeDistSubscriber(dr, colnbr);
            DistanceCategory = new TripsByCatsTimeDistDistanceCategory(dr, colnbr);
            DurationCategory = new TripsByCatsTimeDistDurationCategory(dr, colnbr);
            WeekdayName = new TripsByCatsTimeDistWeekdayName(dr, colnbr);
            DistanceQtrMi = new TripsByCatsTimeDistQtrMi(dr, colnbr);
            Hour2OfDay = new TripsByCatsTimeDistHour2OfDay(dr, colnbr);
            Bikes = new TripsByCatsTimeDistBikes(dr, colnbr);
        }

    }

    /// <summary>
    /// Summary results for distance and duration categories by hour and day
    /// </summary>
    public class TripsByCatsTimeDist : List<TripByCatTimeDistRow>
    {
        /// <summary>
        /// Query to retrieve summary data for table
        /// </summary>
        /// <returns>Query string for SSAS</returns>
        public string CommandText()
        {
            StringBuilder cmd = new StringBuilder();
            cmd.Append("SELECT NON EMPTY { ");
            cmd.Append("    [Measures].[Bikes]");
            cmd.Append("} ON COLUMNS, ");
            cmd.Append("NON EMPTY { (");
            cmd.Append("   [Time Table].[Nameofday].[Nameofday].ALLMEMBERS * ");
            cmd.Append("   [Station Pair Distance].[Qtr Miles].[Qtr Miles].ALLMEMBERS * ");
            cmd.Append("   [Time Table].[Hour2ofday].[Hour2ofday].ALLMEMBERS * ");
            cmd.Append("   [TripCat].[Trip Category].[Trip Category].ALLMEMBERS * ");
            cmd.Append("   [Subscribers].[Subscriber Info].[Subscriber Info].ALLMEMBERS * ");
            cmd.Append("   [Station Pair Distance].[Mile Categories].[Mile Categories].ALLMEMBERS ");
            cmd.Append("   ) } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS ");
            cmd.Append("FROM [Bikeshare]");
            return cmd.ToString();
        }
        /// <summary>
        /// Execute MDX query and populate model with results
        /// </summary>
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
                TripByCatTimeDistRow oneresult = new TripByCatTimeDistRow(dr, colnbr);

                Add(oneresult);
            }

        }
    }
}