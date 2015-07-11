using System.Collections.Generic;
using System.Data;

namespace Reporting.BikesBase
{
    public class DimPairQtrMile : VariableForBikesCube
    {
        public DimPairQtrMile(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Station Pair Distance].[Qtr Miles].[Qtr Miles]") { }

        public float GetValue()
        {
            float qtrmi = 0;
            float.TryParse(MemberCaption, out qtrmi);
            return qtrmi;
        }

    }
}