using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Reporting.Models;

namespace Reporting.TripsByDurDistCatHierarchy
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
        /// Constructor for hour record
        /// </summary>
        /// <param name="dr">
        /// Dataset row
        /// </param>
        /// <param name="colnbr">
        /// Dictionary of column headings in dataset
        /// </param>
        public TripsByHourRow(DataRow dr, Dictionary<string, int> colnbr)
        {
            Hour2OfDay = new Hour2OfDayDimension(dr, colnbr);
            Bikes = new BikesMeasure(dr, colnbr);
        }
    }

    /// <summary>
    /// Hold query results for Trips by hour
    /// </summary>
    public class TripsByHourList : List<TripsByHourRow>
    {
        /// <summary>
        ///   Capture query to populate TripsByHourRow
        /// </summary>
        /// <returns>
        ///   MDX Query with parameters
        /// </returns>
        public string CommandText()
        {
            StringBuilder cmd = new StringBuilder();
            cmd.AppendLine("SELECT NON EMPTY { [Measures].[Bikes] } ");
            cmd.AppendLine("ON COLUMNS, ");
            cmd.AppendLine("NON EMPTY { ([Time Table].[Hour2ofday].[Hour2ofday].ALLMEMBERS ) } ");
            cmd.AppendLine("DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS ");
            cmd.AppendLine("FROM(SELECT (STRTOSET(@TripCatTripCategory, CONSTRAINED) ) ON COLUMNS ");
            cmd.AppendLine("     FROM(SELECT (STRTOSET(@StationPairDistanceQtrMiles, CONSTRAINED) ) ON COLUMNS ");
            cmd.AppendLine("          FROM(SELECT (STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED) ) ON COLUMNS ");
            cmd.AppendLine("               FROM(SELECT ( STRTOSET(@SubscribersSubscriberInfo, CONSTRAINED) ) ON COLUMNS ");
            cmd.AppendLine("                    FROM(SELECT ( STRTOSET(@TimeTableNameofday, CONSTRAINED) ) ON COLUMNS ");
            cmd.AppendLine("                         FROM[Bikeshare]");
            cmd.AppendLine("                        )");
            cmd.AppendLine("                   )");
            cmd.AppendLine("               )");
            cmd.AppendLine("         )");
            cmd.AppendLine("   )");
            cmd.AppendLine("WHERE(IIF( STRTOSET(@TimeTableNameofday, CONSTRAINED).Count = 1, ");
            cmd.AppendLine("           STRTOSET(@TimeTableNameofday, CONSTRAINED), ");
            cmd.AppendLine("           [Time Table].[Nameofday].currentmember ), ");
            cmd.AppendLine("      IIF(STRTOSET(@SubscribersSubscriberInfo, CONSTRAINED).Count = 1, ");
            cmd.AppendLine("           STRTOSET(@SubscribersSubscriberInfo, CONSTRAINED), ");
            cmd.AppendLine("           [Subscribers].[Subscriber Info].currentmember ), ");
            cmd.AppendLine("      IIF(STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED).Count = 1, ");
            cmd.AppendLine("           STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED), ");
            cmd.AppendLine("           [Station Pair Distance].[Mile Categories].currentmember ), ");
            cmd.AppendLine("      IIF(STRTOSET(@StationPairDistanceQtrMiles, CONSTRAINED).Count = 1, ");
            cmd.AppendLine("           STRTOSET(@StationPairDistanceQtrMiles, CONSTRAINED), ");
            cmd.AppendLine("           [Station Pair Distance].[Qtr Miles].currentmember ), ");
            cmd.AppendLine("      IIF(STRTOSET(@TripCatTripCategory, CONSTRAINED).Count = 1, ");
            cmd.AppendLine("           STRTOSET(@TripCatTripCategory, CONSTRAINED), ");
            cmd.AppendLine("           [TripCat].[Trip Category].currentmember ) ");
            cmd.AppendLine("     )");
            // construct paramterized query
            return (cmd.ToString());
        }

        public void GetData(TripsBySubscriberRow subscriberparm)
        {
            DataSet ds = new DataSet();
            using (AdomdConnection conn = new AdomdConnection("Data Source=miranda;Initial Catalog=bikesMD2"))
            {
                conn.Open();
                using (AdomdCommand cmd = new AdomdCommand(CommandText(), conn))
                {
                    cmd.Parameters.Add(new AdomdParameter("StationPairDistanceMileCategories", subscriberparm.Weekday.DistanceDurationCategory.DistanceCategory.MemberUniqueName));
                    cmd.Parameters.Add(new AdomdParameter("TripCatTripCategory", subscriberparm.Weekday.DistanceDurationCategory.DurationCategory.MemberUniqueName));
                    cmd.Parameters.Add(new AdomdParameter("StationPairDistanceQtrMiles", subscriberparm.Weekday.DistanceDurationCategory.DistanceQtrMi.MemberUniqueName));
                    cmd.Parameters.Add(new AdomdParameter("TimeTableNameofday", subscriberparm.Weekday.WeekdayName.MemberUniqueName));
                    cmd.Parameters.Add(new AdomdParameter("SubscribersSubscriberInfo", subscriberparm.Subscriber.MemberUniqueName));
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
                TripsByHourRow oneresult = new TripsByHourRow(dr, colnbr);

                Add(oneresult);
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
            TripHours.GetData(this);
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
        public string CommandText()
        {
            StringBuilder cmd = new StringBuilder();
            cmd.AppendLine("SELECT NON EMPTY { [Measures].[Bikes] } ON COLUMNS, ");
            cmd.AppendLine("       NON EMPTY { ([Subscribers].[Subscriber Info].[Subscriber Info].ALLMEMBERS ) } ");
            cmd.AppendLine("           DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS ");
            cmd.AppendLine("FROM ( SELECT ( STRTOSET(@TimeTableNameofday, CONSTRAINED) ) ON COLUMNS ");
            cmd.AppendLine("       FROM ( SELECT ( STRTOSET(@TripCatTripCategory, CONSTRAINED) ) ON COLUMNS ");
            cmd.AppendLine("              FROM ( SELECT ( STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED) ) ON COLUMNS ");
            cmd.AppendLine("                     FROM ( SELECT ( STRTOSET(@StationPairDistanceQtrMiles, CONSTRAINED) ) ON COLUMNS ");
            cmd.AppendLine("                            FROM [Bikeshare])))) ");
            cmd.AppendLine("WHERE ( IIF( STRTOSET(@StationPairDistanceQtrMiles, CONSTRAINED).Count = 1, ");
            cmd.AppendLine("             STRTOSET(@StationPairDistanceQtrMiles, CONSTRAINED), ");
            cmd.AppendLine("             [Station Pair Distance].[Qtr Miles].currentmember ), ");
            cmd.AppendLine("        IIF( STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED).Count = 1, ");
            cmd.AppendLine("             STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED), ");
            cmd.AppendLine("             [Station Pair Distance].[Mile Categories].currentmember ), ");
            cmd.AppendLine("        IIF( STRTOSET(@TripCatTripCategory, CONSTRAINED).Count = 1, ");
            cmd.AppendLine("             STRTOSET(@TripCatTripCategory, CONSTRAINED), ");
            cmd.AppendLine("             [TripCat].[Trip Category].currentmember ), ");
            cmd.AppendLine("        IIF( STRTOSET(@TimeTableNameofday, CONSTRAINED).Count = 1, ");
            cmd.AppendLine("             STRTOSET(@TimeTableNameofday, CONSTRAINED), ");
            cmd.AppendLine("             [Time Table].[Nameofday].currentmember ) )");
            return (cmd.ToString());
        }

        public void GetData(TripsByWeekdayRow weekdayparm)
        {
            DataSet ds = new DataSet();
            using (AdomdConnection conn = new AdomdConnection("Data Source=miranda;Initial Catalog=bikesMD2"))
            {
                conn.Open();
                using (AdomdCommand cmd = new AdomdCommand(CommandText(), conn))
                {
                    cmd.Parameters.Add(new AdomdParameter("StationPairDistanceMileCategories", weekdayparm.DistanceDurationCategory.DistanceCategory.MemberUniqueName));
                    cmd.Parameters.Add(new AdomdParameter("TripCatTripCategory", weekdayparm.DistanceDurationCategory.DurationCategory.MemberUniqueName));
                    cmd.Parameters.Add(new AdomdParameter("StationPairDistanceQtrMiles", weekdayparm.DistanceDurationCategory.DistanceQtrMi.MemberUniqueName));
                    cmd.Parameters.Add(new AdomdParameter("TimeTableNameofday", weekdayparm.WeekdayName.MemberUniqueName));
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
                TripsBySubscriberRow oneresult = new TripsBySubscriberRow(weekdayparm, dr, colnbr);

                Add(oneresult);
            }

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
            SubscriberList.GetData(this);
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
        public string CommandText()
        {
            StringBuilder cmd = new StringBuilder();
            cmd.AppendLine("SELECT NON EMPTY { [Measures].[Bikes] } ON COLUMNS, ");
            cmd.AppendLine("       NON EMPTY { ([Time Table].[Nameofday].[Nameofday].ALLMEMBERS ) } ");
            cmd.AppendLine("           DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS ");
            cmd.AppendLine("FROM ( SELECT ( STRTOSET(@TripCatTripCategory, CONSTRAINED) ) ON COLUMNS ");
            cmd.AppendLine("       FROM ( SELECT ( STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED) ) ON COLUMNS ");
            cmd.AppendLine("              FROM ( SELECT ( STRTOSET(@StationPairDistanceQtrMiles, CONSTRAINED) ) ON COLUMNS ");
            cmd.AppendLine("                     FROM [Bikeshare]))) ");
            cmd.AppendLine("WHERE ( IIF( STRTOSET(@StationPairDistanceQtrMiles, CONSTRAINED).Count = 1, ");
            cmd.AppendLine("             STRTOSET(@StationPairDistanceQtrMiles, CONSTRAINED), ");
            cmd.AppendLine("             [Station Pair Distance].[Qtr Miles].currentmember ), ");
            cmd.AppendLine("        IIF( STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED).Count = 1, ");
            cmd.AppendLine("             STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED), ");
            cmd.AppendLine("             [Station Pair Distance].[Mile Categories].currentmember ), ");
            cmd.AppendLine("        IIF( STRTOSET(@TripCatTripCategory, CONSTRAINED).Count = 1, ");
            cmd.AppendLine("             STRTOSET(@TripCatTripCategory, CONSTRAINED), ");
            cmd.AppendLine("             [TripCat].[Trip Category].currentmember ) )");
            return (cmd.ToString());
        }

        public void GetData(TripsByDistDurCatRow distdurparm)
        {
            DataSet ds = new DataSet();
            using (AdomdConnection conn = new AdomdConnection("Data Source=miranda;Initial Catalog=bikesMD2"))
            {
                conn.Open();
                using (AdomdCommand cmd = new AdomdCommand(CommandText(), conn))
                {
                    cmd.Parameters.Add(new AdomdParameter("StationPairDistanceMileCategories", distdurparm.DistanceCategory.MemberUniqueName));
                    cmd.Parameters.Add(new AdomdParameter("TripCatTripCategory", distdurparm.DurationCategory.MemberUniqueName));
                    cmd.Parameters.Add(new AdomdParameter("StationPairDistanceQtrMiles", distdurparm.DistanceQtrMi.MemberUniqueName));
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
                TripsByWeekdayRow oneresult = new TripsByWeekdayRow(distdurparm, dr, colnbr);

                Add(oneresult);
            }
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
            WeekdayList.GetData(this);
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
            StringBuilder cmd = new StringBuilder();
            cmd.AppendLine("SELECT NON EMPTY { [Measures].[Bikes] } ON COLUMNS, ");
            cmd.AppendLine("NON EMPTY { (");
            cmd.AppendLine("  [Station Pair Distance].[Mile Categories].[Mile Categories].ALLMEMBERS * ");
            cmd.AppendLine("  [Station Pair Distance].[Qtr Miles].[Qtr Miles].ALLMEMBERS * ");
            cmd.AppendLine("  [TripCat].[Trip Category].[Trip Category].ALLMEMBERS ");
            cmd.AppendLine(") } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS ");
            cmd.AppendLine("FROM [Bikeshare]");
            return (cmd.ToString());
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
                TripsByDistDurCatRow oneresult = new TripsByDistDurCatRow(dr, colnbr);
                Add(oneresult);
            }
        }

    }

}