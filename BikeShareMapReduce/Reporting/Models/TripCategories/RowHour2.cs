using System;
using System.Collections.Generic;
using Reporting.BikesBase;
using System.Data;

namespace Reporting.Models.TripCategories
{
    public class RowHour2 : AbstractBikeRow, IEquatable<RowHour2>
    {
        /// <summary>
        /// Time periods of day in 2-hour intervals
        /// Goal to populate column chart to visualize diurnal pattern
        /// </summary>
        /// <remarks>
        /// Reasoning is traffic periods in 4-hour intervals (such as commute periods)
        /// Cut in half to allow flexibility in defining start of 4-hour block
        /// </remarks>
        public DimTimeHour2ofDay Hour2OfDay;

        /// <summary>
        /// Constructor for hour record
        /// </summary>
        /// <param name="dr">
        /// Dataset row
        /// </param>
        /// <param name="colnbr">
        /// Dictionary of column headings in dataset
        /// </param>
        public RowHour2(DataRow dr, Dictionary<string, int> colnbr) : base(dr, colnbr)
        {
            Hour2OfDay = new DimTimeHour2ofDay(dr, colnbr);
        }

        public bool Equals(RowHour2 other)
        {
            return (Hour2OfDay.MemberUniqueName == other.Hour2OfDay.MemberUniqueName);            
        }
    }

    public class TableHour2: List<RowHour2> //: BikeTable
    {
        internal void UpdateForDR(DataRow dr, Dictionary<string, int> colnbr)
        {
            RowHour2 candidate = new RowHour2(dr, colnbr);
            RowHour2 selected;
            if (Exists(x => x.Equals(candidate)))
            {
                selected = Find(x => x.Equals(candidate));
                selected.Bikes.IncValue(candidate.Bikes.GetValue());
            }
            else
            {
                Add(candidate);
                selected = candidate;
            }
        }
        
        //public override bool CompareRows(AbstractBikeRow selected, AbstractBikeRow candidate)
        //{
        //    RowHour2 thisSelected = (RowHour2)selected;
        //    RowHour2 thisCandidate = (RowHour2)candidate;
        //    bool result = thisSelected.Equals(thisCandidate);
        //    return result;
        //}

        //public override AbstractBikeRow GenerateNewRow(DataRow dr, Dictionary<string, int> colnbr)
        //{
        //    return new RowHour2(dr, colnbr);
        //}
    }
}