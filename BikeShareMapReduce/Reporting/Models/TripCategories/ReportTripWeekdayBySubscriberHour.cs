using Microsoft.AnalysisServices.AdomdClient;
using Reporting.BikesBase;
using System;
using System.Collections.Generic;
using System.Data;

namespace Reporting.Models.TripCategories
{
    public class ViewWeekofyearGivenDistanceDurationCategories : TableWeekofyear
    {
        public string DistanceCategory { get; set; }
        public string DurationCategory { get; set; }
        public float Bikes { get; set; }

        public ViewWeekofyearGivenDistanceDurationCategories(string DistanceCategoryParm, string DurationCategoryParm)
        {

            string CommandText = @"SELECT 
NON EMPTY { [Measures].[Bikes] } ON COLUMNS, 
NON EMPTY { ([Station Pair Distance].[Mile Categories].[Mile Categories].ALLMEMBERS * 
             [TripCat].[Trip Category].[Trip Category].ALLMEMBERS ) 
          } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS 
FROM ( SELECT ( STRTOSET(@TripCatTripCategory, CONSTRAINED) ) ON COLUMNS 
       FROM ( SELECT ( STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED) ) ON COLUMNS 
FROM ( SELECT ( { [Direction].[Direction].&[A-B] } ) ON COLUMNS 
       FROM [Bikeshare]) 
       WHERE ( [Direction].[Direction].&[A-B] )))";
            DataSet ds = new DataSet();
            using (AdomdConnection conn = new AdomdConnection("Data Source=miranda;Initial Catalog=bikesMD2"))
            {
                conn.Open();
                using (AdomdCommand cmd = new AdomdCommand(CommandText, conn))
                {
                    cmd.Parameters.Add(new AdomdParameter("StationPairDistanceMileCategories", DistanceCategoryParm));
                    cmd.Parameters.Add(new AdomdParameter("TripCatTripCategory", DurationCategoryParm));
                    AdomdDataAdapter adapter = new AdomdDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                conn.Close();
            }
            DataTable dt = ds.Tables[0];
            Dictionary<string, int> colnbr = new Dictionary<string, int>();
            foreach (DataColumn dc in dt.Columns)
            {
                colnbr.Add(dc.ColumnName, dc.Ordinal);
            }
            foreach (DataRow dr in dt.Rows)
            {
                DistanceCategory = dr[colnbr["[Station Pair Distance].[Mile Categories].[Mile Categories].[MEMBER_CAPTION]"]].ToString();
                DurationCategory = dr[colnbr["[TripCat].[Trip Category].[Trip Category].[MEMBER_CAPTION]"]].ToString();
                Bikes = Convert.ToSingle(dr[colnbr["[Measures].[Bikes]"]]);
            }

        }
    }

    public class ReportTripWeekdayBySubscriberHour
    {
        public ViewWeekofyearGivenDistanceDurationCategories GetData(string DistanceCategory, string DurationCategory)
        {
            ViewWeekofyearGivenDistanceDurationCategories model = 
                 new ViewWeekofyearGivenDistanceDurationCategories(DistanceCategory, DurationCategory);

            List<BikeTable> hierarchy = new List<BikeTable>();
            hierarchy.Add(new TableWeekday());
            hierarchy.Add(new TableSubscriber());
            hierarchy.Add(new TableHour2());

            string CommandText = @"SELECT 
NON EMPTY { [Measures].[Bikes] } ON COLUMNS, 
NON EMPTY { ([Time Table].[Nameofday].[Nameofday].ALLMEMBERS * 
             [Time Table].[Weekofyear].[Weekofyear].ALLMEMBERS * 
             [Subscribers].[Subscriber Info].[Subscriber Info].ALLMEMBERS * 
             [Time Table].[Hour2ofday].[Hour2ofday].ALLMEMBERS ) 
          } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS 
FROM ( SELECT ( { [Direction].[Direction].&[A-B] } ) ON COLUMNS 
       FROM ( SELECT ( STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED) ) ON COLUMNS 
              FROM ( SELECT ( STRTOSET(@TripCatTripCategory, CONSTRAINED) ) ON COLUMNS 
                     FROM [Bikeshare]))) 
WHERE ( IIF( STRTOSET(@TripCatTripCategory, CONSTRAINED).Count = 1, 
             STRTOSET(@TripCatTripCategory, CONSTRAINED), 
             [TripCat].[Trip Category].currentmember ), 
        IIF( STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED).Count = 1, 
             STRTOSET(@StationPairDistanceMileCategories, CONSTRAINED), 
             [Station Pair Distance].[Mile Categories].currentmember ), 
        [Direction].[Direction].&[A-B] )";

            DataSet ds = new DataSet();
            using (AdomdConnection conn = new AdomdConnection("Data Source=miranda;Initial Catalog=bikesMD2"))
            {
                conn.Open();
                using (AdomdCommand cmd = new AdomdCommand(CommandText, conn))
                {
                    cmd.Parameters.Add(new AdomdParameter("StationPairDistanceMileCategories", DistanceCategory));
                    cmd.Parameters.Add(new AdomdParameter("TripCatTripCategory", DurationCategory));
                    AdomdDataAdapter adapter = new AdomdDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                conn.Close();
            }
            PopulateModel(hierarchy, model, ds);

            return model;
        }

        private static void PopulateModel(List<BikeTable> hierarchy, BikeTable model, DataSet ds)
        {
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
                model.UpdateForDR(hierarchy, 0, dr, colnbr);
            }
        }

    }
}