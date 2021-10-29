using CommonPluginsShared.Extensions;
using LibraryManagement.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace LibraryManagement.Views
{
    /// <summary>
    /// Logique d'interaction pour LmTagToFeatureView.xaml
    /// </summary>
    public partial class LmTagToFeatureView : UserControl
    {
        public LmTagToFeature NewItem;


        public LmTagToFeatureView(IPlayniteAPI PlayniteApi)
        {
            InitializeComponent();

            var ListTags = PlayniteApi.Database.Tags
                    .Select(x => new ListElement { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToObservable();

            var ListFeatures = PlayniteApi.Database.Features
                    .Select(x => new ListElement { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToObservable();

            PART_TagList.ItemsSource = ListTags;
            PART_FeatureList.ItemsSource = ListFeatures;
        }


        private void PART_Save_Click(object sender, RoutedEventArgs e)
        {
            NewItem = new LmTagToFeature
            {
                TagId = ((ListElement)PART_TagList.SelectedItem).Id,
                TagName = ((ListElement)PART_TagList.SelectedItem).Name,
                FeatureId = ((ListElement)PART_FeatureList.SelectedItem).Id,
                FeatureName = ((ListElement)PART_FeatureList.SelectedItem).Name,
            };

            ((Window)this.Parent).Close();
        }

        private void PART_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }


        private void PART_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PART_Save.IsEnabled = PART_TagList.SelectedIndex != -1 && PART_FeatureList.SelectedIndex != -1;
        }


        private void TagSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((ObservableCollection<ListElement>)PART_TagList.ItemsSource)
                .ForEach(x => x.IsVisible = true);

            if (!TagSearch.Text.IsNullOrEmpty())
            {
                ((ObservableCollection<ListElement>)PART_TagList.ItemsSource)
                    .Where(x => !x.Name.RemoveDiacritics().Contains(TagSearch.Text.RemoveDiacritics(), StringComparison.InvariantCultureIgnoreCase))
                    .ForEach(x => x.IsVisible = false);
            }
        }

        private void FeatureSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((ObservableCollection<ListElement>)PART_FeatureList.ItemsSource).ForEach(x => x.IsVisible = true);

            if (!FeatureSearch.Text.IsNullOrEmpty())
            {
                ((ObservableCollection<ListElement>)PART_FeatureList.ItemsSource)
                    .Where(x => !x.Name.RemoveDiacritics().Contains(FeatureSearch.Text.RemoveDiacritics(), StringComparison.InvariantCultureIgnoreCase))
                    .ForEach(x => x.IsVisible = false);
            }
        }
    }


    public class ListElement : ObservableObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        private bool isVisible = true;
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                isVisible = value;
                OnPropertyChanged();
            }
        }
    }
}
