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
using System.Windows.Controls;

namespace LibraryManagement
{
    public class LibraryManagement : Plugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private LibraryManagementSettings settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("d02f854e-900d-48df-b01c-6d13e985f479");

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

                if (cv.Check("GameActivity", pluginFolder))
                {
                    cv.ShowNotification(api, "GameActivity - " + resources.GetString("LOCUpdaterWindowTitle"));
                }
            }
        }


        // To add new game menu items override GetGameMenuItems
        public override List<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            List<GameMenuItem> gameMenuItems = new List<GameMenuItem>();
            return gameMenuItems;
        }

        // To add new main menu items override GetMainMenuItems
        public override List<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            List<MainMenuItem> mainMenuItems = new List<MainMenuItem>();
            return mainMenuItems;
        }


        public override void OnGameSelected(GameSelectionEventArgs args)
        {

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
            
        }


        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new LibraryManagementSettingsView();
        }
    }
}