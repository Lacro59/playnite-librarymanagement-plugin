using CommonPlayniteShared.Common;
using LibraryManagement.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LibraryManagement.Views
{
    /// <summary>
    /// Logique d'interaction pour AddNewFeatureIcon.xaml
    /// </summary>
    public partial class AddNewFeatureIcon : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private LibraryManagement plugin;
        public ItemFeature itemFeature;


        public AddNewFeatureIcon(LibraryManagement plugin)
        {
            this.plugin = plugin;

            InitializeComponent();
        }


        private void PART_AddCustomIcon_Click(object sender, RoutedEventArgs e)
        {
            var result = API.Instance.Dialogs.SelectIconFile();
            if (!result.IsNullOrEmpty())
            {
                string PathDest = Path.Combine(plugin.GetPluginUserDataPath(), Path.GetFileName(result));
                FileSystem.CopyFile(result, PathDest);

                PART_IconCustom.Tag = PathDest;
                PART_IconCustom.Source = BitmapExtensions.BitmapFromFile(PathDest);
            }

            PART_TextChanged(null, null);
        }


        private void PART_Save_Click(object sender, RoutedEventArgs e)
        {
            itemFeature = new ItemFeature
            {
                Name = PART_Name.Text,
                NameAssociated = PART_NameAssociated.Text,
                IconCustom = PART_IconCustom.Tag.ToString(),
                IsAdd = true
            };

            ((Window)this.Parent).Close();
        }

        private void PART_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }


        private void PART_TextChanged(object sender, TextChangedEventArgs e)
        {
            PART_Save.IsEnabled = PART_Name.Text.Length > 2 && PART_NameAssociated.Text.Length > 2 && !PART_IconCustom.Tag.ToString().IsNullOrEmpty();
        }
    }
}
