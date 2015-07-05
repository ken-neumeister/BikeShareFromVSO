using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;

namespace Reporting.Models
{
    public class L2ABwHrWD
    {

        public string Level2A { get; set; }

        public string Level2B { get; set; }

        public int HourOfDay { get; set; }

        public int DayOfWeek { get; set; }

        public string Weekday { get; set; }

        public float Bikes { get; set; }

        public L2ABwHrWD(string L2A, string L2B, int HOD, int DOW, string WD, float B)
        {
            Level2A = L2A;
            Level2B = L2B;
            HourOfDay = HOD;
            DayOfWeek = DOW;
            Weekday = WD;
            Bikes = B;
        }

        public L2ABwHrWD(DataRow dr, Dictionary<string, int> colnbr)
        {
            int col;
            if (colnbr.TryGetValue("[CentroidA].[Level2 Locality].[Level2 Locality].[MEMBER_CAPTION]", out col))
            {
                Level2A = (string)dr[col];
            }
            if (colnbr.TryGetValue("[CentroidB].[Level2 Locality].[Level2 Locality].[MEMBER_CAPTION]", out col))
            {
                Level2B = (string)dr[col];
            }
            if (colnbr.TryGetValue("[Time Table].[Nameofday].[Nameofday].[MEMBER_CAPTION]", out col))
            {
                Weekday = (string)dr[col];
            }
            if (colnbr.TryGetValue("[Time Table].[Dayofweek].[Dayofweek].[MEMBER_CAPTION]", out col))
            {
                DayOfWeek = Convert.ToInt32(dr[col]);
            }
            if (colnbr.TryGetValue("[Time Table].[Hourofday].[Hourofday].[MEMBER_CAPTION]", out col))
            {
                HourOfDay = Convert.ToInt32(dr[col]);
            }
            if (colnbr.TryGetValue("[Measures].[Bikes]", out col))
            {
                Bikes = Convert.ToSingle(dr[col]);
            }
        }
    }

    public class L3ABwHrWDParams
    {

        //public static string friendly(string queryname)
        //{
        //    string[] nameparts = queryname.Split(new char[] { '.' });
        //    string friendly = nameparts[nameparts.Count() - 1].TrimStart(new char[] { '[', '&' }).TrimEnd(new char[] { ']' }); //Replace("[", "").Replace("]", "");
        //    return (friendly);
        //}

        public OlapQueryParameter Level3A { get; set; }
        public OlapQueryParameter Level3B { get; set; }
        public OlapQueryParameter Direction { get; set; }

        //public string Level3AName () { return friendly(Level3A); }
        //public string Level3BName() { return friendly(Level3B); }
        //public string DirectionName() { return friendly(Direction); }

        public L3ABwHrWDParams(string Level3A, string Level3B, string Direction)
        {
            this.Level3A = new OlapQueryParameter(Level3A);
            this.Level3B = new OlapQueryParameter(Level3B);
            this.Direction = new OlapQueryParameter(Direction);
        }

        public L3ABwHrWDParams()  // Visual studio generated this
        {
        }

        public L3ABwHrWDParams(L3ABwHrWDParams l3params)
        {
            Level3A = new OlapQueryParameter(l3params.Level3A.UniqueName);
            Level3B = new OlapQueryParameter(l3params.Level3B.UniqueName);
            Direction = new OlapQueryParameter(l3params.Direction.UniqueName);
        }
    }

    public class L2ABwHrWDReport
    {
        public List<L2ABwHrWD> results;
        public L3ABwHrWDParams queryparameters;  // Now query parameters are in the model

        public L2ABwHrWDReport()
        {
            results = new List<L2ABwHrWD>();
        }

        public L2ABwHrWDReport(string l3A, string l3B, string dir)
        {
            results = new List<L2ABwHrWD>();
            queryparameters = new L3ABwHrWDParams(l3A, l3B, dir);
        }

        public L2ABwHrWDReport(L3ABwHrWDParams l3params)
        {
            results = new List<L2ABwHrWD>();
            queryparameters = new L3ABwHrWDParams(l3params);
        }


        //public void L2ABHrWDReport()
        //{
        //    // new does not invoke tihs constructor?
        //    // examples do not say return type needed for constructor, but complains here.
        //    results = new List<L2ABwHrWD>();
        //}

        public List<L2ABwHrWD>.Enumerator GetEnumerator()
        {
            return results.GetEnumerator();
        }

        public void GetData() // was (L3ABwHrWDParams param)
        {
            // TBD, make view-model consisting of replicating L3 results and details for L2 results
            // 1) create view-model model
            // 2) populate L3-model sub-model
            // 3) populate L2-model sub-model
            // 
            DataSet ds = new DataSet();
            // work-around for non-functioning constructor: results = new List<L2ABwHrWD>();

            string command = @"
                SELECT NON EMPTY { [Measures].[Bikes] } ON COLUMNS, 
                       NON EMPTY { ([CentroidA].[Level2 Locality].[Level2 Locality].ALLMEMBERS * 
                                    [CentroidB].[Level2 Locality].[Level2 Locality].ALLMEMBERS * 
                                    [Time Table].[Hourofday].[Hourofday].ALLMEMBERS * 
                                    [Time Table].[Nameofday].[Nameofday].ALLMEMBERS *  
                                    [Time Table].[Dayofweek].[Dayofweek].ALLMEMBERS ) 
                                  } DIMENSION PROPERTIES MEMBER_CAPTION ON ROWS 
                FROM 
                  ( 
                   SELECT ( STRTOSET(@DirectionDirection, CONSTRAINED) ) ON COLUMNS 
                   FROM ( 
                     SELECT ( STRTOSET(@CentroidBLevelLocality, CONSTRAINED) ) ON COLUMNS 
                     FROM ( 
                       SELECT ( STRTOSET(@CentroidALevelLocality, CONSTRAINED) ) ON COLUMNS 
                       FROM [Bikeshare]
                              )
                          )
                        ) 
                   WHERE ( 
                     IIF( 
                       STRTOSET(@CentroidALevelLocality, CONSTRAINED).Count = 1, 
                       STRTOSET(@CentroidALevelLocality, CONSTRAINED), 
                       [CentroidA].[Level3 Locality].currentmember 
                         ), 
                     IIF( 
                       STRTOSET(@CentroidBLevelLocality, CONSTRAINED).Count = 1, 
                       STRTOSET(@CentroidBLevelLocality, CONSTRAINED), 
                       [CentroidB].[Level3 Locality].currentmember 
                         ), 
                     IIF( 
                       STRTOSET(@DirectionDirection, CONSTRAINED).Count = 1, 
                       STRTOSET(@DirectionDirection, CONSTRAINED), 
                       [Direction].[Direction].currentmember 
                        ) 
                  )";

            using (AdomdConnection conn = new AdomdConnection("Data Source=miranda;Initial Catalog=bikesMD2"))
            {
                conn.Open();
                using (AdomdCommand cmd = new AdomdCommand(command, conn))
                {
                    cmd.Parameters.Add(new AdomdParameter("DirectionDirection", queryparameters.Direction.UniqueName));
                    cmd.Parameters.Add(new AdomdParameter("CentroidALevelLocality", queryparameters.Level3A.UniqueName));
                    cmd.Parameters.Add(new AdomdParameter("CentroidBLevelLocality", queryparameters.Level3B.UniqueName));
                    AdomdDataAdapter adapter = new AdomdDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                conn.Close();
            }
            // results in ds.Tables[0]
            // need to transfer to new list
            DataTable dt = ds.Tables[0];
            Dictionary<string,int> colnbr = new Dictionary<string,int >();
            //Dictionary(<string>,<int>) colnbr = new Dictionary(<string>,<int>);
            foreach (DataColumn dc in dt.Columns)
            {
                colnbr.Add(dc.ColumnName, dc.Ordinal);
            }
            foreach (DataRow dr in dt.Rows)
            {
                L2ABwHrWD oneresult = new L2ABwHrWD(dr, colnbr);

                results.Add(oneresult);
            }
        }
    }
}