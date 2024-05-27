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
        public object NewItem { get; set; }

        private Guid? Id { get; set; }
        private ItemType ItemType { get; set; }


        public LibraryManagementItemEditor(object data, ItemType itemType, Guid? id = null, string newName = "", string iconUnicode = "", List<string> ListAlreadyAdded = null)
        {
            Id = id;
            ItemType = itemType;

            InitializeComponent();

            List<ListItem> listItems = new List<ListItem>();
            if (data is List<Genre>)
            {
                foreach (Genre item in (List<Genre>)data)
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
                foreach (GameFeature item in (List<GameFeature>)data)
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
                foreach (Tag item in (List<Tag>)data)
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
                foreach (Company item in (List<Company>)data)
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
                foreach (string item in ListAlreadyAdded)
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
            PART_NewName.Text = newName;
            PART_IconUnicode.Text = iconUnicode;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(PART_OldNames.ItemsSource);
            view.Filter = UserFilter;
        }


        public void OnlySimple()
        {
            PART_IconLabel.Visibility = Visibility.Collapsed;
            PART_IconContener.Visibility = Visibility.Collapsed;

            ObservableCollection<ListItem> listItems = (ObservableCollection<ListItem>)PART_OldNames.ItemsSource;
            foreach (ListItem item in listItems)
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

            if (ItemType == ItemType.Genre)
            {
                NewItem = new LmGenreEquivalences
                {
                    Id = Id,
                    Name = PART_NewName.Text,
                    IconUnicode = IconUnicode,
                    OldNames = OldNames
                };
            }

            if (ItemType == ItemType.Feature)
            {
                NewItem = new LmFeatureEquivalences
                {
                    Id = Id,
                    Name = PART_NewName.Text,
                    IconUnicode = IconUnicode,
                    OldNames = OldNames
                };
            }

            if (ItemType == ItemType.Tag)
            {
                NewItem = new LmTagsEquivalences
                {
                    Id = Id,
                    Name = PART_NewName.Text,
                    IconUnicode = IconUnicode,
                    OldNames = OldNames
                };
            }

            if (ItemType == ItemType.Company)
            {
                NewItem = new LmCompaniesEquivalences
                {
                    Id = Id,
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
            _ = Process.Start((string)link.Tag);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PART_NewName.Text = (string)((Button)sender).Tag;
        }


        private void PART_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(PART_OldNames.ItemsSource).Refresh();
        }
        private bool UserFilter(object item)
        {
            return (item as ListItem).Name.RemoveDiacritics().Contains(PART_Search.Text.RemoveDiacritics());
        }
    }

    public class ListItem : ObservableObject
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }
        public bool OnlySimple { get; set; }
    }
}
