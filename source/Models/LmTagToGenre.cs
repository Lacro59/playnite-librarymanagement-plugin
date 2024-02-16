using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.Models
{
    public class LmTagToGenre
    {
        public Guid TagId { get; set; }
        public string TagName { get; set; }

        public Guid GenreId { get; set; }
        public string GenreName { get; set; }

        public bool KeepTag { get; set; }
    }
}
