using System;
using System.Collections.Generic;

namespace efsample.Models
{
    public partial class Regions
    {
        public Regions()
        {
            Countries = new HashSet<Countries>();
        }

        public int RegionId { get; set; }
        public string RegionName { get; set; }

        public virtual ICollection<Countries> Countries { get; set; }
    }
}
