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

namespace LibraryManagement.Views
{
    public enum TypeItem
    {
        FeatureItem, GenreItem, CategoryItem
    }

    /// <summary>
    /// Logique d'interaction pour LmTagToItemView.xaml
    /// </summary>
    public partial class LmTagToItemView : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        public dynamic NewItem;
        private TypeItem typeItem;


        public LmTagToItemView(IPlayniteAPI PlayniteApi, TypeItem typeItem)
        {
            this.typeItem = typeItem;

            InitializeComponent();

            ObservableCollection<ListElement> ListTags = null;
            ObservableCollection<ListElement> ListItems = null;
            switch (typeItem)
            {
                case TypeItem.FeatureItem:
                    ListTags = PlayniteApi.Database.Tags
                            .Select(x => new ListElement { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToObservable();

                    ListItems = PlayniteApi.Database.Features
                            .Select(x => new ListElement { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToObservable();

                    PART_ItemLabel.Content = resources.GetString("LOCFeaturesLabel");
                    break;

                case TypeItem.GenreItem:
                    ListTags = PlayniteApi.Database.Tags
                            .Select(x => new ListElement { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToObservable();

                    ListItems = PlayniteApi.Database.Genres
                            .Select(x => new ListElement { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToObservable();

                    PART_ItemLabel.Content = resources.GetString("LOCGenresLabel");
                    break;

                case TypeItem.CategoryItem:
                    ListTags = PlayniteApi.Database.Tags
                            .Select(x => new ListElement { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToObservable();

                    ListItems = PlayniteApi.Database.Categories
                            .Select(x => new ListElement { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToObservable();

                    PART_ItemLabel.Content = resources.GetString("LOCCategoriesLabel");
                    break;
            }

            PART_TagList.ItemsSource = ListTags;
            PART_ItemsList.ItemsSource = ListItems;
        }


        private void PART_Save_Click(object sender, RoutedEventArgs e)
        {
            switch (typeItem)
            {
                case TypeItem.FeatureItem:
                    NewItem = new LmTagToFeature
                    {
                        TagId = ((ListElement)PART_TagList.SelectedItem).Id,
                        TagName = ((ListElement)PART_TagList.SelectedItem).Name,
                        FeatureId = ((ListElement)PART_ItemsList.SelectedItem).Id,
                        FeatureName = ((ListElement)PART_ItemsList.SelectedItem).Name,
                    };
                    break;

                case TypeItem.GenreItem:
                    NewItem = new LmTagToGenre
                    {
                        TagId = ((ListElement)PART_TagList.SelectedItem).Id,
                        TagName = ((ListElement)PART_TagList.SelectedItem).Name,
                        GenreId = ((ListElement)PART_ItemsList.SelectedItem).Id,
                        GenreName = ((ListElement)PART_ItemsList.SelectedItem).Name,
                    };
                    break;

                case TypeItem.CategoryItem:
                    NewItem = new LmTagToCategory
                    {
                        TagId = ((ListElement)PART_TagList.SelectedItem).Id,
                        TagName = ((ListElement)PART_TagList.SelectedItem).Name,
                        CategoryId = ((ListElement)PART_ItemsList.SelectedItem).Id,
                        CategoryName = ((ListElement)PART_ItemsList.SelectedItem).Name,
                    };
                    break;
            }

            ((Window)this.Parent).Close();
        }

        private void PART_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }


        private void PART_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PART_Save.IsEnabled = PART_TagList.SelectedIndex != -1 && PART_ItemsList.SelectedIndex != -1;
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
            ((ObservableCollection<ListElement>)PART_ItemsList.ItemsSource).ForEach(x => x.IsVisible = true);

            if (!FeatureSearch.Text.IsNullOrEmpty())
            {
                ((ObservableCollection<ListElement>)PART_ItemsList.ItemsSource)
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
        public bool IsVisible { get => isVisible; set => SetValue(ref isVisible, value); }
    }
}
