using System;
using System.Collections.Generic;
using System.Data;

namespace Reporting.BikesBase
{
    public class DimPairQtrMile : VariableForBikesCube
    {
        public DimPairQtrMile(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Station Pair Distance].[Qtr Miles].[Qtr Miles]") { }

        public float GetValue()
        {
            return Convert.ToSingle(MemberCaption);
        }

    }
}