using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Reporting.Models
{
    /// <summary>
    /// Base class for variable to capture OLAP results
    /// </summary>
    /// <remarks>
    /// Override for each new use, adding variable to cast the MemberCaption to appropriate type
    /// </remarks>
    public abstract class OlapVariable
    {
        /// <summary>
        /// Holds contents of [MEMBER_UNIQUE_NAME] column
        /// </summary>
        public string MemberUniqueName { get; set; }

        /// <summary>
        /// Holds contents of [MEMBER_CAPTION] column
        /// </summary>
        public string MemberCaption { get; set; }

        /// <summary>
        /// Constructor from reading results from table
        /// </summary>
        /// <param name="dr">
        /// current data row from dataset from query
        /// </param>
        /// <param name="colnbr">
        /// dictionary of column names prepaped by reading data table
        /// </param>
        /// <param name="DimensionName">
        /// prefix of column name
        /// </param>
        public OlapVariable(DataRow dr, Dictionary<string, int> colnbr, string DimensionName)
        {
            int col = 0;
            if (DimensionName.StartsWith("[Measures]"))
            {
                if (colnbr.TryGetValue(DimensionName, out col))
                {
                    MemberCaption = dr[col].ToString();
                    MemberUniqueName = "";
                }
            }
            else
            {
                if (colnbr.TryGetValue(DimensionName + ".[MEMBER_CAPTION]", out col))
                {
                    MemberCaption = dr[col].ToString();
                }
                if (colnbr.TryGetValue(DimensionName + ".[MEMBER_UNIQUE_NAME]", out col))
                {
                    MemberUniqueName = (string)dr[col];
                }
            }

        }
    }


    public class OlapQueryParameter
    {

        public OlapQueryParameter(string uniquename)
        {
            this.UniqueName = uniquename;
        }

        public string UniqueName { set; get; }
        public string ShortName()
        {
            string[] nameparts = UniqueName.Split(new char[] { '.' });
            string friendly = nameparts[nameparts.Count() - 1].TrimStart(new char[] { '[', '&' }).TrimEnd(new char[] { ']' });
            return (friendly);
        }

    }
}