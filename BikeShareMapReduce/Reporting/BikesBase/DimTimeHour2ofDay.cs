using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Reporting.BikesBase
{
    public class DimTimeHour2ofDay : VariableForBikesCube
    {
        public DimTimeHour2ofDay(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Time Table].[Hour2ofday].[Hour2ofday]") { }

        public int GetValue()
        {
            return Convert.ToInt32(MemberCaption);
        }
    }
}