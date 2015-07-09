using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Reporting.BikesBase
{
    public class MeasureBikes : VariableForBikesCube
    {
        public MeasureBikes(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Measures].[Bikes]") { }

        public float GetValue()
        {
            float bikes = 0;
            float.TryParse(MemberCaption, out bikes);
            return bikes;
        }

        internal void IncValue(float v)
        {
            string newcaption = (GetValue() + v).ToString();
            // MemberUniqueName.Replace(MemberCaption, newcaption);
            MemberCaption = newcaption;
        }

    }
}