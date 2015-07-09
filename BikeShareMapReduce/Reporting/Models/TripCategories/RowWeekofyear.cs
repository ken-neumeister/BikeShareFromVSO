using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Reporting.BikesBase;
using System.Data;

namespace Reporting.Models.TripCategories
{
    public class RowWeekofyear : AbstractBikeRow, IEquatable<RowWeekofyear>
    {
        public DimTimeWeekofyear Weekofyear;

        public RowWeekofyear(BikeTable tabletype, DataRow dr, Dictionary<string, int> colnbr) : base(tabletype, dr, colnbr)
        {
            Weekofyear = new DimTimeWeekofyear(dr, colnbr);
        }

        public bool Equals(RowWeekofyear other)
        {
            return (Weekofyear.MemberUniqueName == other.Weekofyear.MemberUniqueName);
        }

    }

    public class TableWeekofyear : BikeTable
    {
        public override bool CompareRows(AbstractBikeRow selected, AbstractBikeRow candidate)
        {
            RowWeekofyear thisselected = (RowWeekofyear)selected;
            RowWeekofyear thiscandidate = (RowWeekofyear)candidate;
            return (thisselected.Equals(thiscandidate));
        }

        public override AbstractBikeRow GenerateNewRow(BikeTable typename, DataRow dr, Dictionary<string, int> colnbr)
        {
            return new RowWeekofyear(typename, dr, colnbr);
        }
    }
}