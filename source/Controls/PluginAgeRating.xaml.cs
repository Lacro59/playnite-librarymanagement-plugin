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
using System.Windows.Media;

namespace LibraryManagement.Controls
{
    /// <summary>
    /// Logique d'interaction pour PluginAgeRating.xaml
    /// </summary>
    public partial class PluginAgeRating : PluginUserControlExtendBase
    {
        private LibraryManagementSettingsViewModel PluginSettings;

        private PluginAgeRatingDataContext ControlDataContext = new PluginAgeRatingDataContext();
        internal override IDataContext _ControlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PluginAgeRatingDataContext)_ControlDataContext;
        }


        public PluginAgeRating(IPlayniteAPI PlayniteApi, LibraryManagementSettingsViewModel PluginSettings)
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
            ControlDataContext.IsActivated = PluginSettings.Settings.EnableIntegrationAgeRatings;
        }


        public override void SetData(Game newContext)
        {
            Models.AgeRating finded = PluginSettings.Settings.AgeRatings?.Where(x => newContext.AgeRatings?.Where(y => x.AgeRatingIds.Any(z => z == y.Id))?.Count() > 0)?.FirstOrDefault();
            if (finded != null)
            {
                this.Visibility = System.Windows.Visibility.Visible;
                ControlDataContext.Color = finded.Color;
                ControlDataContext.Age = finded.Age.ToString();
            }
            else
            {
                this.Visibility = System.Windows.Visibility.Collapsed;
                ControlDataContext.Color = null;
                ControlDataContext.Age = string.Empty;
            }
        }
    }


    public class PluginAgeRatingDataContext : ObservableObject, IDataContext
    {
        private bool _IsActivated;
        public bool IsActivated { get => _IsActivated; set => SetValue(ref _IsActivated, value); }

    
        private SolidColorBrush _Color;
        public SolidColorBrush Color { get => _Color; set => SetValue(ref _Color, value); }
    
        private string _Age;
        public string Age { get => _Age; set => SetValue(ref _Age, value); }
    }
}
