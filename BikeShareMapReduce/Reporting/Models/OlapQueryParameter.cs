using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reporting.Models
{
    public class OlapQueryParameter
    {

        public OlapQueryParameter(string uniquename)
        {
            this.UniqueName = uniquename;
        }

        public string UniqueName { set; get; }
        public string ShortName()
        {
            string[] nameparts = UniqueName.Split(new char[] { '.' });
            string friendly = nameparts[nameparts.Count() - 1].TrimStart(new char[] { '[', '&' }).TrimEnd(new char[] { ']' });
            return (friendly);
        }
        
    }
}