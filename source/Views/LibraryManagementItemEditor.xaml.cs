using CommonPluginsShared.Extensions;
using LibraryManagement.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace LibraryManagement.Views
{
    public enum ItemType
    {
        Genre, Tag, Feature, Company
    }

    /// <summary>
    /// Logique d'interaction pour LibraryManagementItemEditor.xaml
    /// </summary>
    public partial class LibraryManagementItemEditor : UserControl
    {
        public object NewItem;

        public Guid? _Id;
        public ItemType _itemType;


        public LibraryManagementItemEditor(object data, ItemType itemType, Guid? Id = null, string NewName = "", string IconUnicode = "", List<string> ListAlreadyAdded = null)
        {
            _Id = Id;
            _itemType = itemType;

            InitializeComponent();

            List<ListItem> listItems = new List<ListItem>();
            if (data is List<Genre>)
            {
                foreach (var item in (List<Genre>)data)
                {
                    if (!item.Name.IsNullOrEmpty())
                    {
                        listItems.Add(new ListItem
                        {
                            Name = item.Name,
                            IsChecked = false
                        });
                    }
                }
            }
            if (data is List<GameFeature>)
            {
                foreach (var item in (List<GameFeature>)data)
                {
                    if (!item.Name.IsNullOrEmpty())
                    {
                        listItems.Add(new ListItem
                        {
                            Name = item.Name,
                            IsChecked = false
                        });
                    }
                }
            }
            if (data is List<Tag>)
            {
                foreach (var item in (List<Tag>)data)
                {
                    if (!item.Name.IsNullOrEmpty())
                    {
                        listItems.Add(new ListItem
                        {
                            Name = item.Name,
                            IsChecked = false
                        });
                    }
                }
            }
            if (data is List<Company>)
            {
                foreach (var item in (List<Company>)data)
                {
                    if (!item.Name.IsNullOrEmpty())
                    {
                        listItems.Add(new ListItem
                        {
                            Name = item.Name,
                            IsChecked = false
                        });
                    }
                }
            }

            if (ListAlreadyAdded != null)
            {
                foreach (var item in ListAlreadyAdded)
                {
                    if (!item.IsNullOrEmpty())
                    {
                        listItems.Add(new ListItem
                        {
                            Name = item,
                            IsChecked = true
                        });
                    }
                }
            }

            listItems.Sort((x, y) => x.Name.CompareTo(y.Name));
            PART_OldNames.ItemsSource = listItems.ToObservable();
            PART_NewName.Text = NewName;
            PART_IconUnicode.Text = IconUnicode;
        }


        public void OnlySimple()
        {
            PART_IconLabel.Visibility = Visibility.Collapsed;
            PART_IconContener.Visibility = Visibility.Collapsed;

            ObservableCollection<ListItem> listItems = (ObservableCollection<ListItem>)PART_OldNames.ItemsSource;
            foreach (var item in listItems)
            {
                item.OnlySimple = true;
            }
        }


        private void PART_Save_Click(object sender, RoutedEventArgs e)
        {
            List<string> OldNames = new List<string>();

            foreach(ListItem item in PART_OldNames.Items)
            {
                if (item.IsChecked)
                {
                    OldNames.Add(item.Name);
                }
            }

            string IconUnicode = PART_IconUnicode.Text;

            if (_itemType == ItemType.Genre)
            {
                NewItem = new LmGenreEquivalences
                {
                    Id = _Id,
                    Name = PART_NewName.Text,
                    IconUnicode = IconUnicode,
                    OldNames = OldNames
                };
            }

            if (_itemType == ItemType.Feature)
            {
                NewItem = new LmFeatureEquivalences
                {
                    Id = _Id,
                    Name = PART_NewName.Text,
                    IconUnicode = IconUnicode,
                    OldNames = OldNames
                };
            }

            if (_itemType == ItemType.Tag)
            {
                NewItem = new LmTagsEquivalences
                {
                    Id = _Id,
                    Name = PART_NewName.Text,
                    IconUnicode = IconUnicode,
                    OldNames = OldNames
                };
            }

            if (_itemType == ItemType.Company)
            {
                NewItem = new LmCompaniesEquivalences
                {
                    Id = _Id,
                    Name = PART_NewName.Text,
                    IconUnicode = IconUnicode,
                    OldNames = OldNames
                };
            }

            ((Window)this.Parent).Close();
        }

        private void PART_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }


        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)sender;
            Process.Start((string)link.Tag);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PART_NewName.Text = (string)((Button)sender).Tag;
        }


        private void PART_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((ObservableCollection<ListItem>)PART_OldNames.ItemsSource).ForEach(x => x.IsVisible = true);

            if (!PART_Search.Text.IsNullOrEmpty())
            {
                ((ObservableCollection<ListItem>)PART_OldNames.ItemsSource)
                    .Where(x => !x.Name.RemoveDiacritics().Contains(PART_Search.Text.RemoveDiacritics(), StringComparison.InvariantCultureIgnoreCase))
                    .ForEach(x => x.IsVisible = false);
            }
        }
    }

    public class ListItem : ObservableObject
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }
        public bool OnlySimple { get; set; }

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
