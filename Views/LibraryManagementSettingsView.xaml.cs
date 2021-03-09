using CommonPluginsShared;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
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
    public partial class LibraryManagementSettingsView : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private LibraryManagement _plugin;
        private IPlayniteAPI _PlayniteApi;
        private LibraryManagementSettings _settings;

        public LibraryManagementSettingsView(LibraryManagement plugin, IPlayniteAPI PlayniteApi, LibraryManagementSettings settings)
        {
            _plugin = plugin;
            _PlayniteApi = PlayniteApi;
            _settings = settings;

            InitializeComponent();
        }

        /*
        private List<string> GetListGenres(List<string> ListAlreadyAdded)
        {
            List<string> ListReturn = new List<string>();
            
            foreach (Genre genre in _PlayniteApi.Database.Genres.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower())))
            {
                ListReturn.Add(genre.Name);
            }

            return ListReturn;
        }

        private List<string> GetListFeatures(List<string> ListAlreadyAdded)
        {
            List<string> ListReturn = new List<string>();
            
            foreach (GameFeature genre in _PlayniteApi.Database.Features.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower())))
            {
                ListReturn.Add(genre.Name);
            }

            return ListReturn;
        }

        private List<string> GetListTags(List<string> ListAlreadyAdded)
        {
            List<string> ListReturn = new List<string>();
            
            foreach (Tag tag in _PlayniteApi.Database.Tags.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower())))
            {
                ListReturn.Add(tag.Name);
            }

            return ListReturn;
        }
        */

        #region Genres equivalences
        private void PART_AddEquivalence_Click(object sender, RoutedEventArgs e)
        {
            var ViewExtension = new LibraryManagementItemEditor(_PlayniteApi.Database.Genres.ToList(), ItemType.Genre);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmGenreAdd"), ViewExtension);
            windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmGenreEquivalences)
            {
                List<LmGenreEquivalences> temp = (List<LmGenreEquivalences>)PART_ListGenreEquivalences.ItemsSource;
                temp.Add((LmGenreEquivalences)ViewExtension.NewItem);

                PART_ListGenreEquivalences.ItemsSource = null;
                PART_ListGenreEquivalences.ItemsSource = temp;
            }
        }

        private void PART_ManageEquivalence_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());
            List<string> ListAlreadyAdded = ((List<LmGenreEquivalences>)PART_ListGenreEquivalences.ItemsSource)[index].OldNames;
            List<Genre> genres = _PlayniteApi.Database.Genres.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower())).ToList();

            var ViewExtension = new LibraryManagementItemEditor(
                genres, ItemType.Genre, 
                ((List<LmGenreEquivalences>)PART_ListGenreEquivalences.ItemsSource)[index].Id,
                ((List<LmGenreEquivalences>)PART_ListGenreEquivalences.ItemsSource)[index].Name,
                ((List<LmGenreEquivalences>)PART_ListGenreEquivalences.ItemsSource)[index].IconUnicode, 
                ListAlreadyAdded
            );
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmGenreEdit"), ViewExtension);
            windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmGenreEquivalences)
            {
                List<LmGenreEquivalences> temp = (List<LmGenreEquivalences>)PART_ListGenreEquivalences.ItemsSource;
                temp[index] = (LmGenreEquivalences)ViewExtension.NewItem;

                PART_ListGenreEquivalences.ItemsSource = null;
                PART_ListGenreEquivalences.ItemsSource = temp;
            }
        }

        private void PART_RemoveEquivalence_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            List<LmGenreEquivalences> temp = (List<LmGenreEquivalences>)PART_ListGenreEquivalences.ItemsSource;
            temp.RemoveAt(index);

            PART_ListGenreEquivalences.ItemsSource = null;
            PART_ListGenreEquivalences.ItemsSource = temp;
        }

        private void PART_SetGenres_Click(object sender, RoutedEventArgs e)
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(_plugin, _PlayniteApi, _settings);
            libraryManagementTools.SetGenres();
        }
        #endregion


        #region Features equivalences
        private void PART_AddEquivalenceFeature_Click(object sender, RoutedEventArgs e)
        {
            var ViewExtension = new LibraryManagementItemEditor(_PlayniteApi.Database.Features.ToList(), ItemType.Feature);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmFeatureAdd"), ViewExtension);
            windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmFeatureEquivalences)
            {
                List<LmFeatureEquivalences> temp = (List<LmFeatureEquivalences>)PART_ListFeatureEquivalences.ItemsSource;
                temp.Add((LmFeatureEquivalences)ViewExtension.NewItem);

                PART_ListFeatureEquivalences.ItemsSource = null;
                PART_ListFeatureEquivalences.ItemsSource = temp;
            }
        }

        private void PART_ManageEquivalenceFeature_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());
            List<string> ListAlreadyAdded = ((List<LmFeatureEquivalences>)PART_ListFeatureEquivalences.ItemsSource)[index].OldNames;
            List<GameFeature> features = _PlayniteApi.Database.Features.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower())).ToList();

            var ViewExtension = new LibraryManagementItemEditor(
                features, ItemType.Feature,
                ((List<LmFeatureEquivalences>)PART_ListFeatureEquivalences.ItemsSource)[index].Id,
                ((List<LmFeatureEquivalences>)PART_ListFeatureEquivalences.ItemsSource)[index].Name,
                ((List<LmFeatureEquivalences>)PART_ListFeatureEquivalences.ItemsSource)[index].IconUnicode,
                ListAlreadyAdded
            );
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmFeatureEdit"), ViewExtension);
            windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmFeatureEquivalences)
            {
                List<LmFeatureEquivalences> temp = (List<LmFeatureEquivalences>)PART_ListFeatureEquivalences.ItemsSource;
                temp[index] = (LmFeatureEquivalences)ViewExtension.NewItem;

                PART_ListFeatureEquivalences.ItemsSource = null;
                PART_ListFeatureEquivalences.ItemsSource = temp;
            }
        }

        private void PART_RemoveEquivalenceFeature_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            List<LmFeatureEquivalences> temp = (List<LmFeatureEquivalences>)PART_ListFeatureEquivalences.ItemsSource;
            temp.RemoveAt(index);

            PART_ListFeatureEquivalences.ItemsSource = null;
            PART_ListFeatureEquivalences.ItemsSource = temp;
        }

        private void PART_SetFeatures_Click(object sender, RoutedEventArgs e)
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(_plugin, _PlayniteApi, _settings);
            libraryManagementTools.SetFeatures();
        }
        #endregion


        #region Tags
        private void PART_AddEquivalenceTag_Click(object sender, RoutedEventArgs e)
        {
            var ViewExtension = new LibraryManagementItemEditor(_PlayniteApi.Database.Tags.ToList(), ItemType.Tag);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmTagAdd"), ViewExtension);
            windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmTagsEquivalences)
            {
                List<LmTagsEquivalences> temp = (List<LmTagsEquivalences>)PART_ListTagEquivalences.ItemsSource;
                temp.Add((LmTagsEquivalences)ViewExtension.NewItem);

                PART_ListTagEquivalences.ItemsSource = null;
                PART_ListTagEquivalences.ItemsSource = temp;
            }
        }

        private void PART_ManageEquivalenceTag_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());
            List<string> ListAlreadyAdded = ((List<LmTagsEquivalences>)PART_ListTagEquivalences.ItemsSource)[index].OldNames;
            List<Tag> tags = _PlayniteApi.Database.Tags.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower())).ToList();

            var ViewExtension = new LibraryManagementItemEditor(
                tags, ItemType.Tag,
                ((List<LmTagsEquivalences>)PART_ListTagEquivalences.ItemsSource)[index].Id,
                ((List<LmTagsEquivalences>)PART_ListTagEquivalences.ItemsSource)[index].Name,
                ((List<LmTagsEquivalences>)PART_ListTagEquivalences.ItemsSource)[index].IconUnicode,
                ListAlreadyAdded
            );
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmTagEdit"), ViewExtension);
            windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmTagsEquivalences)
            {
                List<LmTagsEquivalences> temp = (List<LmTagsEquivalences>)PART_ListFeatureEquivalences.ItemsSource;
                temp[index] = (LmTagsEquivalences)ViewExtension.NewItem;

                PART_ListTagEquivalences.ItemsSource = null;
                PART_ListTagEquivalences.ItemsSource = temp;
            }
        }

        private void PART_RemoveEquivalenceTag_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            List<LmTagsEquivalences> temp = (List<LmTagsEquivalences>)PART_ListTagEquivalences.ItemsSource;
            temp.RemoveAt(index);

            PART_ListTagEquivalences.ItemsSource = null;
            PART_ListTagEquivalences.ItemsSource = temp;
        }

        private void PART_SetTag_Click(object sender, RoutedEventArgs e)
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(_plugin, _PlayniteApi, _settings);
            libraryManagementTools.SetTags();
        }
        #endregion
    }
}
