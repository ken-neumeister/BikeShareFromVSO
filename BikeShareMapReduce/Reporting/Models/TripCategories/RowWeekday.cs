using System;
using System.Collections.Generic;
using Reporting.BikesBase;
using System.Data;

namespace Reporting.Models.TripCategories
{
    public class RowWeekday : AbstractBikeRow, IEquatable<RowWeekday>
    {
        /// <summary>
        /// WeekdayName
        /// </summary>
        /// <remarks>
        /// Will rely on query to use DayOfWeek number to order weekdays appropriately
        /// </remarks>
        public DimTimeWeekdayName WeekdayName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dr">
        /// row containing results of query
        /// </param>
        /// <param name="colnbr">
        /// Dictionary of column numbers for column headings
        /// </param>
        public RowWeekday(BikeTable tabletype, DataRow dr, Dictionary<string, int> colnbr) : base(tabletype, dr, colnbr)
        {
            WeekdayName = new DimTimeWeekdayName(dr, colnbr);
        }

        public bool Equals(RowWeekday other)
        {
            return (WeekdayName.MemberUniqueName == other.WeekdayName.MemberUniqueName);
        }
    }

    public class TableWeekday : BikeTable
    {
        public override bool CompareRows(AbstractBikeRow selected, AbstractBikeRow candidate)
        {
            RowWeekday thisSelected = (RowWeekday)selected;
            RowWeekday thisCandidate = (RowWeekday)candidate;
            bool result = thisSelected.Equals(thisCandidate);
            return result;
        }

        public override AbstractBikeRow GenerateNewRow(BikeTable tabletype, DataRow dr, Dictionary<string, int> colnbr)
        {
            return new RowWeekday(tabletype, dr, colnbr);
        }
    }
}