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
        public override Guid Id { get; } = Guid.Parse("d02f854e-900d-48df-b01c-6d13e985f479");

        private bool IsFinished { get; set; } = true;
        private bool IsStarted { get; set; } = false;


        public LibraryManagement(IPlayniteAPI api) : base(api)
        {
            // Custom elements integration
            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "PluginFeaturesIconList" },
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
                return new PluginFeaturesIconList(PlayniteApi, PluginSettings);
            }

            return null;
        }
        #endregion


        #region Menus
        // To add new game menu items override GetGameMenuItems
        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var GameMenu = args.Games.First();

            List<GameMenuItem> gameMenuItems = new List<GameMenuItem>();

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = resources.GetString("LOCLm"),
                Description = resources.GetString("LOCLmMediaEditor"),
                Action = (gameMenuItem) =>
                {
                    LmImageEditor ViewExtension = new LmImageEditor(PlayniteApi, GameMenu);
                    Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, "ImageEditor", ViewExtension);
                    windowExtension.ShowDialog();
                }
            });

#if DEBUG
            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = resources.GetString("LOCLm"),
                Description = "-"
            });
            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = resources.GetString("LOCLm"),
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
                MenuSection = MenuInExtensions + resources.GetString("LOCLm"),
                Description = resources.GetString("LOCLmSetCompanies"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PlayniteApi, PluginSettings.Settings);
                    libraryManagementTools.SetCompanies();
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + resources.GetString("LOCLm"),
                Description = resources.GetString("LOCLmSetFeatures"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PlayniteApi, PluginSettings.Settings);
                    libraryManagementTools.SetFeatures();
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + resources.GetString("LOCLm"),
                Description = resources.GetString("LOCLmSetGenres"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PlayniteApi, PluginSettings.Settings);
                    libraryManagementTools.SetGenres();
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + resources.GetString("LOCLm"),
                Description = resources.GetString("LOCLmSetTags"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PlayniteApi, PluginSettings.Settings);
                    libraryManagementTools.SetTags();
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + resources.GetString("LOCLm"),
                Description = resources.GetString("LOCLmSetTagsToFeatures"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PlayniteApi, PluginSettings.Settings);
                    libraryManagementTools.SetTagsToFeatures();
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + resources.GetString("LOCLm"),
                Description = resources.GetString("LOCLmSetTagsToGenres"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PlayniteApi, PluginSettings.Settings);
                    libraryManagementTools.SetTagsToGenres();
                }
            });

#if DEBUG
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + resources.GetString("LOCLm"),
                Description = "-"
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + resources.GetString("LOCLm"),
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
            Task.Run(() =>
            {
                Thread.Sleep(5000);
                IsStarted = true;
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
            if (IsStarted)
            {
                AutoUpdate(true);
            }
        }


        private void AutoUpdate(bool OnlyToDay = false, Game gameUpdated = null)
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PlayniteApi, PluginSettings.Settings);

            if (PluginSettings.Settings.AutoUpdateCompanies)
            {
                try
                {
                    libraryManagementTools.SetCompanies(OnlyToDay, gameUpdated);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }

            if (PluginSettings.Settings.AutoUpdateGenres)
            {
                try
                {
                    libraryManagementTools.SetGenres(OnlyToDay, gameUpdated);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }

            if (PluginSettings.Settings.AutoUpdateFeatures)
            {
                try
                {
                    libraryManagementTools.SetFeatures(OnlyToDay, gameUpdated);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }

            if (PluginSettings.Settings.AutoUpdateTags)
            {
                try
                {
                    libraryManagementTools.SetTags(OnlyToDay, gameUpdated);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }

            if (PluginSettings.Settings.AutoUpdateTagsToFeatures)
            {
                try
                {
                    libraryManagementTools.SetTagsToFeatures(OnlyToDay, gameUpdated);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }

            if (PluginSettings.Settings.AutoUpdateTagsToGenres)
            {
                try
                {
                    libraryManagementTools.SetTagsToGenres(OnlyToDay, gameUpdated);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }
        }


        #region Settings
        public override ISettings GetSettings(bool firstRunSettings)
        {
            return PluginSettings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new LibraryManagementSettingsView(this, PlayniteApi, PluginSettings.Settings);
        }
        #endregion
    }
}
