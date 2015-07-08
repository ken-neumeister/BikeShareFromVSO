using System;
using System.Collections.Generic;
using System.Data;

namespace Reporting.BikesBase
{
    public class BikeTable : List<AbstractBikeRow>
    {
        /// <summary>
        /// Each specific table will override this method to return appropriate row type
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="colnbr"></param>
        /// <returns></returns>
        public virtual AbstractBikeRow GenerateNewRow(BikeTable typename, DataRow dr, Dictionary<string, int> colnbr)
        {
            throw new NotImplementedException();
        }

        public virtual bool CompareRows(AbstractBikeRow selected, AbstractBikeRow candidate)
        {
            throw new NotImplementedException();
        }

        internal void UpdateForDR(List<BikeTable> hierarchy, int hierarchylevel, DataRow dr, Dictionary<string, int> colnbr)
        {
            BikeTable childlisttype;
            if (hierarchy.Count > hierarchylevel)
            {
                childlisttype = hierarchy[hierarchylevel];
            }
            else
            {
                childlisttype = null;
            }
            AbstractBikeRow candidate = GenerateNewRow(childlisttype, dr, colnbr);
            AbstractBikeRow selected;
            if (Exists(x => CompareRows(x, candidate)))
            {
                // how does this work?
                // AbstractBikeRow.Equals will throw exception
                //  -- this is what happens, call to AbstractBikeRow instead of derived class
                selected = Find(x => CompareRows(x, candidate));
                selected.Bikes.IncValue(candidate.Bikes.GetValue());
            }
            else
            {
                Add(candidate);
                selected = candidate;
            }
            hierarchylevel += 1;
            //if (hierarchy.Count > hierarchylevel)
            //{
            if (selected.ChildList != null)
            {
                selected.ChildList.UpdateForDR(hierarchy, hierarchylevel, dr, colnbr);
            }
            //}
        }
    }
}