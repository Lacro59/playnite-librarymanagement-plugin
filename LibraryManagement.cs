﻿using CommonPluginsShared;
using CommonPluginsShared.PlayniteExtended;
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace LibraryManagement
{
    public class LibraryManagement : PluginExtended<LibraryManagementSettingsViewModel>
    {
        public override Guid Id { get; } = Guid.Parse("d02f854e-900d-48df-b01c-6d13e985f479");


        public LibraryManagement(IPlayniteAPI api) : base(api)
        {

        }


        #region Menus
        // To add new game menu items override GetGameMenuItems
        public override List<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
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
                Description = "Test",
                Action = (gameMenuItem) =>
                {

                }
            });
#endif

            return gameMenuItems;
        }

        // To add new main menu items override GetMainMenuItems
        public override List<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
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

#if DEBUG
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
        public override void OnGameSelected(GameSelectionEventArgs args)
        {
            try
            {
                if (args.NewValue != null && args.NewValue.Count == 1)
                {
                    Game GameSelected = args.NewValue[0];
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }

        // Add code to be executed when game is finished installing.
        public override void OnGameInstalled(Game game)
        {
            
        }

        // Add code to be executed when game is started running.
        public override void OnGameStarted(Game game)
        {
            
        }

        // Add code to be executed when game is preparing to be started.
        public override void OnGameStarting(Game game)
        {
            
        }

        // Add code to be executed when game is preparing to be started.
        public override void OnGameStopped(Game game, long elapsedSeconds)
        {

        }

        // Add code to be executed when game is uninstalled.
        public override void OnGameUninstalled(Game game)
        {

        }
        #endregion


        #region Application event
        // Add code to be executed when Playnite is initialized.
        public override void OnApplicationStarted()
        {
            
        }

        // Add code to be executed when Playnite is shutting down.
        public override void OnApplicationStopped()
        {
            
        }
        #endregion


        // Add code to be executed when library is updated.
        public override void OnLibraryUpdated()
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PlayniteApi, PluginSettings.Settings);

            if (PluginSettings.Settings.AutoUpdateGenres)
            {
                libraryManagementTools.SetGenres(true);
            }

            if (PluginSettings.Settings.AutoUpdateGenres)
            {
                libraryManagementTools.SetFeatures(true);
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