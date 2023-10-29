using CommonPlayniteShared;
using CommonPlayniteShared.Common;
using CommonPluginsShared.Extensions;
using LibraryManagement.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LibraryManagement.Views
{
    /// <summary>
    /// Logique d'interaction pour AddEditNewFeatureIcon.xaml
    /// </summary>
    public partial class AddEditNewFeatureIcon : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private LibraryManagement plugin;
        public ItemFeature itemFeature;

        private bool UsedGog { get; set; }
        private bool UsedDark { get; set; }


        public AddEditNewFeatureIcon(LibraryManagement plugin, bool UsedGog, bool UsedDark, ItemFeature itemFeature = null)
        {
            this.plugin = plugin;
            this.UsedGog = UsedGog;
            this.UsedDark = UsedDark;

            InitializeComponent();

            List<ListItem> listItems = new List<ListItem>();
            foreach (var item in API.Instance.Database.Features)
            {
                if (!item.Name.IsNullOrEmpty())
                {
                    listItems.Add(new ListItem
                    {
                        Name = item.Name
                    });
                }
            }
            PART_OldNames.ItemsSource = listItems.ToObservable();


            List<ItemFeature> ItemFeatures = new List<ItemFeature>() {
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Achievements", NameAssociated = "Achievements", IconDefault = "ico_achievements.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Captions Available", NameAssociated = "Captions Available", IconDefault = "ico_cc.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Cloud Saves", NameAssociated = "Cloud Saves", IconDefault = "ico_cloud.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Commentary Available", NameAssociated = "Commentary Available", IconDefault = "ico_commentary.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Controller support", NameAssociated = "Controller support", IconDefault = "ico_controller.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Co-Op", NameAssociated = "Co-Op", IconDefault = "ico_coop.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Co-Operative", NameAssociated = "Co-Operative", IconDefault = "ico_coop.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Cross-Platform Multiplayer", NameAssociated = "Cross-Platform Multiplayer", IconDefault = "ico_multiPlayer.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "DLC", NameAssociated = "DLC", IconDefault = "ico_dlc.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Full Controller Support", NameAssociated = "Full Controller Support", IconDefault = "ico_controller.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "In-App Purchases", NameAssociated = "In-App Purchases", IconDefault = "ico_cart.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Includes Level Editor", NameAssociated = "Includes Level Editor", IconDefault = "ico_editor.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Includes Source SDK", NameAssociated = "Includes Source SDK", IconDefault = "ico_sdk.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "LAN Co-Op", NameAssociated = "LAN Co-Op", IconDefault = "ico_coop.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "LAN Pvp", NameAssociated = "LAN Pvp", IconDefault = "ico_multiPlayer.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Leaderboards", NameAssociated = "Leaderboards", IconDefault = "ico_leaderboards.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Massively Multiplayer Online (MMO)", NameAssociated = "Massively Multiplayer Online (MMO)", IconDefault = "ico_multiPlayer.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "MMO", NameAssociated = "MMO", IconDefault = "ico_multiPlayer.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Mods", NameAssociated = "Mods", IconDefault = "ico_sdk.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Multiplayer", NameAssociated = "Multiplayer", IconDefault = "ico_multiPlayer.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Nexus Mods", NameAssociated = "Nexus Mods", IconDefault = "ico_nexus.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Online Co-Op", NameAssociated = "Online Co-Op", IconDefault = "ico_coop.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Online Pvp", NameAssociated = "Online Pvp", IconDefault = "ico_multiPlayer.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Overlay", NameAssociated = "Overlay", IconDefault = "ico_overlay.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Partial Controller Support", NameAssociated = "Partial Controller Support", IconDefault = "ico_partial_controller.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Pvp", NameAssociated = "Pvp", IconDefault = "ico_multiPlayer.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Remote Play On Phone", NameAssociated = "Remote Play On Phone", IconDefault = "ico_remote_play.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Remote Play On Tablet", NameAssociated = "Remote Play On Tablet", IconDefault = "ico_remote_play.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Remote Play On TV", NameAssociated = "Remote Play On TV", IconDefault = "ico_remote_play.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Remote Play Together", NameAssociated = "Remote Play Together", IconDefault = "ico_remote_play_together.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Shared/Split Screen", NameAssociated = "Shared/Split Screen", IconDefault = "ico_multiPlayer.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Shared/Split Screen Co-Op", NameAssociated = "Shared/Split Screen Co-Op", IconDefault = "ico_multiPlayer.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Shared/Split Screen Pvp", NameAssociated = "Shared/Split Screen Pvp", IconDefault = "ico_multiPlayer.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Single Player", NameAssociated = "Single Player", IconDefault = "ico_singlePlayer.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Split Screen", NameAssociated = "Split Screen", IconDefault = "ico_multiPlayer.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Stats", NameAssociated = "Stats", IconDefault = "ico_stats.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Trading Cards", NameAssociated = "Trading Cards", IconDefault = "ico_cards.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Valve Anti-Cheat Enabled", NameAssociated = "Valve Anti-Cheat Enabled", IconDefault = "ico_vac.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "VR", NameAssociated = "VR", IconDefault = "ico_vr.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "VR Gamepad", NameAssociated = "VR Gamepad", IconDefault = "ico_vr_input_motion.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "VR Keyboard/Mouse", NameAssociated = "VR Keyboard/Mouse", IconDefault = "ico_vr_input_kbm.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "VR Motion Controllers", NameAssociated = "VR Motion Controllers", IconDefault = "ico_vr_input_motion.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "VR Room-Scale", NameAssociated = "VR Room-Scale", IconDefault = "ico_vr_area_roomscale.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "VR Seated", NameAssociated = "VR Seated", IconDefault = "ico_vr_area_seated.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "VR Standing", NameAssociated = "VR Standing", IconDefault = "ico_vr_area_standing.png" },
                new ItemFeature { IsGog = this.UsedGog, IsDark = this.UsedDark, Name = "Workshop", NameAssociated = "Workshop", IconDefault = "ico_workshop.png" }
            };
            PART_CbIcon.ItemsSource = ItemFeatures;


            if (itemFeature != null)
            {
                PART_Name.Text = itemFeature.Name;
                PART_NameAssociated.Text = itemFeature.NameAssociated;

                if (File.Exists(itemFeature.IconCustom))
                {
                    PART_IconCustom.Tag = itemFeature.IconCustom;
                    PART_IconCustom.Source = itemFeature.IconCustomBitmapImage;
                }

                if (!itemFeature.IconDefault.IsNullOrEmpty())
                {
                    PART_CbIcon.SelectedIndex = ItemFeatures.FindIndex(x => x.IconDefault.IsEqual(itemFeature.IconDefault));
                }
            }

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(PART_OldNames.ItemsSource);
            view.Filter = UserFilter;
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
            PART_RemoveCustomIcon.IsEnabled = true;
        }

        private void PART_RemoveCustomIcon_Click(object sender, RoutedEventArgs e)
        {
            PART_IconCustom.Tag = null;
            PART_IconCustom.Source = null;
            PART_RemoveCustomIcon.IsEnabled = false;
        }


        private void PART_Save_Click(object sender, RoutedEventArgs e)
        {
            itemFeature = new ItemFeature
            {
                Name = PART_Name.Text,
                NameAssociated = PART_NameAssociated.Text,
                IconCustom = PART_IconCustom.Tag?.ToString() ?? string.Empty,
                IconDefault = ((ItemFeature)PART_CbIcon.SelectedItem)?.IconDefault ?? string.Empty,
                IsGog = UsedGog,
                IsDark = UsedDark,
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
            PART_Save.IsEnabled = PART_Name.Text.Length > 2 && PART_NameAssociated.Text.Length > 2 && !(PART_IconCustom.Tag?.ToString()?.IsNullOrEmpty() ?? false);
        }


        private void PART_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(PART_OldNames.ItemsSource).Refresh();
        }
        private bool UserFilter(object item)
        {
            return (item as ListItem).Name.RemoveDiacritics().Contains(PART_Search.Text.RemoveDiacritics());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PART_NameAssociated.Text = (string)((Button)sender).Tag;
        }
    }
}
