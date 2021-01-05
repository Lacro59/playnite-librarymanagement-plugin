using LibraryManagement.Models;
using LibraryManagement.Services;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement
{
    public class LibraryManagementSettings : ISettings
    {
        private readonly LibraryManagement plugin;
        private LibraryManagementSettings editingClone;

        public bool EnableCheckVersion { get; set; } = true;
        public bool MenuInExtensions { get; set; } = true;

        public bool AutoUpdateGenres { get; set; } = false;
        public bool AutoUpdateFeatures { get; set; } = false;

        public List<LmGenreEquivalences> ListGenreEquivalences { get; set; } = new List<LmGenreEquivalences>();
        public List<LmFeatureEquivalences> ListFeatureEquivalences { get; set; } = new List<LmFeatureEquivalences>();


        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonIgnore` ignore attribute.
        [JsonIgnore]
        public bool OptionThatWontBeSaved { get; set; } = false;

        // Parameterless constructor must exist if you want to use LoadPluginSettings method.
        public LibraryManagementSettings()
        {
        }

        public LibraryManagementSettings(LibraryManagement plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<LibraryManagementSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                EnableCheckVersion = savedSettings.EnableCheckVersion;
                MenuInExtensions = savedSettings.MenuInExtensions;

                AutoUpdateGenres = savedSettings.AutoUpdateGenres;
                AutoUpdateFeatures = savedSettings.AutoUpdateFeatures;

                ListGenreEquivalences = savedSettings.ListGenreEquivalences;
                ListFeatureEquivalences = savedSettings.ListFeatureEquivalences;
            }
        }

        // Code executed when settings view is opened and user starts editing values.
        public void BeginEdit()
        {
            editingClone = this.GetClone();
        }

        // Code executed when user decides to cancel any changes made since BeginEdit was called.
        // This method should revert any changes made to Option1 and Option2.
        public void CancelEdit()
        {
            LoadValues(editingClone);
        }

        private void LoadValues(LibraryManagementSettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        // Code executed when user decides to confirm changes made since BeginEdit was called.
        // This method should save settings made to Option1 and Option2.
        public void EndEdit()
        {
            plugin.SavePluginSettings(this);

            // Rename
            foreach (LmGenreEquivalences lmGenreEquivalences in ListGenreEquivalences)
            {
                if (lmGenreEquivalences.Id != null)
                {
                    LibraryManagementTools.RenameGenre(plugin.PlayniteApi, (Guid)lmGenreEquivalences.Id, lmGenreEquivalences.NewName);
                }
            }
            foreach (LmFeatureEquivalences lmFeatureEquivalences in ListFeatureEquivalences)
            {
                if (lmFeatureEquivalences.Id != null)
                {
                    LibraryManagementTools.RenameGenre(plugin.PlayniteApi, (Guid)lmFeatureEquivalences.Id, lmFeatureEquivalences.NewName);
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