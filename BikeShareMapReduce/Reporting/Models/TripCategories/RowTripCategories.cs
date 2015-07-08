using System;
using System.Collections.Generic;
using System.Data;
using Reporting.BikesBase;

namespace Reporting.Models.TripCategories
{
    public class RowTripCategories : AbstractBikeRow, IEquatable<RowTripCategories>
    {
        /// <summary>
        /// Constructor for row
        /// </summary>
        /// <param name="dr">
        /// rowset for record
        /// </param>
        /// <param name="colnbr">
        /// dictionary mapping column names to numbers
        /// </param>
        public RowTripCategories(BikeTable tabletype, DataRow dr, Dictionary<string, int> colnbr) : base(tabletype, dr,colnbr)
        {
            DistanceCategory = new DimPairDistanceCategory(dr, colnbr);
            DurationCategory = new DimTripCategory(dr, colnbr);
            //SubscriberList = new TableSubscriber();
        }

        /// <summary>
        /// Distance category in broad terms
        /// </summary>
        public DimPairDistanceCategory DistanceCategory;

        /// <summary>
        /// Duration category depending on whether trip involved a stop
        /// </summary>
        public DimTripCategory DurationCategory;

        //public TableSubscriber SubscriberList;

        public bool Equals(RowTripCategories other)
        {
            return ((DurationCategory.MemberUniqueName == other.DurationCategory.MemberUniqueName) &
                    (DistanceCategory.MemberUniqueName == other.DistanceCategory.MemberUniqueName));
        }
    }

    public class TableTripCategories: BikeTable //: List<RowTripCategories>
    {
        //internal void UpdateForDR(DataRow dr, Dictionary<string, int> colnbr)
        //{
        //    RowTripCategories candidate = new RowTripCategories(dr, colnbr);
        //    RowTripCategories selected;
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
        //    selected.SubscriberList.UpdateForDR(dr, colnbr);
        //}


        public override bool CompareRows(AbstractBikeRow selected, AbstractBikeRow candidate)
        {
            RowTripCategories thisSelected = (RowTripCategories)selected;
            RowTripCategories thisCandidate = (RowTripCategories)candidate;
            bool result = thisSelected.Equals(thisCandidate);
            return result;
        }

        public override AbstractBikeRow GenerateNewRow(BikeTable tabletype, DataRow dr, Dictionary<string, int> colnbr)
        {
            return new RowTripCategories(tabletype, dr, colnbr);
        }
    }
}