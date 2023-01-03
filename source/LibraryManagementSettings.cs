using CommonPluginsShared.Extensions;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace LibraryManagement
{
    public class LibraryManagementSettings : ObservableObject
    {
        #region Settings variables
        public bool MenuInExtensions { get; set; } = true;
        public DateTime LastAutoLibUpdateAssetsDownload { get; set; } = DateTime.Now;

        public bool NotifitcationAfterUpdate { get; set; } = true;

        public bool AutoUpdateCompanies { get; set; } = false;
        public bool AutoUpdateGenres { get; set; } = false;
        public bool AutoUpdateFeatures { get; set; } = false;
        public bool AutoUpdateTags { get; set; } = false;
        public bool AutoUpdateTagsToFeatures { get; set; } = false;
        public bool AutoUpdateTagsToGenres { get; set; } = false;


        public List<LmGenreEquivalences> ListGenreEquivalences { get; set; } = new List<LmGenreEquivalences>();
        public List<LmFeatureEquivalences> ListFeatureEquivalences { get; set; } = new List<LmFeatureEquivalences>();
        public List<LmTagsEquivalences> ListTagsEquivalences { get; set; } = new List<LmTagsEquivalences>();
        public List<LmCompaniesEquivalences> ListCompaniesEquivalences { get; set; } = new List<LmCompaniesEquivalences>();

        public List<string> ListGenreExclusion { get; set; } = new List<string>();
        public List<string> ListFeatureExclusion { get; set; } = new List<string>();
        public List<string> ListTagsExclusion { get; set; } = new List<string>();
        public List<string> ListCompaniesExclusion { get; set; } = new List<string>();


        public List<LmTagToFeature> ListTagsToFeatures { get; set; } = new List<LmTagToFeature>();
        public List<LmTagToGenre> ListTagsToGenres { get; set; } = new List<LmTagToGenre>();
        public List<LmTagToCategory> ListTagsToCategories { get; set; } = new List<LmTagToCategory>();


        private bool _EnableIntegrationFeatures { get; set; } = true;
        public bool EnableIntegrationFeatures
        {
            get => _EnableIntegrationFeatures;
            set
            {
                _EnableIntegrationFeatures = value;
                OnPropertyChanged();
            }
        }

        public bool OneForSameIcon { get; set; } = false;
        public bool UsedDark { get; set; } = false;
        public bool UsedGog { get; set; } = false;
        public List<ItemFeature> ItemFeatures { get; set; } = new List<ItemFeature>();


        private bool _EnableIntegrationAgeRatings { get; set; } = true;
        public bool EnableIntegrationAgeRatings
        {
            get => _EnableIntegrationAgeRatings;
            set
            {
                _EnableIntegrationAgeRatings = value;
                OnPropertyChanged();
            }
        }

        public List<AgeRating> AgeRatings { get; set; } = new List<AgeRating>();
        #endregion

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        #region Variables exposed
        private bool _HasData { get; set; } = false;
        [DontSerialize]
        public bool HasData
        {
            get => _HasData;
            set
            {
                _HasData = value;
                OnPropertyChanged();
            }
        }

        private int _DataCount { get; set; } = 0;
        [DontSerialize]
        public int DataCount
        {
            get => _DataCount;
            set
            {
                _DataCount = value;
                OnPropertyChanged();
            }
        }


        private List<ItemFeature> _DataList { get; set; } = new List<ItemFeature>();
        [DontSerialize]
        public List<ItemFeature> DataList
        {
            get => _DataList;
            set
            {
                _DataList = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }


    public class LibraryManagementSettingsViewModel : ObservableObject, ISettings
    {
        private readonly LibraryManagement Plugin;
        private LibraryManagementSettings EditingClone { get; set; }

        private LibraryManagementSettings _Settings;
        public LibraryManagementSettings Settings { get => _Settings; set => SetValue(ref _Settings, value); }


        public LibraryManagementSettingsViewModel(LibraryManagement plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            Plugin = plugin;

            // Load saved settings.
            LibraryManagementSettings savedSettings = plugin.LoadPluginSettings<LibraryManagementSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            Settings = savedSettings ?? new LibraryManagementSettings();


            if (Settings.ItemFeatures.Count == 0)
            {
                List<ItemFeature> ItemFeatures = new List<ItemFeature>() {
                    new ItemFeature { Name = "Achievements", NameAssociated = "Achievements", IconDefault = "ico_achievements.png" },
                    new ItemFeature { Name = "Captions Available", NameAssociated = "Captions Available", IconDefault = "ico_cc.png" },
                    new ItemFeature { Name = "Cloud Saves", NameAssociated = "Cloud Saves", IconDefault = "ico_cloud.png" },
                    new ItemFeature { Name = "Commentary Available", NameAssociated = "Commentary Available", IconDefault = "ico_commentary.png" },
                    new ItemFeature { Name = "Controller support", NameAssociated = "Controller support", IconDefault = "ico_controller.png" },
                    new ItemFeature { Name = "Co-Op", NameAssociated = "Co-Op", IconDefault = "ico_coop.png" },
                    new ItemFeature { Name = "Co-Operative", NameAssociated = "Co-Operative", IconDefault = "ico_coop.png" },
                    new ItemFeature { Name = "Cross-Platform Multiplayer", NameAssociated = "Cross-Platform Multiplayer", IconDefault = "ico_multiPlayer.png" },
                    new ItemFeature { Name = "DLC", NameAssociated = "DLC", IconDefault = "ico_dlc.png" },
                    new ItemFeature { Name = "Full Controller Support", NameAssociated = "Full Controller Support", IconDefault = "ico_controller.png" },
                    new ItemFeature { Name = "In-App Purchases", NameAssociated = "In-App Purchases", IconDefault = "ico_cart.png" },
                    new ItemFeature { Name = "Includes Level Editor", NameAssociated = "Includes Level Editor", IconDefault = "ico_editor.png" },
                    new ItemFeature { Name = "Includes Source SDK", NameAssociated = "Includes Source SDK", IconDefault = "ico_sdk.png" },
                    new ItemFeature { Name = "LAN Co-Op", NameAssociated = "LAN Co-Op", IconDefault = "ico_coop.png" },
                    new ItemFeature { Name = "LAN Pvp", NameAssociated = "LAN Pvp", IconDefault = "ico_multiPlayer.png" },
                    new ItemFeature { Name = "Leaderboards", NameAssociated = "Leaderboards", IconDefault = "ico_leaderboards.png" },
                    new ItemFeature { Name = "Massively Multiplayer Online (MMO)", NameAssociated = "Massively Multiplayer Online (MMO)", IconDefault = "ico_multiPlayer.png" },
                    new ItemFeature { Name = "MMO", NameAssociated = "MMO", IconDefault = "ico_multiPlayer.png" },
                    new ItemFeature { Name = "Mods", NameAssociated = "Mods", IconDefault = "ico_sdk.png" },
                    new ItemFeature { Name = "Multiplayer", NameAssociated = "Multiplayer", IconDefault = "ico_multiPlayer.png" },
                    new ItemFeature { Name = "Nexus Mods", NameAssociated = "Nexus Mods", IconDefault = "ico_nexus.png" },
                    new ItemFeature { Name = "Online Co-Op", NameAssociated = "Online Co-Op", IconDefault = "ico_coop.png" },
                    new ItemFeature { Name = "Online Pvp", NameAssociated = "Online Pvp", IconDefault = "ico_multiPlayer.png" },
                    new ItemFeature { Name = "Overlay", NameAssociated = "Overlay", IconDefault = "ico_overlay.png" },
                    new ItemFeature { Name = "Partial Controller Support", NameAssociated = "Partial Controller Support", IconDefault = "ico_partial_controller.png" },
                    new ItemFeature { Name = "Pvp", NameAssociated = "Pvp", IconDefault = "ico_multiPlayer.png" },
                    new ItemFeature { Name = "Remote Play On Phone", NameAssociated = "Remote Play On Phone", IconDefault = "ico_remote_play.png" },
                    new ItemFeature { Name = "Remote Play On Tablet", NameAssociated = "Remote Play On Tablet", IconDefault = "ico_remote_play.png" },
                    new ItemFeature { Name = "Remote Play On TV", NameAssociated = "Remote Play On TV", IconDefault = "ico_remote_play.png" },
                    new ItemFeature { Name = "Remote Play Together", NameAssociated = "Remote Play Together", IconDefault = "ico_remote_play_together.png" },
                    new ItemFeature { Name = "Shared/Split Screen", NameAssociated = "Shared/Split Screen", IconDefault = "ico_multiPlayer.png" },
                    new ItemFeature { Name = "Shared/Split Screen Co-Op", NameAssociated = "Shared/Split Screen Co-Op", IconDefault = "ico_multiPlayer.png" },
                    new ItemFeature { Name = "Shared/Split Screen Pvp", NameAssociated = "Shared/Split Screen Pvp", IconDefault = "ico_multiPlayer.png" },
                    new ItemFeature { Name = "Single Player", NameAssociated = "Single Player", IconDefault = "ico_singlePlayer.png" },
                    new ItemFeature { Name = "Split Screen", NameAssociated = "Split Screen", IconDefault = "ico_multiPlayer.png" },
                    new ItemFeature { Name = "Stats", NameAssociated = "Stats", IconDefault = "ico_stats.png" },
                    new ItemFeature { Name = "Trading Cards", NameAssociated = "Trading Cards", IconDefault = "ico_cards.png" },
                    new ItemFeature { Name = "Valve Anti-Cheat Enabled", NameAssociated = "Valve Anti-Cheat Enabled", IconDefault = "ico_vac.png" },
                    new ItemFeature { Name = "VR", NameAssociated = "VR", IconDefault = "ico_vr.png" },
                    new ItemFeature { Name = "VR Gamepad", NameAssociated = "VR Gamepad", IconDefault = "ico_vr_input_motion.png" },
                    new ItemFeature { Name = "VR Keyboard/Mouse", NameAssociated = "VR Keyboard/Mouse", IconDefault = "ico_vr_input_kbm.png" },
                    new ItemFeature { Name = "VR Motion Controllers", NameAssociated = "VR Motion Controllers", IconDefault = "ico_vr_input_motion.png" },
                    new ItemFeature { Name = "VR Room-Scale", NameAssociated = "VR Room-Scale", IconDefault = "ico_vr_area_roomscale.png" },
                    new ItemFeature { Name = "VR Seated", NameAssociated = "VR Seated", IconDefault = "ico_vr_area_seated.png" },
                    new ItemFeature { Name = "VR Standing", NameAssociated = "VR Standing", IconDefault = "ico_vr_area_standing.png" },
                    new ItemFeature { Name = "Workshop", NameAssociated = "Workshop", IconDefault = "ico_workshop.png" }
                };

                Settings.ItemFeatures = ItemFeatures;
            }

            if (Settings.ItemFeatures.Find(x => x.Name.IsEqual("vr")) == null)
            {
                Settings.ItemFeatures.Add(new ItemFeature { Name = "VR", NameAssociated = "VR", IconDefault = "ico_vr.png" });
            }

            if (Settings.ItemFeatures.Find(x => x.Name.IsEqual("dlc")) == null)
            {
                Settings.ItemFeatures.Add(new ItemFeature { Name = "DLC", NameAssociated = "DLC", IconDefault = "ico_dlc.png" });
            }

            if (Settings.AgeRatings.Count == 0)
            {
                Task.Run(() =>
                {
                    System.Threading.SpinWait.SpinUntil(() => API.Instance.Database.IsOpen, -1);

                    List<string> Age_3 = new List<string> { "ACB G", "CERO A", "ClassInd G", "ESRB EC", "GRAC ALL", "PEGI 3", "USK 0" };
                    List<string> Age_6 = new List<string> { "ESRB E", "USK 6" };
                    List<string> Age_7 = new List<string> { "PEGI 7" };
                    List<string> Age_8 = new List<string> { "ACB PG" };
                    List<string> Age_10 = new List<string> { "ClassInd 10", "ESRB E10" };
                    List<string> Age_12 = new List<string> { "ACB M", "CERO B", "ClassInd 12", "GRAC 12", "PEGI 12", "USK 12" };
                    List<string> Age_13 = new List<string> { "ESRB T" };
                    List<string> Age_14 = new List<string> { "ClassInd 14" };
                    List<string> Age_15 = new List<string> { "ACB MA", "CERO C", "GRAC 15" };
                    List<string> Age_16 = new List<string> { "ClassInd 16", "PEGI 16", "USK 16" };
                    List<string> Age_17 = new List<string> { "CERO D", "ESRB M" };
                    List<string> Age_18 = new List<string> { "ACB R18", "ACB X18", "CERO Z", "ClassInd 18", "ESRB AO", "GRAC 18", "PEGI 18", "USK 18" };

                    Application.Current.Dispatcher?.Invoke(() =>
                        Settings.AgeRatings = new List<AgeRating>
                        {
                            new AgeRating { Age = 3, Color = new SolidColorBrush(Colors.White), AgeRatingIds = API.Instance.Database.AgeRatings?.Where(x => Age_3.Any(y => y.IsEqual(x.Name)))?.Select(x => x.Id)?.ToList() ?? new List<Guid>() },
                            new AgeRating { Age = 6, Color = new SolidColorBrush(Colors.Gold), AgeRatingIds = API.Instance.Database.AgeRatings?.Where(x => Age_6.Any(y => y.IsEqual(x.Name)))?.Select(x => x.Id)?.ToList() ?? new List<Guid>() },
                            new AgeRating { Age = 7, Color = new SolidColorBrush(Colors.Gold), AgeRatingIds = API.Instance.Database.AgeRatings?.Where(x => Age_7.Any(y => y.IsEqual(x.Name)))?.Select(x => x.Id)?.ToList() ?? new List<Guid>() },
                            new AgeRating { Age = 8, Color = new SolidColorBrush(Colors.Gold), AgeRatingIds = API.Instance.Database.AgeRatings?.Where(x => Age_8.Any(y => y.IsEqual(x.Name)))?.Select(x => x.Id)?.ToList() ?? new List<Guid>() },
                            new AgeRating { Age = 10, Color = new SolidColorBrush(Colors.SeaGreen), AgeRatingIds = API.Instance.Database.AgeRatings?.Where(x => Age_10.Any(y => y.IsEqual(x.Name)))?.Select(x => x.Id)?.ToList() ?? new List<Guid>() },
                            new AgeRating { Age = 12, Color = new SolidColorBrush(Colors.SeaGreen), AgeRatingIds = API.Instance.Database.AgeRatings?.Where(x => Age_12.Any(y => y.IsEqual(x.Name)))?.Select(x => x.Id)?.ToList() ?? new List<Guid>() },
                            new AgeRating { Age = 13, Color = new SolidColorBrush(Colors.SeaGreen), AgeRatingIds = API.Instance.Database.AgeRatings?.Where(x => Age_13.Any(y => y.IsEqual(x.Name)))?.Select(x => x.Id)?.ToList() ?? new List<Guid>() },
                            new AgeRating { Age = 14, Color = new SolidColorBrush(Colors.SeaGreen), AgeRatingIds = API.Instance.Database.AgeRatings?.Where(x => Age_14.Any(y => y.IsEqual(x.Name)))?.Select(x => x.Id)?.ToList() ?? new List<Guid>() },
                            new AgeRating { Age = 15, Color = new SolidColorBrush(Colors.DodgerBlue), AgeRatingIds = API.Instance.Database.AgeRatings?.Where(x => Age_15.Any(y => y.IsEqual(x.Name)))?.Select(x => x.Id)?.ToList() ?? new List<Guid>() },
                            new AgeRating { Age = 16, Color = new SolidColorBrush(Colors.DodgerBlue), AgeRatingIds = API.Instance.Database.AgeRatings?.Where(x => Age_16.Any(y => y.IsEqual(x.Name)))?.Select(x => x.Id)?.ToList() ?? new List<Guid>() },
                            new AgeRating { Age = 17, Color = new SolidColorBrush(Colors.DodgerBlue), AgeRatingIds = API.Instance.Database.AgeRatings?.Where(x => Age_17.Any(y => y.IsEqual(x.Name)))?.Select(x => x.Id)?.ToList() ?? new List<Guid>() },
                            new AgeRating { Age = 18, Color = new SolidColorBrush(Colors.Firebrick), AgeRatingIds = API.Instance.Database.AgeRatings?.Where(x => Age_18.Any(y => y.IsEqual(x.Name)))?.Select(x => x.Id)?.ToList() ?? new List<Guid>() }
                        }
                    );
                });
            }


            Settings.ItemFeatures = Settings.ItemFeatures.OrderBy(x => x.Name).ToList();
        }

        // Code executed when settings view is opened and user starts editing values.
        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
        }

        // Code executed when user decides to cancel any changes made since BeginEdit was called.
        // This method should revert any changes made to Option1 and Option2.
        public void CancelEdit()
        {
            Settings = EditingClone;
        }

        // Code executed when user decides to confirm changes made since BeginEdit was called.
        // This method should save settings made to Option1 and Option2.
        public void EndEdit()
        {
            Plugin.SavePluginSettings(Settings);


            // Rename
            foreach (LmGenreEquivalences lmGenreEquivalences in Settings.ListGenreEquivalences)
            {
                if (lmGenreEquivalences.Id != null)
                {
                    LibraryManagementTools.RenameGenre(Plugin.PlayniteApi, (Guid)lmGenreEquivalences.Id, lmGenreEquivalences.NewName);
                }
            }
            foreach (LmFeatureEquivalences lmFeatureEquivalences in Settings.ListFeatureEquivalences)
            {
                if (lmFeatureEquivalences.Id != null)
                {
                    LibraryManagementTools.RenameFeature(Plugin.PlayniteApi, (Guid)lmFeatureEquivalences.Id, lmFeatureEquivalences.NewName);
                }
            }
            foreach (LmCompaniesEquivalences lmCompaniesEquivalences in Settings.ListCompaniesEquivalences)
            {
                if (lmCompaniesEquivalences.Id != null)
                {
                    LibraryManagementTools.RenameCompanies(Plugin.PlayniteApi, (Guid)lmCompaniesEquivalences.Id, lmCompaniesEquivalences.NewName);
                }
            }
            foreach (LmTagsEquivalences lmTagsEquivalences in Settings.ListTagsEquivalences)
            {
                if (lmTagsEquivalences.Id != null)
                {
                    LibraryManagementTools.RenameTags(Plugin.PlayniteApi, (Guid)lmTagsEquivalences.Id, lmTagsEquivalences.NewName);
                }
            }

            this.OnPropertyChanged();
        }

        // Code execute when user decides to confirm changes made since BeginEdit was called.
        // Executed before EndEdit is called and EndEdit is not called if false is returned.
        // List of errors is presented to user if verification fails.
        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}
