using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Reporting.Data
{
    public class Reporting
    {
        // Following example from https://datatellblog.wordpress.com/2015/04/21/surfacing-ssas-data-with-google-charts/
        // google charts created on client side javascript
        // for now, just present a table
        // chart alternatives http://www.creativebloq.com/design-tools/data-visualization-712402
        public static List<object> FirstReport()
        {
            // removed ', MEMBER_UNIQUE_NAME' from ON ROWS
            string command = @" 
           SELECT NON EMPTY { [Measures].[Bikes] } ON COLUMNS, 
                  NON EMPTY { ([CentroidA].[Level3 Locality].[Level3 Locality].ALLMEMBERS * 
                               [CentroidA].[Level3 ID].[Level3 ID].ALLMEMBERS * 
                               [CentroidB].[Level3 Locality].[Level3 Locality].ALLMEMBERS * 
                               [CentroidB].[Level3 ID].[Level3 ID].ALLMEMBERS * 
                               [Direction].[Direction].[Direction].ALLMEMBERS ) } 
                       DIMENSION PROPERTIES MEMBER_CAPTION ON ROWS 
           FROM [Bikeshare]";
            return GetData(command);
        }


        public static List<object> GetData(string command)
        {
            DataSet ds = new DataSet();

            using (AdomdConnection conn = new AdomdConnection("Data Source=miranda;Initial Catalog=bikesMD2"))
            {
                conn.Open();
                using (AdomdCommand cmd = new AdomdCommand(command, conn))
                {
                    AdomdDataAdapter adapter = new AdomdDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                conn.Close();
            }

            return ConvertDataTableToObjectList(ds.Tables[0]);
        }

        private static List<object> ConvertDataTableToObjectList(DataTable dt)
        {
            List<object> data = new List<object>();
            int columnCount = dt.Columns.Count;

            string[] columnObject = new string[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                string name = dt.Columns[i].ColumnName.Replace(".[MEMBER_CAPTION]", "");
                string[] nameParts = name.Split(new char[] { '.' });

                columnObject[i] = nameParts[nameParts.Count() - 1].Replace("[", "").Replace("]", "");
            }
            data.Add(columnObject);

            foreach (DataRow row in dt.Rows)
            {
                data.Add(row.ItemArray);
            }

            return data;
        }
    }

}