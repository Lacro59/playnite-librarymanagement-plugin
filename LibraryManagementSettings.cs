using LibraryManagement.Models;
using LibraryManagement.Services;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement
{
    public class LibraryManagementSettings : ObservableObject
    {
        #region Settings variables
        public bool EnableCheckVersion { get; set; } = true;
        public bool MenuInExtensions { get; set; } = true;

        public bool AutoUpdateGenres { get; set; } = false;
        public bool AutoUpdateFeatures { get; set; } = false;
        public bool AutoUpdateTags { get; set; } = false;

        public List<LmGenreEquivalences> ListGenreEquivalences { get; set; } = new List<LmGenreEquivalences>();
        public List<LmFeatureEquivalences> ListFeatureEquivalences { get; set; } = new List<LmFeatureEquivalences>();
        public List<LmTagsEquivalences> ListTagsEquivalences { get; set; } = new List<LmTagsEquivalences>();

        public List<string> ListGenreExclusion { get; set; } = new List<string>();
        public List<string> ListFeatureExclusion { get; set; } = new List<string>();
        public List<string> ListTagsExclusion { get; set; } = new List<string>();
        #endregion

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        #region Variables exposed

        #endregion  
    }


    public class LibraryManagementSettingsViewModel : ObservableObject, ISettings
    {
        private readonly LibraryManagement Plugin;
        private LibraryManagementSettings EditingClone { get; set; }

        private LibraryManagementSettings _Settings;
        public LibraryManagementSettings Settings
        {
            get => _Settings;
            set
            {
                _Settings = value;
                OnPropertyChanged();
            }
        }


        public LibraryManagementSettingsViewModel(LibraryManagement plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            Plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<LibraryManagementSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new LibraryManagementSettings();
            }
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
                    LibraryManagementTools.RenameGenre(Plugin.PlayniteApi, (Guid)lmFeatureEquivalences.Id, lmFeatureEquivalences.NewName);
                }
            }
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
