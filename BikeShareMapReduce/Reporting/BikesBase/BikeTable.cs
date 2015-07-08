using System;
using System.Collections.Generic;
using System.Data;

namespace Reporting.BikesBase
{
    public abstract class BikeTable : List<AbstractBikeRow>
    {
        /// <summary>
        /// Each specific table will override this method to return appropriate row type
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="colnbr"></param>
        /// <returns></returns>
        public virtual AbstractBikeRow GenerateNewRow(DataRow dr, Dictionary<string, int> colnbr)
        {
           throw new NotImplementedException();
        }

        public virtual bool CompareRows(AbstractBikeRow selected, AbstractBikeRow candidate)
        {
            throw new NotImplementedException();
        }

        internal void UpdateForDR(List<BikeTable> hierarchy, DataRow dr, Dictionary<string, int> colnbr)
        {
            BikeTable thislist = hierarchy[0];
            AbstractBikeRow candidate = thislist.GenerateNewRow(dr, colnbr);
            AbstractBikeRow selected;
            if (Contains(candidate))
            {
                // how does this work?
                // AbstractBikeRow.Equals will throw exception
                //  -- this is what happens, call to AbstractBikeRow instead of derived class
                selected = Find(x => x.Equals(candidate));
                selected.Bikes.IncValue(candidate.Bikes.GetValue());
            }
            else
            {
                Add(candidate);
                selected = candidate;
            }
            if (hierarchy.Count > 0)
            {
                selected.ChildList = hierarchy[0];
                hierarchy.RemoveAt(0);
                selected.ChildList.UpdateForDR(hierarchy, dr, colnbr);
            }
        }
    }
}