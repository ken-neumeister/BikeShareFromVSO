﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Reporting.BikesBase
{
    public class DimPairDistanceCategory : VariableForBikesCube
    {
        public DimPairDistanceCategory(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr, "[Station Pair Distance].[Mile Categories].[Mile Categories]") { }

        public string GetValue()
        {
            return MemberCaption;
        }
    }
}