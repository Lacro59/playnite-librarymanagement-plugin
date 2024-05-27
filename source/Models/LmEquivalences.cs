using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryManagement.Models
{
    public abstract class LmEquivalences
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public List<string> OldNames { get; set; }
        public string IconUnicode { get; set; }

        [DontSerialize]
        public string OldNamesLinear => string.Join(", ", OldNames.ToArray());

        [DontSerialize]
        public string NewName => (IconUnicode + " " + Name).Trim();
    }
}
