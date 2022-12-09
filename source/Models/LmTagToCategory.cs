using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.Models
{
    public class LmTagToCategory
    {
        public Guid TagId { get; set; }
        public string TagName { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
