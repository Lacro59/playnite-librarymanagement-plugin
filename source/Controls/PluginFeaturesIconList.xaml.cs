using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
using LibraryManagement.Models;
using LibraryManagement.Services;
using MoreLinq;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.Controls
{
    /// <summary>
    /// Logique d'interaction pour PluginFeaturesIconList.xaml
    /// </summary>
    public partial class PluginFeaturesIconList : PluginUserControlExtendBase
    {
        private LibraryManagementSettingsViewModel PluginSettings;

        private PluginFeaturesIconListDataContext ControlDataContext = new PluginFeaturesIconListDataContext();
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
            this.DataContext = ControlDataContext;

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
            ControlDataContext.IsActivated = PluginSettings.Settings.EnableIntegrationFeatures;

            ControlDataContext.CountItems = 0;
            ControlDataContext.ItemsSource = new ObservableCollection<ItemList>();
        }


        public override void SetData(Game newContext)
        {
            List<ItemFeature> itemFeatures = IcoFeatures.GetAvailableItemFeatures(PluginSettings, newContext);
            ObservableCollection<ItemList> itemLists = new ObservableCollection<ItemList>();
            itemLists = itemFeatures.Select(x => new ItemList { Name = x.NameAssociated, Icon = x.IconString }).ToObservable();

            if (PluginSettings.Settings.OneForSameIcon)
            {
                itemFeatures.ForEach(x => 
                {
                    string NewName = string.Empty;
                    itemLists.Where(y => x.IconString == y.Icon)?.ToList().ForEach(y => 
                    {
                        if (!y.Name.Contains(Environment.NewLine))
                        {
                            if (NewName.IsNullOrEmpty())
                            {
                                NewName = y.Name;
                            }
                            else
                            {
                                NewName += Environment.NewLine + y.Name;
                            }
                        }
                    });
                    if (!NewName.IsNullOrEmpty())
                    {
                        itemLists.Where(y => x.IconString == y.Icon)?.ToList().ForEach(y => y.Name = NewName);
                    }
                });

                itemLists = itemLists.DistinctBy(x=> x.Icon).ToObservable();
            }

            ControlDataContext.CountItems = itemLists.Count;
            ControlDataContext.ItemsSource = itemLists;
        }
    }


    public class PluginFeaturesIconListDataContext : ObservableObject, IDataContext
    {
        private bool _IsActivated;
        public bool IsActivated { get => _IsActivated; set => SetValue(ref _IsActivated, value); }

        public int _CountItems;
        public int CountItems { get => _CountItems; set => SetValue(ref _CountItems, value); }

        public ObservableCollection<ItemList> _ItemsSource;
        public ObservableCollection<ItemList> ItemsSource { get => _ItemsSource; set => SetValue(ref _ItemsSource, value); }
    }

    public class ItemList
    {
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}
