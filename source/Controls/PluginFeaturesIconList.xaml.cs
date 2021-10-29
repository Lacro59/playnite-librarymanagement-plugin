using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LibraryManagement.Controls
{
    /// <summary>
    /// Logique d'interaction pour PluginFeaturesIconList.xaml
    /// </summary>
    public partial class PluginFeaturesIconList : PluginUserControlExtendBase
    {
        private LibraryManagementSettingsViewModel PluginSettings;

        private PluginFeaturesIconListDataContext ControlDataContext;
        internal override IDataContext _ControlDataContext
        {
            get
            {
                return ControlDataContext;
            }
            set
            {
                ControlDataContext = (PluginFeaturesIconListDataContext)_ControlDataContext;
            }
        }


        public PluginFeaturesIconList(IPlayniteAPI PlayniteApi, LibraryManagementSettingsViewModel PluginSettings)
        {
            this.PluginSettings = PluginSettings;

            InitializeComponent();

            Task.Run(() =>
            {
                this.Dispatcher.BeginInvoke((Action)delegate
                {
                    PluginSettings.PropertyChanged += PluginSettings_PropertyChanged;
                    PlayniteApi.Database.Games.ItemUpdated += Games_ItemUpdated;

                    // Apply settings
                    PluginSettings_PropertyChanged(null, null);
                });
            });
        }


        public override void SetDefaultDataContext()
        {
            ControlDataContext = new PluginFeaturesIconListDataContext
            {
                IsActivated = PluginSettings.Settings.EnableIntegrationFeatures,

                CountItems = 0,
                ItemsSource = new ObservableCollection<ItemList>()
            };
        }


        public override Task<bool> SetData(Game newContext)
        {
            return Task.Run(() =>
            {
                List<ItemFeature> itemFeatures = IcoFeatures.GetAvailableItemFeatures(PluginSettings, newContext);
                ObservableCollection<ItemList> itemLists = new ObservableCollection<ItemList>();
                itemLists = itemFeatures.Select(x => new ItemList { Name = x.NameAssociated, Icon = x.IconString }).ToObservable();

                ControlDataContext.CountItems = itemLists.Count;
                ControlDataContext.ItemsSource = itemLists;

                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                {
                    this.DataContext = ControlDataContext;
                }));

                return true;
            });
        }
    }


    public class PluginFeaturesIconListDataContext : IDataContext
    {
        public bool IsActivated { get; set; }

        public int CountItems { get; set; }
        public ObservableCollection<ItemList> ItemsSource { get; set; }
    }

    public class ItemList
    {
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}
