using CommonPluginsShared.Controls;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LibraryManagement.Controls
{
    /// <summary>
    /// Logique d'interaction pour FmFeaturesIconList.xaml
    /// </summary>
    public partial class LmFeaturesIconList : PluginUserControlExtend
    {
        private LibraryManagementSettingsViewModel PluginSettings;

        private List<ItemList> itemLists = new List<ItemList>();


        public LmFeaturesIconList(IPlayniteAPI PlayniteApi, LibraryManagementSettingsViewModel PluginSettings)
        {
            this.PluginSettings = PluginSettings;

            InitializeComponent();

            PluginSettings.PropertyChanged += PluginSettings_PropertyChanged;
            PlayniteApi.Database.Games.ItemUpdated += Games_ItemUpdated;

            // Apply settings
            PluginSettings_PropertyChanged(null, null);
        }

        #region OnPropertyChange
        // When settings is updated
        public override void PluginSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Apply settings
            this.DataContext = new
            {

            };

            // Publish changes for the currently displayed game
            GameContextChanged(null, GameContext);
        }

        // When game is changed
        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            MustDisplay = PluginSettings.Settings.EnableIntegrationFeatures;

            // When control is not used
            if (!PluginSettings.Settings.EnableIntegrationFeatures)
            {
                return;
            }


            List<ItemFeature> itemFeatures = IcoFeatures.GetAvailableItemFeatures(PluginSettings, newContext);
            itemLists = new List<ItemList>();
            itemLists = itemFeatures.Select(x => new ItemList { Name = x.NameAssociated, Icon = x.IconBitmapImage }).ToList();


            PART_FeaturesList.ItemsSource = null;
            PART_FeaturesList.ItemsSource = itemLists;


            this.DataContext = new
            {
                CountItems = itemLists.Count
            };
        }
        #endregion
    }


    public class ItemList
    {
        public string Name { get; set; }
        public BitmapImage Icon { get; set; }
    }
}
