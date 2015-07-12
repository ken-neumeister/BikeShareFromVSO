using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

namespace Reporting.Models.LinqCategories
{
    //not needed
    //public class LinqResultTable
    //{
    //    public string DistanceCategory { get; set; }
    //    public string DistanceCategoryCaption { get; set; }
    //    public string DurationCategory { get; set; }
    //    public string DurationCategoryCaption { get; set; }
    //    public string Subscriber { get; set; }
    //    public string SubscriberCaption { get; set; }
    //    public string Hour2 { get; set; }
    //    public string Hour2Caption { get; set; }
    //    public string Bikes { get; set; }
    //    public float BikesValue ()
    //    {
    //        float retval = 0.0f;
    //        float.TryParse(Bikes, out retval);
    //        return retval;
    //    }
    //}

    public static class impFunctions
    {
        // Not needed
        //public static float ToFloat(this string olapresult)
        //{
        //    float retval = 0;
        //    float.TryParse(olapresult, out retval);
        //    return retval;
        //}

        public static ExpandoObject ToExpando(this object anonymousObject)
        {
            IDictionary<string, object> anonymousDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(anonymousObject);
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var item in anonymousDictionary)
            {
                expando.Add(item);  // does not recurse into lists
                // something like the following would be necessary, but item.Value is not correct handle to the underlying anonymous type

                //if (item.Key == "subscribers")
                //{

                //    IDictionary<string, object> subDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(item.Value);
                //    IDictionary<string, object> subExpando = new ExpandoObject();
                //    foreach (var sub in subDictionary)
                //    {
                //        if (sub.Key == "Hour2")
                //        {
                //            IDictionary<string, object> hrDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(item.Value);
                //            IDictionary<string, object> hrExpando = new ExpandoObject();
                //            foreach (var hr in hrDictionary)
                //            {
                //                hrExpando.Add(hr);
                //            }
                //            subExpando.Add(sub.Key, hrExpando);
                //        } 
                //        else
                //        {
                //            subExpando.Add(sub);
                //        }
                //    }
                //    expando.Add(item.Key, subExpando);
                //} 
                //else
                //{
                //    expando.Add(item);  // does not recurse into lists
                //}
            }
            return (ExpandoObject)expando;
        }
    } // See more at: http://www.dotnetfunda.com/articles/show/2655/binding-views-with-anonymous-type-collection-in-aspnet-mvc#sthash.tQPxvw88.dpuf

    public class ReportUsingLinq
    {
        public string Title { get; set; }
        public IEnumerable<dynamic> model { get; set; }

        public float GetValue(string olapvalue)
        {
            float retvalue = 0;
            float.TryParse(olapvalue, out retvalue);
            return retvalue;
        }


        public IEnumerator GetEnumerator()
        {
            return model.GetEnumerator();
        }

        public ReportUsingLinq(string title)
        {

            string CommandText = @"SELECT 
NON EMPTY { [Measures].[Bikes] } ON COLUMNS, 
NON EMPTY { ([Station Pair Distance].[Mile Categories].[Mile Categories].ALLMEMBERS * 
             [TripCat].[Trip Category].[Trip Category].ALLMEMBERS * 
             [Subscribers].[Subscriber Info].[Subscriber Info].ALLMEMBERS * 
             [Time Table].[Hour2ofday].[Hour2ofday].ALLMEMBERS ) 
          } DIMENSION PROPERTIES MEMBER_CAPTION, MEMBER_UNIQUE_NAME ON ROWS 
FROM ( SELECT ( { [Direction].[Direction].&[A-B] } ) ON COLUMNS 
       FROM [Bikeshare]) 
       WHERE ( [Direction].[Direction].&[A-B] )";

            DataSet ds = new DataSet();
            using (AdomdConnection conn = new AdomdConnection("Data Source=miranda;Initial Catalog=bikesMD2"))
            {
                conn.Open();
                using (AdomdCommand cmd = new AdomdCommand(CommandText, conn))
                {
                    AdomdDataAdapter adapter = new AdomdDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                conn.Close();
            }
            // Linq using nested groups to build hierarchy, does this work?
            // if it works, can intellisense in Razor figure it out?
            var HourBikes = (from row in ds.Tables[0].AsEnumerable()
                             select new //LinqResultTable()
                             {
                                 DistanceCategory = row.Field<string>("[Station Pair Distance].[Mile Categories].[Mile Categories].[MEMBER_UNIQUE_NAME]"),
                                 DurationCategory = row.Field<string>("[TripCat].[Trip Category].[Trip Category].[MEMBER_UNIQUE_NAME]"),
                                 Subscriber = row.Field<string>("[Subscribers].[Subscriber Info].[Subscriber Info].[MEMBER_UNIQUE_NAME]"),
                                 Hour2 = row.Field<string>("[Time Table].[Hour2ofday].[Hour2ofday].[MEMBER_UNIQUE_NAME]"),
                                 DistanceCategoryCaption = row.Field<string>("[Station Pair Distance].[Mile Categories].[Mile Categories].[MEMBER_CAPTION]"),
                                 DurationCategoryCaption = row.Field<string>("[TripCat].[Trip Category].[Trip Category].[MEMBER_CAPTION]"),
                                 SubscriberCaption = row.Field<string>("[Subscribers].[Subscriber Info].[Subscriber Info].[MEMBER_CAPTION]"),
                                 Hour2Caption = row.Field<string>("[Time Table].[Hour2ofday].[Hour2ofday].[MEMBER_UNIQUE_NAME]"),
                                 Bikes = (row.Field<double>("[Measures].[Bikes]")).ToString()
                             }); // .ToArray(); //forces immediate execution, not needed.

            model = (from row in HourBikes
                     group row by new
                     {
                         DistanceCategory = row.DistanceCategory,
                         DurationCategory = row.DurationCategory,
                         DistanceCategoryCaption = row.DistanceCategoryCaption,
                         DurationCategoryCaption = row.DurationCategoryCaption
                     } into Cat
                     select new
                     {
                         DistanceCategory = Cat.Key.DistanceCategory,
                         DurationCategory = Cat.Key.DurationCategory,
                         DistanceCategoryCaption = Cat.Key.DistanceCategoryCaption, 
                         DurationCategoryCaption = Cat.Key.DurationCategoryCaption,
                         // BikeValueList = (Cat.Select(row => row.Bikes)), // generates list of string values that can be summed later
                         TotalBikes = (Cat.Sum(row => Convert.ToDouble(row.Bikes))).ToString(),
                         subscribers = from srow in HourBikes
                                       where Cat.Key.DistanceCategory == srow.DistanceCategory 
                                          && Cat.Key.DurationCategory == srow.DurationCategory 
                                       group srow by new
                                       {
                                           Subsciber = srow.Subscriber, 
                                           SubscriberCaption = srow.SubscriberCaption
                                       } into Scat
                                       select new
                                       {
                                           Subscriber = Scat.Key.Subsciber,
                                           SubscriberCaption = Scat.Key.SubscriberCaption, 
                                           // BikeValueList = (Scat.Select(row => row.Bikes)),
                                           TotalBikes = (Scat.Sum(row => Convert.ToDouble(row.Bikes))).ToString("#,#.##"),
                                           Hour2 = from hrow in HourBikes
                                                   where Cat.Key.DistanceCategory == hrow.DistanceCategory 
                                                      && Cat.Key.DurationCategory == hrow.DurationCategory 
                                                      && Scat.Key.Subsciber == hrow.Subscriber 
                                                   group hrow by new
                                                   {
                                                       Hour2 = hrow.Hour2,
                                                       Hour2Caption = hrow.Hour2Caption
                                                   } into Hcat
                                                   select new
                                                   {
                                                       Hour2 = Hcat.Key.Hour2,
                                                       Hour2Caption = Hcat.Key.Hour2Caption,
                                                       // BikeValueList = (Hcat.Select(row => row.Bikes)), 
                                                       TotalBikes = (Hcat.Sum(row => Convert.ToDouble(row.Bikes))).ToString()
                                                   }
                                       }
                     }).AsEnumerable().Select(c=>c.ToExpando());
            Title = title;
            // return model;
        }
    }
}