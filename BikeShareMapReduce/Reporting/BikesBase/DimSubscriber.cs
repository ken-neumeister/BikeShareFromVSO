using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Reporting.BikesBase
{
    public class DimSubscriber : VariableForBikesCube
    {

        public DimSubscriber(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Subscribers].[Subscriber Info].[Subscriber Info]") { }

        public string GetValue()
        {
            return MemberCaption;
        }
    }
}