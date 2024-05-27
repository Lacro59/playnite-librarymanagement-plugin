using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LibraryManagement.Models
{
    public class ItemFeature
    {
        public string Name { get; set; } = string.Empty;
        public string IconDefault { get; set; } = string.Empty;

        public Playnite.SDK.Models.GameFeature Feature { get; set; }

        [DontSerialize]
        public string IconDefaultFullPath 
        {
            get
            {
                string PluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string FullPath;
                if (IsGog)
                {
                    FullPath = IsDark
                        ? Path.Combine(PluginPath, "Resources\\gog\\dark", IconDefault)
                        : Path.Combine(PluginPath, "Resources\\gog\\white", IconDefault);
                }
                else
                {
                    FullPath = IsDark
                        ? Path.Combine(PluginPath, "Resources\\steam\\dark", IconDefault)
                        : Path.Combine(PluginPath, "Resources\\steam\\white", IconDefault);
                }

                return File.Exists(FullPath) ? FullPath : string.Empty;
            }
        }

        public string NameAssociated { get; set; } = string.Empty;
        public string IconCustom { get; set; } = string.Empty;
        [DontSerialize]
        public BitmapImage IconCustomBitmapImage => !IconCustom.IsNullOrEmpty() && File.Exists(IconCustom)
                    ? BitmapExtensions.BitmapFromFile(IconCustom, new BitmapLoadProperties(100, 0))
                    : null;

        public bool IsDark { get; set; }
        public bool IsGog { get; set; }

        [DontSerialize]
        public string IconString => !IconCustom.IsNullOrEmpty() && File.Exists(IconCustom) ? IconCustom : IconDefaultFullPath;

        [DontSerialize]
        public BitmapImage IconBitmapImage => !IconCustom.IsNullOrEmpty() && File.Exists(IconCustom)
                    ? IconCustomBitmapImage
                    : BitmapExtensions.BitmapFromFile(IconDefaultFullPath, new BitmapLoadProperties(100, 0));

        public bool IsAdd { get; set; }
    }
}
