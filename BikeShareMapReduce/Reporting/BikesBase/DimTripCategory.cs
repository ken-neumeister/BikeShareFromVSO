using System.Collections.Generic;
using System.Data;

namespace Reporting.BikesBase
{
    public class DimTripCategory : VariableForBikesCube
    {
        public DimTripCategory(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[TripCat].[Trip Category].[Trip Category]") { }

        public string GetValue()
        {
            return MemberCaption;
        }
    }
}