using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Reporting.BikesBase
{
    public class DimTimeWeekdayName : VariableForBikesCube
    {
        public DimTimeWeekdayName(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Time Table].[Nameofday].[Nameofday]") { }

        public string GetValue()
        {
            return MemberCaption;
        }

    }
}