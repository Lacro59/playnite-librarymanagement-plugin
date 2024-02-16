using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.Models
{
    public class LmTagToFeature
    {
        public Guid TagId { get; set; }
        public string TagName { get; set; }

        public Guid FeatureId { get; set; }
        public string FeatureName { get; set; }

        public bool KeepTag { get; set; }
    }
}
