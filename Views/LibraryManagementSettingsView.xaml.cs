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


        #region Genres equivalences
        private void PART_AddEquivalence_Click(object sender, RoutedEventArgs e)
        {
            var ViewExtension = new LibraryManagementItemEditor(_PlayniteApi.Database.Genres.ToList(), ItemType.Genre);

            if (!(bool)PART_Exclusion.IsChecked)
            {
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
            else
            {
                ViewExtension.OnlyAdd();
                Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmGenreAdd"), ViewExtension);
                windowExtension.ShowDialog();

                if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmGenreEquivalences)
                {
                    List<string> temp = (List<string>)PART_ListGenreExclusion.ItemsSource;
                    temp.Add(((LmGenreEquivalences)ViewExtension.NewItem).Name);

                    PART_ListGenreExclusion.ItemsSource = null;
                    PART_ListGenreExclusion.ItemsSource = temp;
                }
            }
        }

        private void PART_ManageEquivalence_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_Exclusion.IsChecked)
            {
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
            else
            {
                List<Genre> genres = _PlayniteApi.Database.Genres.ToList();

                var ViewExtension = new LibraryManagementItemEditor(
                    genres, ItemType.Feature,
                    null,
                    ((List<string>)PART_ListGenreExclusion.ItemsSource)[index],
                    string.Empty,
                    null
                );
                ViewExtension.OnlyAdd();
                Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmGenreEdit"), ViewExtension);
                windowExtension.ShowDialog();

                if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmGenreEquivalences)
                {
                    List<string> temp = (List<string>)PART_ListGenreExclusion.ItemsSource;
                    temp[index] = ((LmGenreEquivalences)ViewExtension.NewItem).Name;

                    PART_ListGenreExclusion.ItemsSource = null;
                    PART_ListGenreExclusion.ItemsSource = temp;
                }
            }
        }

        private void PART_RemoveEquivalence_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_Exclusion.IsChecked)
            {
                List<LmGenreEquivalences> temp = (List<LmGenreEquivalences>)PART_ListGenreEquivalences.ItemsSource;
                temp.RemoveAt(index);

                PART_ListGenreEquivalences.ItemsSource = null;
                PART_ListGenreEquivalences.ItemsSource = temp;
            }
            else
            {
                List<string> temp = new List<string>();
                temp = (List<string>)PART_ListGenreEquivalences.ItemsSource;
                temp.RemoveAt(index);

                PART_ListGenreEquivalences.ItemsSource = null;
                PART_ListGenreEquivalences.ItemsSource = temp;
            }
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

            if (!(bool)PART_ExclusionFeature.IsChecked)
            {                
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
            else
            {
                ViewExtension.OnlyAdd();
                Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmFeatureAdd"), ViewExtension);
                windowExtension.ShowDialog();

                if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmFeatureEquivalences)
                {
                    List<string> temp = (List<string>)PART_ListFeatureExclusion.ItemsSource;
                    temp.Add(((LmFeatureEquivalences)ViewExtension.NewItem).Name);

                    PART_ListFeatureExclusion.ItemsSource = null;
                    PART_ListFeatureExclusion.ItemsSource = temp;
                }
            }
        }

        private void PART_ManageEquivalenceFeature_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_ExclusionFeature.IsChecked)
            {
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
            else
            {
                List<GameFeature> features = _PlayniteApi.Database.Features.ToList();

                var ViewExtension = new LibraryManagementItemEditor(
                    features, ItemType.Feature,
                    null,
                    ((List<string>)PART_ListFeatureExclusion.ItemsSource)[index],
                    string.Empty,
                    null
                );
                ViewExtension.OnlyAdd();
                Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmTagEdit"), ViewExtension);
                windowExtension.ShowDialog();

                if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmFeatureEquivalences)
                {
                    List<string> temp = (List<string>)PART_ListFeatureExclusion.ItemsSource;
                    temp[index] = ((LmFeatureEquivalences)ViewExtension.NewItem).Name;

                    PART_ListFeatureExclusion.ItemsSource = null;
                    PART_ListFeatureExclusion.ItemsSource = temp;
                }
            }
        }

        private void PART_RemoveEquivalenceFeature_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_ExclusionFeature.IsChecked)
            {
                List<LmFeatureEquivalences> temp = (List<LmFeatureEquivalences>)PART_ListFeatureEquivalences.ItemsSource;
                temp.RemoveAt(index);

                PART_ListFeatureEquivalences.ItemsSource = null;
                PART_ListFeatureEquivalences.ItemsSource = temp;
            }
            else
            {
                List<string> temp = new List<string>();
                temp = (List<string>)PART_ListFeatureExclusion.ItemsSource;
                temp.RemoveAt(index);

                PART_ListFeatureExclusion.ItemsSource = null;
                PART_ListFeatureExclusion.ItemsSource = temp;
            }
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

            if (!(bool)PART_ExclusionTag.IsChecked)
            {
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
            else
            {
                ViewExtension.OnlyAdd();
                Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmTagAdd"), ViewExtension);
                windowExtension.ShowDialog();

                if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmTagsEquivalences)
                {
                    List<string> temp = (List<string>)PART_ListTagExclusion.ItemsSource;
                    temp.Add(((LmTagsEquivalences)ViewExtension.NewItem).Name);

                    PART_ListTagExclusion.ItemsSource = null;
                    PART_ListTagExclusion.ItemsSource = temp;
                }
            }
        }

        private void PART_ManageEquivalenceTag_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_ExclusionTag.IsChecked)
            {
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
                    List<LmTagsEquivalences> temp = (List<LmTagsEquivalences>)PART_ListTagEquivalences.ItemsSource;
                    temp[index] = (LmTagsEquivalences)ViewExtension.NewItem;

                    PART_ListTagEquivalences.ItemsSource = null;
                    PART_ListTagEquivalences.ItemsSource = temp;
                }
            }
            else
            {
                List<Tag> tags = _PlayniteApi.Database.Tags.ToList();

                var ViewExtension = new LibraryManagementItemEditor(
                    tags, ItemType.Tag,
                    null,
                    ((List<string>)PART_ListTagExclusion.ItemsSource)[index],
                    string.Empty,
                    null
                );
                ViewExtension.OnlyAdd();
                Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmTagEdit"), ViewExtension);
                windowExtension.ShowDialog();

                if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmTagsEquivalences)
                {
                    List<string> temp = (List<string>)PART_ListTagExclusion.ItemsSource;
                    temp[index] = ((LmTagsEquivalences)ViewExtension.NewItem).Name;

                    PART_ListTagExclusion.ItemsSource = null;
                    PART_ListTagExclusion.ItemsSource = temp;
                }
            }
        }

        private void PART_RemoveEquivalenceTag_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_ExclusionTag.IsChecked)
            {
                List<LmTagsEquivalences> temp = new List<LmTagsEquivalences>();
                temp = (List<LmTagsEquivalences>)PART_ListTagEquivalences.ItemsSource;
                temp.RemoveAt(index);

                PART_ListTagEquivalences.ItemsSource = null;
                PART_ListTagEquivalences.ItemsSource = temp;
            }
            else
            {
                List<string> temp = new List<string>();
                temp = (List<string>)PART_ListTagExclusion.ItemsSource;
                temp.RemoveAt(index);

                PART_ListTagExclusion.ItemsSource = null;
                PART_ListTagExclusion.ItemsSource = temp;
            }
        }

        private void PART_SetTag_Click(object sender, RoutedEventArgs e)
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(_plugin, _PlayniteApi, _settings);
            libraryManagementTools.SetTags();
        }
        #endregion
    }
}
