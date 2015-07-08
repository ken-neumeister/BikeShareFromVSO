using System;
using System.Collections.Generic;
using System.Data;

namespace Reporting.BikesBase
{
    /// <summary>
    /// All rows will have a bike measure
    /// </summary>
    /// <remarks>
    /// IEquatable must be inherited on final derived class
    /// derived classes will also have their own type-specific parent rows and childlists
    /// </remarks>
    public class AbstractBikeRow : IEquatable<AbstractBikeRow>
    {
        public MeasureBikes Bikes { get; set; }

        public BikeTable ChildList { get; set; }

        public AbstractBikeRow(DataRow dr, Dictionary<string, int> colnbr)
        {
            Bikes = new MeasureBikes(dr, colnbr);
        }

        public virtual bool Equals(AbstractBikeRow other)
        {
            throw new NotImplementedException();
        }
    }
}