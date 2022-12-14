using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using CommonPluginsShared.Models;
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
    /// Logique d'interaction pour AddEditNewAgeRating.xaml
    /// </summary>
    public partial class AddEditNewAgeRating : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private LibraryManagement plugin;
        public AgeRating ageRating;


        public AddEditNewAgeRating(LibraryManagement plugin, AgeRating ageRating = null)
        {
            InitializeComponent();

            ObservableCollection<CheckElement> checkElements = API.Instance.Database.AgeRatings?
                .Select(x => new CheckElement { Name = x.Name, Id = x.Id, IsCheck = ageRating?.AgeRatingIds?.Any(y => y == x.Id) ?? false, IsVisible = true })
                .OrderBy(x => x.Name)
                .ToObservable();

            PART_ListAgeRating.ItemsSource = checkElements;
            PART_Color.Background = new SolidColorBrush(Colors.SeaGreen);

            if (ageRating != null)
            {
                PART_NumericBox.LongValue = ageRating.Age;
                PART_Color.Background = ageRating.Color;
                PART_Save.IsEnabled = true;
            }

            PART_SelectorColorPicker.OnlySimpleColor = true;
        }


        private void PART_Save_Click(object sender, RoutedEventArgs e)
        {
            ageRating = new AgeRating
            {
                Age = Convert.ToInt32(PART_NumericBox.LongValue),
                AgeRatingIds = ((ObservableCollection<CheckElement>)PART_ListAgeRating.ItemsSource).Where(x => x.IsCheck).Select(x => x.Id).ToList(),
                Color = (SolidColorBrush)PART_Color.Background
            };

            ((Window)this.Parent).Close();
        }

        private void PART_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }


        private void PART_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((ObservableCollection<CheckElement>)PART_ListAgeRating.ItemsSource).ForEach(x => x.IsVisible = true);

            if (!PART_Search.Text.IsNullOrEmpty())
            {
                ((ObservableCollection<CheckElement>)PART_ListAgeRating.ItemsSource)
                    .Where(x => !x.Name.RemoveDiacritics().Contains(PART_Search.Text.RemoveDiacritics(), StringComparison.InvariantCultureIgnoreCase))
                    .ForEach(x => x.IsVisible = false);
            }
        }


        private void PART_TM_ColorOK_Click(object sender, RoutedEventArgs e)
        {
            Color color = default(Color);
            if (PART_SelectorColorPicker.IsSimpleColor)
            {
                color = PART_SelectorColorPicker.SimpleColor;
                PART_Color.Background = new SolidColorBrush(color);
            }

            PART_SelectorColor.Visibility = Visibility.Collapsed;
            spSettings.Visibility = Visibility.Visible;
        }

        private void PART_TM_ColorCancel_Click(object sender, RoutedEventArgs e)
        {
            PART_SelectorColor.Visibility = Visibility.Collapsed;
            spSettings.Visibility = Visibility.Visible;
        }

        private void BtPickColor_Click(object sender, RoutedEventArgs e)
        {
            Color color = ((SolidColorBrush)PART_Color.Background).Color;
            PART_SelectorColorPicker.SetColors(color);

            PART_SelectorColor.Visibility = Visibility.Visible;
            spSettings.Visibility = Visibility.Collapsed;
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            IsOk();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            IsOk();
        }

        private void IsOk()
        {
            PART_Save.IsEnabled = ((ObservableCollection<CheckElement>)PART_ListAgeRating.ItemsSource)?.Where(x => x.IsCheck)?.Count() > 0;
        }
    }
}
