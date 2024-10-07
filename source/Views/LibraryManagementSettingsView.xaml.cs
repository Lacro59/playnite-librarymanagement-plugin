using CommonPlayniteShared.Common;
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
        private LibraryManagement Plugin { get; set; }
        private LibraryManagementSettings Settings { get; set; }

        public LibraryManagementSettingsView(LibraryManagement plugin, LibraryManagementSettings settings)
        {
            Plugin = plugin;
            Settings = settings;

            InitializeComponent();
        }


        #region Companies equivalences
        private void PART_AddEquivalenceCompany_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)PART_ExclusionCompany.IsChecked)
            {
                AddElement<LmCompaniesEquivalences>(PART_ListCompanyEquivalences, ItemType.Company, API.Instance.Database.Companies.ToList(), false);
            }
            else
            {
                AddElement<string>(PART_ListCompanyExclusion, ItemType.Company, API.Instance.Database.Companies.ToList(), true);
            }
        }

        private void PART_ManageEquivalenceCompany_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_ExclusionCompany.IsChecked)
            {
                List<string> ListAlreadyAdded = ((List<LmCompaniesEquivalences>)PART_ListCompanyEquivalences.ItemsSource)[index].OldNames;
                List<Company> companies = API.Instance.Database.Companies.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower()))?.ToList() ?? new List<Company>();

                ManageElement<LmCompaniesEquivalences>(PART_ListCompanyEquivalences, index, ListAlreadyAdded,
                    ItemType.Company, companies, ((List<LmCompaniesEquivalences>)PART_ListCompanyEquivalences.ItemsSource)[index], false);
            }
            else
            {
                List<Company> companies = API.Instance.Database.Companies.ToList();
                LmCompaniesEquivalences lmEquivalences = new LmCompaniesEquivalences { Name = ((List<string>)PART_ListCompanyExclusion.ItemsSource)[index] };

                ManageElement<string>(PART_ListCompanyExclusion, index, null,
                    ItemType.Company, companies, lmEquivalences, true);
            }
        }

        private void PART_RemoveEquivalenceCompany_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_ExclusionCompany.IsChecked)
            {
                RemoveElement<LmCompaniesEquivalences>(PART_ListCompanyEquivalences, index);
            }
            else
            {
                RemoveElement<string>(PART_ListCompanyExclusion, index);
            }
        }

        private void PART_SetCompanies_Click(object sender, RoutedEventArgs e)
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(Plugin, Settings);
            libraryManagementTools.SetCompanies();
        }
        #endregion


        #region Genres equivalences
        private void PART_AddEquivalence_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)PART_Exclusion.IsChecked)
            {
                AddElement<LmGenreEquivalences>(PART_ListGenreEquivalences, ItemType.Genre, API.Instance.Database.Genres.ToList(), false);
            }
            else
            {
                AddElement<string>(PART_ListGenreExclusion, ItemType.Genre, API.Instance.Database.Genres.ToList(), true);
            }
        }

        private void PART_ManageEquivalence_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_Exclusion.IsChecked)
            {
                List<string> ListAlreadyAdded = ((List<LmGenreEquivalences>)PART_ListGenreEquivalences.ItemsSource)[index].OldNames;
                List<Genre> genres = API.Instance.Database.Genres.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower()))?.ToList() ?? new List<Genre>();

                ManageElement<LmGenreEquivalences>(PART_ListGenreEquivalences, index, ListAlreadyAdded,
                    ItemType.Genre, genres, ((List<LmGenreEquivalences>)PART_ListGenreEquivalences.ItemsSource)[index], false);
            }
            else
            {
                List<Genre> genres = API.Instance.Database.Genres.ToList();
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
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(Plugin, Settings);
            libraryManagementTools.SetGenres();
        }
        #endregion


        #region Features equivalences
        private void PART_AddEquivalenceFeature_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)PART_ExclusionFeature.IsChecked)
            {
                AddElement<LmFeatureEquivalences>(PART_ListFeatureEquivalences, ItemType.Feature, API.Instance.Database.Features.ToList(), false);
            }
            else
            {
                AddElement<string>(PART_ListFeatureExclusion, ItemType.Feature, API.Instance.Database.Features.ToList(), true);
            }
        }

        private void PART_ManageEquivalenceFeature_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_ExclusionFeature.IsChecked)
            {
                List<string> ListAlreadyAdded = ((List<LmFeatureEquivalences>)PART_ListFeatureEquivalences.ItemsSource)[index].OldNames;
                List<GameFeature> features = API.Instance.Database.Features.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower()))?.ToList() ?? new List<GameFeature>();

                ManageElement<LmFeatureEquivalences>(PART_ListFeatureEquivalences, index, ListAlreadyAdded,
                    ItemType.Feature, features, ((List<LmFeatureEquivalences>)PART_ListFeatureEquivalences.ItemsSource)[index], false);
            }
            else
            {
                List<GameFeature> features = API.Instance.Database.Features.ToList();
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
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(Plugin, Settings);
            libraryManagementTools.SetFeatures();
        }
        #endregion


        #region Tags equivalences
        private void PART_AddEquivalenceTag_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)PART_ExclusionTag.IsChecked)
            {
                AddElement<LmTagsEquivalences>(PART_ListTagEquivalences, ItemType.Tag, API.Instance.Database.Tags.ToList(), false);
            }
            else
            {
                AddElement<string>(PART_ListTagExclusion, ItemType.Tag, API.Instance.Database.Tags.ToList(), true);
            }
        }

        private void PART_ManageEquivalenceTag_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            if (!(bool)PART_ExclusionTag.IsChecked)
            {
                List<string> ListAlreadyAdded = ((List<LmTagsEquivalences>)PART_ListTagEquivalences.ItemsSource)[index].OldNames;
                List<Tag> tags = API.Instance.Database.Tags.Where(x => !ListAlreadyAdded.Any(y => x.Name.ToLower() == y.ToLower()))?.ToList() ?? new List<Tag>();

                ManageElement<LmTagsEquivalences>(PART_ListTagEquivalences, index, ListAlreadyAdded,
                    ItemType.Tag, tags, ((List<LmTagsEquivalences>)PART_ListTagEquivalences.ItemsSource)[index], false);

            }
            else
            {
                List<Tag> tags = API.Instance.Database.Tags.ToList();
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
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(Plugin, Settings);
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
                    TitleWindows = ResourceProvider.GetString("LOCLmGenreAdd");
                    break;
                case ItemType.Tag:
                    TitleWindows = ResourceProvider.GetString("LOCLmTagAdd");
                    break;
                case ItemType.Feature:
                    TitleWindows = ResourceProvider.GetString("LOCLmFeatureAdd");
                    break;
                case ItemType.Company:
                    TitleWindows = ResourceProvider.GetString("LOCLmCompanyAdd");
                    break;
                default:
                    break;
            }

            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow( TitleWindows, ViewExtension);
            _ = windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmEquivalences)
            {
                List<TItemList> temp = (List<TItemList>)CtrlListView.ItemsSource;
                if (temp == null)
                {
                    temp = new List<TItemList>();
                }

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

        private void ManageElement<TItemList>(ListView CtrlListView, int Index, List<string> ListAlreadyAdded, ItemType itemType, object ListData, LmEquivalences Data, bool IsExclusion)
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
                    TitleWindows = ResourceProvider.GetString("LOCLmGenreEdit");
                    break;
                case ItemType.Tag:
                    TitleWindows = ResourceProvider.GetString("LOCLmTagEdit");
                    break;
                case ItemType.Feature:
                    TitleWindows = ResourceProvider.GetString("LOCLmFeatureEdit");
                    break;
                case ItemType.Company:
                    TitleWindows = ResourceProvider.GetString("LOCLmCompanyEdit");
                    break;
                default:
                    break;
            }

            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow( TitleWindows, ViewExtension);
            _ = windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null && ViewExtension.NewItem is LmEquivalences)
            {
                List<TItemList> temp = (List<TItemList>)CtrlListView.ItemsSource;
                temp[Index] = IsExclusion ? (TItemList)((dynamic)ViewExtension.NewItem).Name : (TItemList)ViewExtension.NewItem;

                CtrlListView.ItemsSource = null;
                CtrlListView.ItemsSource = temp;
            }
        }

        private void RemoveElement<TItemList>(ListView CtrlListView, int Index)
        {
            List<TItemList> temp = (List<TItemList>)CtrlListView.ItemsSource;
            temp.RemoveAt(Index);

            CtrlListView.ItemsSource = null;
            CtrlListView.ItemsSource = temp;
        }


        #region Features icon
        private void CheckBoxDark_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            List<ItemFeature> itemFeatures = (List<ItemFeature>)PART_ListItemFeatures.ItemsSource;

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
            List<ItemFeature> itemFeatures = (List<ItemFeature>)PART_ListItemFeatures.ItemsSource;

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


        private void PART_AddNewFeature_Click(object sender, RoutedEventArgs e)
        {
            bool IsGog = ((List<ItemFeature>)PART_ListItemFeatures.ItemsSource).FirstOrDefault().IsGog;
            bool IsDark = ((List<ItemFeature>)PART_ListItemFeatures.ItemsSource).FirstOrDefault().IsDark;

            AddEditNewFeatureIcon ViewExtension = new AddEditNewFeatureIcon(Plugin, IsGog, IsDark);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmTagAddNewFeature"), ViewExtension);
            _ = windowExtension.ShowDialog();

            if (ViewExtension.ItemFeature != null)
            {
                List<ItemFeature> itemFeatures = (List<ItemFeature>)PART_ListItemFeatures.ItemsSource;
                itemFeatures.Add(ViewExtension.ItemFeature);

                PART_ListItemFeatures.ItemsSource = null;
                PART_ListItemFeatures.ItemsSource = itemFeatures;
            }
        }
        
        private void PART_EditFeature_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());
            bool IsGog = ((List<ItemFeature>)PART_ListItemFeatures.ItemsSource).FirstOrDefault().IsGog;
            bool IsDark = ((List<ItemFeature>)PART_ListItemFeatures.ItemsSource).FirstOrDefault().IsDark;

            AddEditNewFeatureIcon ViewExtension = new AddEditNewFeatureIcon(Plugin, IsGog, IsDark, ((List<ItemFeature>)PART_ListItemFeatures.ItemsSource)[index]);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmTagEditFeature"), ViewExtension);
            _ = windowExtension.ShowDialog();

            if (ViewExtension.ItemFeature != null)
            {
                List<ItemFeature> itemFeatures = (List<ItemFeature>)PART_ListItemFeatures.ItemsSource;
                itemFeatures[index] = ViewExtension.ItemFeature;

                PART_ListItemFeatures.ItemsSource = null;
                PART_ListItemFeatures.ItemsSource = itemFeatures;
            }
        }

        private void PART_RemoveFeature_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            List<ItemFeature> itemFeatures = (List<ItemFeature>)PART_ListItemFeatures.ItemsSource;
            itemFeatures.RemoveAt(index);

            PART_ListItemFeatures.ItemsSource = null;
            PART_ListItemFeatures.ItemsSource = itemFeatures;
        }
        #endregion


        #region TagsToFeatures
        private void PART_AddTagsToFeatures_Click(object sender, RoutedEventArgs e)
        {
            LmTagToItemView ViewExtension = new LmTagToItemView(TypeItem.FeatureItem);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmTagsToFeatures"), ViewExtension);
            _ = windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null)
            {
                List<LmTagToFeature> temp = (List<LmTagToFeature>)PART_ListTagsToFeatures.ItemsSource;
                if (temp == null)
                {
                    temp = new List<LmTagToFeature>();
                }

                temp.Add((LmTagToFeature)ViewExtension.NewItem);

                PART_ListTagsToFeatures.ItemsSource = null;
                PART_ListTagsToFeatures.ItemsSource = temp;
            }
        }

        private void PART_RemoveTagsToFeatures_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());
            RemoveElement<LmTagToFeature>(PART_ListTagsToFeatures, index);
        }

        private void PART_SetTagsToFEatures_Click(object sender, RoutedEventArgs e)
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(Plugin, Settings);
            libraryManagementTools.SetTagsToFeatures();
        }
        #endregion


        #region TagsToGenres
        private void PART_AddTagsToGenres_Click(object sender, RoutedEventArgs e)
        {
            LmTagToItemView ViewExtension = new LmTagToItemView(TypeItem.GenreItem);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmTagsToGenres"), ViewExtension);
            _ = windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null)
            {
                List<LmTagToGenre> temp = (List<LmTagToGenre>)PART_ListTagsToGenres.ItemsSource;
                if (temp == null)
                {
                    temp = new List<LmTagToGenre>();
                }

                temp.Add((LmTagToGenre)ViewExtension.NewItem);

                PART_ListTagsToGenres.ItemsSource = null;
                PART_ListTagsToGenres.ItemsSource = temp;
            }
        }

        private void PART_RemoveTagsToGenres_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());
            RemoveElement<LmTagToGenre>(PART_ListTagsToGenres, index);
        }

        private void PART_SetTagsToGenres_Click(object sender, RoutedEventArgs e)
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(Plugin, Settings);
            libraryManagementTools.SetTagsToGenres();
        }
        #endregion


        #region #TagsToCategories
        private void PART_AddTagsToCategories_Click(object sender, RoutedEventArgs e)
        {
            LmTagToItemView ViewExtension = new LmTagToItemView(TypeItem.CategoryItem);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmTagsToCategories"), ViewExtension);
            _ = windowExtension.ShowDialog();

            if (ViewExtension.NewItem != null)
            {
                List<LmTagToCategory> temp = (List<LmTagToCategory>)PART_ListTagsToCategories.ItemsSource;
                if (temp == null)
                {
                    temp = new List<LmTagToCategory>();
                }

                temp.Add((LmTagToCategory)ViewExtension.NewItem);

                PART_ListTagsToCategories.ItemsSource = null;
                PART_ListTagsToCategories.ItemsSource = temp;
            }
        }

        private void PART_RemoveTagsToCategories_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());
            RemoveElement<LmTagToCategory>(PART_ListTagsToCategories, index);
        }

        private void PART_SetTagsToCategories_Click(object sender, RoutedEventArgs e)
        {
            LibraryManagementTools libraryManagementTools = new LibraryManagementTools(Plugin, Settings);
            libraryManagementTools.SetTagsToCategories();
        }
        #endregion


        #region AgeRatings
        private void PART_AddNewAgeRating_Click(object sender, RoutedEventArgs e)
        {
            AddEditNewAgeRating ViewExtension = new AddEditNewAgeRating(null);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmAddNewAgeRating"), ViewExtension);
            _ = windowExtension.ShowDialog();

            if (ViewExtension.AgeRating != null)
            {
                List<Models.AgeRating> ageRating = ((List<Models.AgeRating>)PART_ListAgeRatings.ItemsSource);
                ageRating.Add(ViewExtension.AgeRating);

                PART_ListAgeRatings.ItemsSource = null;
                PART_ListAgeRatings.ItemsSource = ageRating;
            }
        }

        private void PART_EditAgeRating_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            AddEditNewAgeRating ViewExtension = new AddEditNewAgeRating(((List<Models.AgeRating>)PART_ListAgeRatings.ItemsSource)[index]);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmEditAgeRating"), ViewExtension);
            _ = windowExtension.ShowDialog();

            if (ViewExtension.AgeRating != null)
            {
                List<Models.AgeRating> ageRating = ((List<Models.AgeRating>)PART_ListAgeRatings.ItemsSource);
                ageRating[index] = ViewExtension.AgeRating;

                PART_ListAgeRatings.ItemsSource = null;
                PART_ListAgeRatings.ItemsSource = ageRating;
            }
        }

        private void PART_RemoveAgeRating_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            List<Models.AgeRating> ageRating = ((List<Models.AgeRating>)PART_ListAgeRatings.ItemsSource);
            ageRating.RemoveAt(index);

            PART_ListAgeRatings.ItemsSource = null;
            PART_ListAgeRatings.ItemsSource = ageRating;
        }
        #endregion
    }
}
