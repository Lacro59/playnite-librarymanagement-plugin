﻿using CommonPluginsShared.Controls;
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
        private LibraryManagementSettingsViewModel PluginSettings { get; set; }

        private PluginAgeRatingDataContext ControlDataContext = new PluginAgeRatingDataContext();
        internal override IDataContext controlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PluginAgeRatingDataContext)controlDataContext;
        }


        public PluginAgeRating(LibraryManagementSettingsViewModel PluginSettings)
        {
            this.PluginSettings = PluginSettings;

            InitializeComponent();
            this.DataContext = ControlDataContext;

            Task.Run(() =>
            {
                this.Dispatcher.BeginInvoke((Action)delegate
                {
                    PluginSettings.PropertyChanged += PluginSettings_PropertyChanged;
                    API.Instance.Database.Games.ItemUpdated += Games_ItemUpdated;

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
            Models.AgeRating finded = PluginSettings.Settings.AgeRatings?
                .Where(x => newContext.AgeRatings?.Where(y => x.AgeRatingIds.Any(z => z == y.Id))?.Count() > 0)?
                .OrderByDescending(x => x.Age)
                .FirstOrDefault();

            if (finded != null)
            {
                this.Visibility = System.Windows.Visibility.Visible;
                ControlDataContext.Color = finded.Color;
                ControlDataContext.Age = finded.Age.ToString();
            }
            else if (PluginSettings.Settings.ShowMissingAge)
            {
                this.Visibility = System.Windows.Visibility.Visible;
                ControlDataContext.Color = null;
                ControlDataContext.Age = "?";
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
        private bool isActivated;
        public bool IsActivated { get => isActivated; set => SetValue(ref isActivated, value); }

    
        private SolidColorBrush color = new SolidColorBrush(Colors.Gray);
        public SolidColorBrush Color { get => color; set => SetValue(ref color, value); }
    
        private string age = "?";
        public string Age { get => age; set => SetValue(ref age, value); }
    }
}
