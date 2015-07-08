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
            return Convert.ToSingle(MemberCaption);
        }

        internal void IncValue(float v)
        {
            string newcaption = (GetValue() + v).ToString();
            MemberUniqueName.Replace(MemberCaption, newcaption);
            MemberCaption = newcaption;
        }

    }
}