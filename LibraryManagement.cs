﻿using CommonPluginsShared;
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
    public class LibraryManagement : Plugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private LibraryManagementSettings settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("d02f854e-900d-48df-b01c-6d13e985f479");

        private Game GameSelected;
        private LmImageEditor ViewExtension;


        public LibraryManagement(IPlayniteAPI api) : base(api)
        {
            settings = new LibraryManagementSettings(this);

            // Get plugin's location 
            string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Add plugin localization in application ressource.
            PluginLocalization.SetPluginLanguage(pluginFolder, api.ApplicationSettings.Language);
            // Add common in application ressource.
            Common.Load(pluginFolder);
            Common.SetEvent(PlayniteApi);

            // Check version
            if (settings.EnableCheckVersion)
            {
                CheckVersion cv = new CheckVersion();
                cv.Check("LibraryManagement", pluginFolder, api);
            }
        }


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
                    ViewExtension = new LmImageEditor(PlayniteApi, GameMenu);
                    Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, "ImageEditor", ViewExtension);
                    windowExtension.ShowDialog();
                }
            });

            return gameMenuItems;
        }

        // To add new main menu items override GetMainMenuItems
        public override List<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            string MenuInExtensions = string.Empty;
            if (settings.MenuInExtensions)
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
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PlayniteApi, settings);
                    libraryManagementTools.SetFeatures();
                }
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + resources.GetString("LOCLm"),
                Description = resources.GetString("LOCLmSetGenres"),
                Action = (mainMenuItem) =>
                {
                    LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PlayniteApi, settings);
                    libraryManagementTools.SetGenres();
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


        public override void OnGameSelected(GameSelectionEventArgs args)
        {
            try
            {
                if (args.NewValue != null && args.NewValue.Count == 1)
                {
                    GameSelected = args.NewValue[0];
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "LibraryManagement");
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


        // Add code to be executed when Playnite is initialized.
        public override void OnApplicationStarted()
        {
            
        }

        // Add code to be executed when Playnite is shutting down.
        public override void OnApplicationStopped()
        {
            
        }


        // Add code to be executed when library is updated.
        public override void OnLibraryUpdated()
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(this, PlayniteApi, settings);

            if (settings.AutoUpdateGenres)
            {
                libraryManagementTools.SetGenres(true);
            }

            if (settings.AutoUpdateGenres)
            {
                libraryManagementTools.SetFeatures(true);
            }
        }


        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new LibraryManagementSettingsView(this, PlayniteApi, settings);
        }
    }
}