using CommonPluginsPlaynite.Common;
using CommonPluginsShared;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            if (!(bool)PART_Exclusion.IsChecked)
            {
                AddElement<LmGenreEquivalences>(PART_ListGenreEquivalences, ItemType.Genre, _PlayniteApi.Database.Genres.ToList(), false);
            }
            else
            {
                AddElement<string>(PART_ListGenreExclusion, ItemType.Genre, _PlayniteApi.Database.Genres.ToList(), true);
            }
        }

        private void PART_ManageEquivalence_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_Exclusion.IsChecked)
            {
                List<string> ListAlreadyAdded = ((List<LmGenreEquivalences>)PART_ListGenreEquivalences.ItemsSource)[index].OldNames;
                List<Genre> genres = _PlayniteApi.Database.Genres.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower())).ToList();

                ManageElement<LmGenreEquivalences>(PART_ListGenreEquivalences, index, ListAlreadyAdded,
                    ItemType.Genre, genres, ((List<LmGenreEquivalences>)PART_ListGenreEquivalences.ItemsSource)[index], false);
            }
            else
            {
                List<Genre> genres = _PlayniteApi.Database.Genres.ToList();
                LmFeatureEquivalences lmEquivalences = new LmFeatureEquivalences { Name = ((List<string>)PART_ListGenreExclusion.ItemsSource)[index] };

                ManageElement<string>(PART_ListGenreExclusion, index, null,
                    ItemType.Genre, genres, lmEquivalences, true);
            }
        }

        private void PART_RemoveEquivalence_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_Exclusion.IsChecked)
            {
                RemoveElement<LmGenreEquivalences>(PART_ListGenreEquivalences, index);
            }
            else
            {
                RemoveElement<string>(PART_ListGenreExclusion, index);
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
            if (!(bool)PART_ExclusionFeature.IsChecked)
            {
                AddElement<LmFeatureEquivalences>(PART_ListFeatureEquivalences, ItemType.Feature, _PlayniteApi.Database.Features.ToList(), false);
            }
            else
            {
                AddElement<string>(PART_ListFeatureExclusion, ItemType.Feature, _PlayniteApi.Database.Features.ToList(), true);
            }
        }

        private void PART_ManageEquivalenceFeature_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_ExclusionFeature.IsChecked)
            {
                List<string> ListAlreadyAdded = ((List<LmFeatureEquivalences>)PART_ListFeatureEquivalences.ItemsSource)[index].OldNames;
                List<GameFeature> features = _PlayniteApi.Database.Features.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower())).ToList();

                ManageElement<LmFeatureEquivalences>(PART_ListFeatureEquivalences, index, ListAlreadyAdded,
                    ItemType.Feature, features, ((List<LmFeatureEquivalences>)PART_ListFeatureEquivalences.ItemsSource)[index], false);
            }
            else
            {
                List<GameFeature> features = _PlayniteApi.Database.Features.ToList();
                LmFeatureEquivalences lmEquivalences = new LmFeatureEquivalences { Name = ((List<string>)PART_ListFeatureExclusion.ItemsSource)[index] };

                ManageElement<string>(PART_ListFeatureExclusion, index, null,
                    ItemType.Feature, features, lmEquivalences, true);
            }
        }

        private void PART_RemoveEquivalenceFeature_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_ExclusionFeature.IsChecked)
            {
                RemoveElement<LmFeatureEquivalences>(PART_ListFeatureEquivalences, index);
            }
            else
            {
                RemoveElement<string>(PART_ListFeatureExclusion, index);
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
            if (!(bool)PART_ExclusionTag.IsChecked)
            {
                AddElement<LmTagsEquivalences>(PART_ListTagEquivalences, ItemType.Tag, _PlayniteApi.Database.Tags.ToList(), false);
            }
            else
            {
                AddElement<string>(PART_ListTagExclusion, ItemType.Tag, _PlayniteApi.Database.Tags.ToList(), true);
            }
        }

        private void PART_ManageEquivalenceTag_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_ExclusionTag.IsChecked)
            {
                List<string> ListAlreadyAdded = ((List<LmTagsEquivalences>)PART_ListTagEquivalences.ItemsSource)[index].OldNames;
                List<Tag> tags = _PlayniteApi.Database.Tags.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower())).ToList();

                ManageElement<LmTagsEquivalences>(PART_ListTagEquivalences, index, ListAlreadyAdded,
                    ItemType.Tag, tags, ((List<LmTagsEquivalences>)PART_ListTagEquivalences.ItemsSource)[index], false);

            }
            else
            {
                List<Tag> tags = _PlayniteApi.Database.Tags.ToList();
                LmTagsEquivalences lmEquivalences = new LmTagsEquivalences { Name = ((List<string>)PART_ListTagExclusion.ItemsSource)[index] };

                ManageElement<string>(PART_ListTagExclusion, index, null,
                    ItemType.Tag, tags, lmEquivalences, true);
            }
        }

        private void PART_RemoveEquivalenceTag_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());
            
            if (!(bool)PART_ExclusionTag.IsChecked)
            {
                RemoveElement<LmTagsEquivalences>(PART_ListTagEquivalences, index);
            }
            else
            {
                RemoveElement<string>(PART_ListTagExclusion, index);
            }
        }

        private void PART_SetTag_Click(object sender, RoutedEventArgs e)
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(_plugin, _PlayniteApi, _settings);
            libraryManagementTools.SetTags();
        }
        #endregion


        private void AddElement<TItemList>(ListView CtrlListView, ItemType itemType, object ListData, bool IsExclusion)
        {
            LibraryManagementItemEditor ViewExtension = new LibraryManagementItemEditor(ListData, itemType);

            if (IsExclusion)
            {
                ViewExtension.OnlySimple();
            }

            string TitleWindows = string.Empty;
            switch (itemType)
            {
                case ItemType.Genre:
                    TitleWindows = resources.GetString("LOCLmGenreAdd");
                    break;
                case ItemType.Tag:
                    TitleWindows = resources.GetString("LOCLmTagAdd");
                    break;
                case ItemType.Feature:
                    TitleWindows = resources.GetString("LOCLmFeatureAdd");
                    break;
            }

            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, TitleWindows, ViewExtension);
            windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmEquivalences)
            {
                List<TItemList> temp = (List<TItemList>)CtrlListView.ItemsSource;
                if (IsExclusion)
                {
                    temp.Add(((dynamic)ViewExtension.NewItem).Name);
                }
                else
                {
                    temp.Add((TItemList)ViewExtension.NewItem);
                }

                CtrlListView.ItemsSource = null;
                CtrlListView.ItemsSource = temp;
            }
        }

        private void ManageElement<TItemList>(ListView CtrlListView, int Index, List<string> ListAlreadyAdded,
            ItemType itemType, object ListData, LmEquivalences Data, bool IsExclusion)
        {
            LibraryManagementItemEditor ViewExtension = null;

            if (IsExclusion)
            {
                ViewExtension = new LibraryManagementItemEditor(
                    ListData, itemType,
                    null, Data.Name, string.Empty,
                    null
                );
                ViewExtension.OnlySimple();
            }
            else
            {
                ViewExtension = new LibraryManagementItemEditor(
                    ListData, itemType,
                    Data.Id, Data.Name, Data.IconUnicode,
                    ListAlreadyAdded
                );
            }

            string TitleWindows = string.Empty;
            switch (itemType)
            {
                case ItemType.Genre:
                    TitleWindows = resources.GetString("LOCLmGenreEdit");
                    break;
                case ItemType.Tag:
                    TitleWindows = resources.GetString("LOCLmTagEdit");
                    break;
                case ItemType.Feature:
                    TitleWindows = resources.GetString("LOCLmFeatureEdit");
                    break;
            }

            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, TitleWindows, ViewExtension);
            windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmEquivalences)
            {
                List<TItemList> temp = (List<TItemList>)CtrlListView.ItemsSource;
                if (IsExclusion)
                {
                    temp[Index] = ((dynamic)ViewExtension.NewItem).Name;
                }
                else
                {
                    temp[Index] = (TItemList)ViewExtension.NewItem;
                }

                CtrlListView.ItemsSource = null;
                CtrlListView.ItemsSource = temp;
            }
        }

        private void RemoveElement<TItemList>(ListView CtrlListView, int Index)
        {
            List<TItemList> temp = new List<TItemList>();
            temp = (List<TItemList>)CtrlListView.ItemsSource;
            temp.RemoveAt(Index);

            CtrlListView.ItemsSource = null;
            CtrlListView.ItemsSource = temp;
        }


        #region Features icon
        private void CheckBoxDark_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            List<ItemFeature> itemFeatures = ((List<ItemFeature>)PART_ListItemFeatures.ItemsSource);

            if ((bool)cb.IsChecked)
            {
                itemFeatures.ForEach(x => x.IsDark = true);
            }
            else
            {
                itemFeatures.ForEach(x => x.IsDark = false);
            }

            PART_ListItemFeatures.ItemsSource = null;
            PART_ListItemFeatures.ItemsSource = itemFeatures;
        }

        private void CheckBoxGog_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            List<ItemFeature> itemFeatures = ((List<ItemFeature>)PART_ListItemFeatures.ItemsSource);

            if ((bool)cb.IsChecked)
            {
                itemFeatures.ForEach(x => x.IsGog = true);
            }
            else
            {
                itemFeatures.ForEach(x => x.IsGog = false);
            }

            PART_ListItemFeatures.ItemsSource = null;
            PART_ListItemFeatures.ItemsSource = itemFeatures;
        }


        private void PART_AddCustomIcon_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            var result = _PlayniteApi.Dialogs.SelectIconFile();
            if (!result.IsNullOrEmpty())
            {
                string PathDest = Path.Combine(_plugin.GetPluginUserDataPath(), Path.GetFileName(result));
                FileSystem.CopyFile(result, PathDest);

                List<ItemFeature> itemFeatures = ((List<ItemFeature>)PART_ListItemFeatures.ItemsSource);
                itemFeatures[index].IconCustom = PathDest;

                PART_ListItemFeatures.ItemsSource = null;
                PART_ListItemFeatures.ItemsSource = itemFeatures;
            }
        }

        private void PART_RemoveCustomIcon_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            List<ItemFeature> itemFeatures = ((List<ItemFeature>)PART_ListItemFeatures.ItemsSource);
            itemFeatures[index].IconCustom = string.Empty;

            PART_ListItemFeatures.ItemsSource = null;
            PART_ListItemFeatures.ItemsSource = itemFeatures;
        }
        
        private void PART_EditName_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            List<GameFeature> features = _PlayniteApi.Database.Features.ToList();

            LibraryManagementItemEditor ViewExtension = new LibraryManagementItemEditor(
                  features, ItemType.Feature,
                  null, (((List<ItemFeature>)PART_ListItemFeatures.ItemsSource)[index]).NameAssociated, string.Empty,
                  null
              );
            ViewExtension.OnlySimple();

            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCLmNameAssociated"), ViewExtension);
            windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmEquivalences)
            {
                List<ItemFeature> itemFeatures = ((List<ItemFeature>)PART_ListItemFeatures.ItemsSource);
                itemFeatures[index].NameAssociated = ((LmEquivalences)ViewExtension.NewItem).Name;

                PART_ListItemFeatures.ItemsSource = null;
                PART_ListItemFeatures.ItemsSource = itemFeatures;
            }
        }

        private void PART_RemoveName_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            List<ItemFeature> itemFeatures = ((List<ItemFeature>)PART_ListItemFeatures.ItemsSource);
            itemFeatures[index].NameAssociated = string.Empty;

            PART_ListItemFeatures.ItemsSource = null;
            PART_ListItemFeatures.ItemsSource = itemFeatures;
        }
        #endregion
    }
}
