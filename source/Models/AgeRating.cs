using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LibraryManagement.Models
{
    public class AgeRating
    {
        public int Age { get; set; }
        public List<Guid> AgeRatingIds { get; set; }
        public SolidColorBrush Color { get; set; }
        [DontSerialize]
        public string AgeRatingString => API.Instance.Database.AgeRatings?.Where(x => AgeRatingIds?.Any(y => y == x.Id) ?? false)?.Select(x => x.Name).Aggregate((a, b) => a + ", " + b) ?? string.Empty;
    }
}
