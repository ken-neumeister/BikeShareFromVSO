using System;
using System.Collections.Generic;
using System.Data;
using Reporting.BikesBase;

namespace Reporting.Models.TripCategories
{
    public class RowSubscriber : AbstractBikeRow, IEquatable<RowSubscriber>
    {
        /// <summary>
        /// Data for subscriber
        /// </summary>
        public DimSubscriber Subscriber { get; set; }

        //public TableHour2 Hour2List;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="colnbr"></param>
        public RowSubscriber(BikeTable tabletype, DataRow dr, Dictionary<string, int> colnbr) : base(tabletype, dr, colnbr)
        {
            Subscriber = new DimSubscriber(dr, colnbr);

            //Hour2List = new TableHour2();
        }

        public bool Equals(RowSubscriber other)
        {
            return (Subscriber.MemberUniqueName == other.Subscriber.MemberUniqueName);
        }
    }

    public class TableSubscriber : BikeTable //: List<RowSubscriber>
    {
        //internal void UpdateForDR(DataRow dr, Dictionary<string, int> colnbr)
        //{
        //    RowSubscriber candidate = new RowSubscriber(dr, colnbr);
        //    RowSubscriber selected;
        //    if (Exists(x => x.Equals(candidate)))
        //    {
        //        selected = Find(x => x.Equals(candidate));
        //        selected.Bikes.IncValue(candidate.Bikes.GetValue());
        //    }
        //    else
        //    {
        //        Add(candidate);
        //        selected = candidate;
        //    }
        //    selected.Hour2List.UpdateForDR(dr, colnbr);
        //}

        public override bool CompareRows(AbstractBikeRow selected, AbstractBikeRow candidate)
        {
            RowSubscriber thisSelected = (RowSubscriber)selected;
            RowSubscriber thisCandidate = (RowSubscriber)candidate;
            bool result = thisSelected.Equals(thisCandidate);
            return result;
        }

        public override AbstractBikeRow GenerateNewRow(BikeTable tabletype, DataRow dr, Dictionary<string, int> colnbr)
        {
            return new RowSubscriber(tabletype, dr, colnbr);
        }
    }
}