using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Reporting.BikesBase
{
    public class DimTimeWeekofyear : VariableForBikesCube
    {
        public DimTimeWeekofyear(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Time Table].[Weekofyear].[Weekofyear]") { }

        public int GetValue()
        {
            int result= 0;
            int.TryParse(MemberCaption, out result);
            return result;
        }
    }

}