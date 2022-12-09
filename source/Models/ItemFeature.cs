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
        private static readonly ILogger logger = LogManager.GetLogger();


        public string Name { get; set; } = string.Empty;
        public string IconDefault { get; set; } = string.Empty;

        public Playnite.SDK.Models.GameFeature Feature { get; set; }

        [DontSerialize]
        public string IconDefaultFullPath 
        {
            get
            {
                string PluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string FullPath = string.Empty;

                if (IsGog)
                {
                    if (IsDark)
                    {
                        FullPath = Path.Combine(PluginPath, "Resources\\gog\\dark", IconDefault);
                    }
                    else
                    {
                        FullPath = Path.Combine(PluginPath, "Resources\\gog\\white", IconDefault);
                    }
                }
                else
                {
                    if (IsDark)
                    {
                        FullPath = Path.Combine(PluginPath, "Resources\\steam\\dark", IconDefault);
                    }
                    else
                    {
                        FullPath = Path.Combine(PluginPath, "Resources\\steam\\white", IconDefault);
                    }
                }

                if (File.Exists(FullPath))
                {
                    return FullPath;
                }

                return string.Empty;
            }
        }

        public string NameAssociated { get; set; } = string.Empty;
        public string IconCustom { get; set; } = string.Empty;
        [DontSerialize]
        public BitmapImage IconCustomBitmapImage
        {
            get
            {
                if (!IconCustom.IsNullOrEmpty() && File.Exists(IconCustom))
                {
                    return BitmapExtensions.BitmapFromFile(IconCustom, new BitmapLoadProperties(100, 0));
                }

                return null;
            }
        }

        public bool IsDark { get; set; }
        public bool IsGog { get; set; }

        [DontSerialize]
        public string IconString
        {
            get
            {
                // Icon custom
                if (!IconCustom.IsNullOrEmpty() && File.Exists(IconCustom))
                {
                    return IconCustom;
                }

                // Default icon
                return IconDefaultFullPath;
            }
        }
        [DontSerialize]
        public BitmapImage IconBitmapImage
        {
            get
            {
                // Icon custom
                if (!IconCustom.IsNullOrEmpty() && File.Exists(IconCustom))
                {
                    return IconCustomBitmapImage;
                }

                // Default icon
                return BitmapExtensions.BitmapFromFile(IconDefaultFullPath, new BitmapLoadProperties(100, 0));
            }
        }

        public bool IsAdd { get; set; }
    }
}
