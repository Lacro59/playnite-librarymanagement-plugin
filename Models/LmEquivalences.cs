using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.Models
{
    public abstract class LmEquivalences
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public List<string> OldNames { get; set; }
        public string IconUnicode { get; set; }

        [JsonIgnore]
        public string OldNamesLinear
        {
            get
            {
                return String.Join(", ", OldNames.ToArray());
            }
        }

        [JsonIgnore]
        public string NewName
        {
            get
            {
                return (IconUnicode + " " + Name).Trim();
            }
        }
    }
}
