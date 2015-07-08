﻿using Microsoft.AnalysisServices.AdomdClient;
using Reporting.BikesBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Reporting.Models.TripCategories
{
    public class ReportTripCategoriesBySubscriberHour
    {
        public string CommandText()
        {
            string cmd = @"SELECT 
NON EMPTY { [Measures].[Bikes] } ON COLUMNS, 
NON EMPTY { ([Station Pair Distance].[Mile Categories].[Mile Categories].ALLMEMBERS * 
             [TripCat].[Trip Category].[Trip Category].ALLMEMBERS * 
             [Subscribers].[Subscriber Info].[Subscriber Info].ALLMEMBERS * 
             [Time Table].[Hour2ofday].[Hour2ofday].ALLMEMBERS ) 
          } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS 
FROM [Bikeshare]";
            return cmd;
        }

        public TableTripCategories GetData()
        {
            TableTripCategories model = new TableTripCategories();
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
                model.UpdateForDR(dr, colnbr);
            }

            return (model);
        }
    }
}