using CommonPluginsShared;
using CommonPluginsShared.PlayniteExtended;
using LibraryManagement.Controls;
using LibraryManagement.Models;
using LibraryManagement.Services;
using LibraryManagement.Views;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LibraryManagement
{
    public class LibraryManagement : PluginExtended<LibraryManagementSettingsViewModel>
    {
        public override Guid Id => Guid.Parse("d02f854e-900d-48df-b01c-6d13e985f479");

        private bool IsFinished { get; set; } = true;
        private bool PreventLibraryUpdatedOnStart { get; set; } = true;


        public LibraryManagement(IPlayniteAPI api) : base(api)
        {
            // Custom elements integration
            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "PluginFeaturesIconList", "PluginAgeRating" },
                SourceName = "LibraryManagement"
            });

            // Settings integration
            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "LibraryManagement",
                SettingsRoot = $"{nameof(PluginSettings)}.{nameof(PluginSettings.Settings)}"
            });
        }


        #region Custom events
        private void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> e)
        {
            if (IsFinished)
            {
                Task.Run(() =>
                {
                    IsFinished = false;
                    AutoUpdate(false, e.UpdatedItems[0].NewData);
                })
                .ContinueWith(antecedent => 
                {
                    IsFinished = true;
                });
            }
        }
        #endregion


        #region Theme integration
        // List custom controls
        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "PluginFeaturesIconList")
            {
                return new PluginFeaturesIconList(PluginSettings);
            }

            if (args.Name == "PluginAgeRating")
            {
                return new PluginAgeRating(PluginSettings);
            }

            return null;
        }
        #endregion


        #region Menus
        // To add new game menu items override GetGameMenuItems
        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            Game GameMenu = args.Games.First();
            List<GameMenuItem> gameMenuItems = new List<GameMenuItem>();

            gameMenuItems.Add(new GameMenuItem
            {
                Icon = Path.Combine(PluginFolder, "Resources", "media_editor.png"),
                Description = ResourceProvider.GetString("LOCLmMediaEditor"),
                Action = (gameMenuItem) =>
                {
                    LmImageEditor ViewExtension = new LmImageEditor(GameMenu);
                    Window windowExtension = PlayniteUiHelper.CreateExtensionWindow("ImageEditor", ViewExtension);
                    windowExtension.ShowDialog();
                }
            });

#if DEBUG
            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = ResourceProvider.GetString("LOCLm"),
                Description = "-"
            });
            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = ResourceProvider.GetString("LOCLm"),
                Description = "Test",
                Action = (gameMenuItem) =>
                {

                }
            });
#endif

            return gameMenuItems;
        }

        // To add new main menu items override GetMainMenuItems
        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            string MenuInExtensions = string.Empty;
            if (PluginSettings.Settings.MenuInExtensions)
            {
                MenuInExtensions = "@";
            }

            List<MainMenuItem> mainMenuItems = new List<MainMenuItem>();
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCLm"),
                Description = ResourceProvider.GetString("LOCLmApplyAll"),
                Action = (mainMenuItem) =>
                {
                    AutoUpdate(false, null, true);
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCLm"),
                Description = "-",
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCLm"),
                Description = ResourceProvider.GetString("LOCLmSetCompanies"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PluginSettings.Settings);
                    libraryManagementTools.SetCompanies();
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCLm"),
                Description = ResourceProvider.GetString("LOCLmSetFeatures"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PluginSettings.Settings);
                    libraryManagementTools.SetFeatures();
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCLm"),
                Description = ResourceProvider.GetString("LOCLmSetGenres"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PluginSettings.Settings);
                    libraryManagementTools.SetGenres();
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCLm"),
                Description = ResourceProvider.GetString("LOCLmSetTags"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PluginSettings.Settings);
                    libraryManagementTools.SetTags();
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCLm"),
                Description = ResourceProvider.GetString("LOCLmSetTagsToFeatures"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PluginSettings.Settings);
                    libraryManagementTools.SetTagsToFeatures();
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCLm"),
                Description = ResourceProvider.GetString("LOCLmSetTagsToGenres"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PluginSettings.Settings);
                    libraryManagementTools.SetTagsToGenres();
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCLm"),
                Description = ResourceProvider.GetString("LOCLmSetTagsToCategories"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PluginSettings.Settings);
                    libraryManagementTools.SetTagsToCategories();
                }
            });

#if DEBUG
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCLm"),
                Description = "-"
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCLm"),
                Description = "Test",
                Action = (mainMenuItem) =>
                {

                }
            });
#endif

            return mainMenuItems;
        }
        #endregion


        #region Game event
        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            try
            {
                if (args.NewValue?.Count == 1)
                {
                    Game GameSelected = args.NewValue[0];

                    List<ItemFeature> itemFeatures = IcoFeatures.GetAvailableItemFeatures(PluginSettings, GameSelected);

                    PluginSettings.Settings.HasData = itemFeatures.Count > 0;
                    PluginSettings.Settings.DataCount = itemFeatures.Count;
                    PluginSettings.Settings.DataList = itemFeatures;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, "LibraryManagement");
            }
        }

        // Add code to be executed when game is finished installing.
        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {

        }

        // Add code to be executed when game is uninstalled.
        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {

        }

        // Add code to be executed when game is preparing to be started.
        public override void OnGameStarting(OnGameStartingEventArgs args)
        {

        }

        // Add code to be executed when game is started running.
        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            
        }

        // Add code to be executed when game is preparing to be started.
        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {

        }
        #endregion


        #region Application event
        // Add code to be executed when Playnite is initialized.
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            _ = Task.Run(() =>
            {
                Thread.Sleep(10000);
                PreventLibraryUpdatedOnStart = false;
            });
        }

        // Add code to be executed when Playnite is shutting down.
        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {

        }
        #endregion


        // Add code to be executed when library is updated.
        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (!PreventLibraryUpdatedOnStart)
            {
                AutoUpdate(true);
            }
        }


        private void AutoUpdate(bool onlyToDay = false, Game gameUpdated = null, bool force = false)
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PluginSettings.Settings);

            if (PluginSettings.Settings.AutoUpdateCompanies || force)
            {
                try
                {
                    libraryManagementTools.SetCompanies(onlyToDay, gameUpdated);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }

            if (PluginSettings.Settings.AutoUpdateGenres || force)
            {
                try
                {
                    libraryManagementTools.SetGenres(onlyToDay, gameUpdated);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }

            if (PluginSettings.Settings.AutoUpdateFeatures || force)
            {
                try
                {
                    libraryManagementTools.SetFeatures(onlyToDay, gameUpdated);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }

            if (PluginSettings.Settings.AutoUpdateTags || force)
            {
                try
                {
                    libraryManagementTools.SetTags(onlyToDay, gameUpdated);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }

            if (PluginSettings.Settings.AutoUpdateTagsToFeatures || force)
            {
                try
                {
                    libraryManagementTools.SetTagsToFeatures(onlyToDay);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }

            if (PluginSettings.Settings.AutoUpdateTagsToGenres || force)
            {
                try
                {
                    libraryManagementTools.SetTagsToGenres(onlyToDay);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }

            PluginSettings.Settings.LastAutoLibUpdateAssetsDownload = DateTime.Now;
            SavePluginSettings(PluginSettings.Settings);
        }


        #region Settings
        public override ISettings GetSettings(bool firstRunSettings)
        {
            return PluginSettings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new LibraryManagementSettingsView(this, PluginSettings.Settings);
        }
        #endregion
    }
}
